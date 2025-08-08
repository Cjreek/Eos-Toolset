using Eos.Config;
using Eos.Models;
using Eos.Models.Tables;
using Eos.Nwn;
using Eos.Nwn.Bif;
using Eos.Nwn.Erf;
using Eos.Nwn.Ssf;
using Eos.Nwn.Tlk;
using Eos.Nwn.TwoDimensionalArray;
using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace Eos.Services
{
    public class CustomDataExport
    {
        private class TableData
        {
            public string Name { get; set; } = "";
            public string Filename { get; set; } = "";
            public TwoDimensionalArrayFile? Data { get; set; } = null;
        }

        private BifCollection ovrBif = new BifCollection();
        private TlkCollection dialogTlk = new TlkCollection();
        private Dictionary<Guid, TableData> tableDataDict = new Dictionary<Guid, TableData>();
        private Dictionary<Guid, int> modelIndices = new Dictionary<Guid, int>();
        private Dictionary<TLKStringSet, int> customTLKIndices = new Dictionary<TLKStringSet, int>();
        private Dictionary<(String resRef, NWNResourceType resType), String> hakResources = new Dictionary<(String, NWNResourceType), String>();
        private Dictionary<(String resRef, NWNResourceType resType), String> erfResources = new Dictionary<(String, NWNResourceType), String>();
        private HashSet<String> generatedConstants = new HashSet<string>();
        private Dictionary<BaseModel, String> modelScriptConstants = new Dictionary<BaseModel, string>();

        private HashSet<NWNResourceType> erfTypes = new HashSet<NWNResourceType>()
        {
            NWNResourceType.NSS,
            NWNResourceType.NCS,
            NWNResourceType.ITP, // Palette
            NWNResourceType.UTC, // Creatures
            NWNResourceType.UTI, // Items
            NWNResourceType.UTP, // Placeables
            NWNResourceType.UTD, // Doors
            NWNResourceType.UTM, // Merchants
            NWNResourceType.UTS, // Sounds
            NWNResourceType.UTT, // Triggers
            NWNResourceType.UTW, // Waypoints
            NWNResourceType.UTE, // Encounters
        };

        private void AddExtensionColumns(TwoDimensionalArrayFile twoDAFile, ModelExtension extensions)
        {
            foreach (var extension in extensions.Items)
            {
                if ((extension != null) && (extension.Column != "") && (!(extension.DataType?.IsVisualOnly ?? false)))
                    twoDAFile.Columns.AddColumn(extension.Column);
            }
        }

        private void WriteExtensionValues(LineRecord record, ObservableCollection<CustomValueInstance> values, bool writeLowerCase)
        {
            foreach (var value in values)
            {
                if ((!value.Property.DataType?.IsVisualOnly ?? false) && (value.Property.DataType?.To2DA != null))
                    record.Set(value.Property.Column, value.Property.DataType?.To2DA(value.Value, writeLowerCase, GetTLKIndex));
            }
        }

        private TwoDimensionalArrayFile? Load2da(String name)
        {
            var filename = Path.Combine(EosConfig.NwnBasePath, "ovr", name + ".2da");
            if (ovrBif.ContainsResource(name, NWNResourceType.TWODA))
            {
                var resource = ovrBif.ReadResource(name, NWNResourceType.TWODA);
                return new TwoDimensionalArrayFile(resource.RawData);
            }
            else if (File.Exists(filename))
            {
                using (var fs = File.OpenRead(filename))
                {
                    return new TwoDimensionalArrayFile(fs);
                }
            }
            else
            {
                var resource = MasterRepository.Resources.Get<Stream>(name, NWNResourceType.TWODA);
                if (resource == null) return null;
                return new TwoDimensionalArrayFile(resource);
            }
        }

        private void AddHAKResource(String name, NWNResourceType type, String filename)
        {
            hakResources[(name.ToLower(), type)] = filename;
        }

        private void AddERFResource(String name, NWNResourceType type, String filename)
        {
            erfResources[(name.ToLower(), type)] = filename;
        }

        private void AddTLKString(TLKStringSet tlk)
        {
            var isEmpty = true;
            foreach (var lang in Enum.GetValues<TLKLanguage>())
            {
                if ((tlk[lang].Text.Trim() != "") || (tlk[lang].TextF.Trim() != ""))
                {
                    isEmpty = false;
                    break;
                }
            }

            var isDifferent = true;
            if (((tlk.OriginalIndex ?? -1) >= 0) && ((tlk.OriginalIndex ?? -1) < 0x01000000))
            {
                isDifferent = false;
                foreach (var lang in Enum.GetValues<TLKLanguage>())
                {
                    var originalTextM = dialogTlk.GetString(lang, false, tlk.OriginalIndex);
                    var originalTextF = dialogTlk.GetString(lang, true, tlk.OriginalIndex);
                    if ((tlk[lang].Text.Trim() != originalTextM?.Trim()) || (tlk[lang].TextF.Trim() != originalTextF?.Trim()))
                    {
                        isDifferent = true;
                        break;
                    }
                }
            }

            if ((!isEmpty) && (isDifferent))
                customTLKIndices.Add(tlk, -1);
        }

        private int? GetTLKIndex(TLKStringSet tlkString)
        {
            if (!customTLKIndices.TryGetValue(tlkString, out int result))
                return tlkString.OriginalIndex;
            else
                return result + 0x01000000;
        }

        private void CollectTLKEntries(EosProject project)
        {
            customTLKIndices.Clear();

            // Races
            foreach (var race in project.Races)
            {
                if (race != null)
                {
                    AddTLKString(race.Name);
                    AddTLKString(race.NamePlural);
                    AddTLKString(race.Adjective);

                    race.AdjectiveLower.FromJson(race.Adjective.ToJson());
                    foreach (var lang in Enum.GetValues<TLKLanguage>())
                    {
                        race.AdjectiveLower[lang].Text = race.AdjectiveLower[lang].Text.ToLower();
                        race.AdjectiveLower[lang].TextF = race.AdjectiveLower[lang].TextF.ToLower();
                    }
                    AddTLKString(race.AdjectiveLower);

                    AddTLKString(race.Description);
                    AddTLKString(race.Biography);
                }
            }

            // Classes
            foreach (var cls in project.Classes)
            {
                if (cls != null)
                {
                    AddTLKString(cls.Name);
                    AddTLKString(cls.NamePlural);

                    cls.NameLower.FromJson(cls.Name.ToJson());
                    foreach (var lang in Enum.GetValues<TLKLanguage>())
                    {
                        cls.NameLower[lang].Text = cls.NameLower[lang].Text.ToLower();
                        cls.NameLower[lang].TextF = cls.NameLower[lang].TextF.ToLower();
                    }
                    AddTLKString(cls.NameLower);
                    AddTLKString(cls.Abbreviation);

                    AddTLKString(cls.Description);
                }
            }

            // Class packages
            foreach (var package in project.ClassPackages)
            {
                if (package != null)
                {
                    AddTLKString(package.Name);
                    AddTLKString(package.Description);
                }
            }

            // Diseases
            foreach (var disease in project.Diseases)
            {
                if (disease != null)
                {
                     AddTLKString(disease.Name);
                }
            }

            // Domains
            foreach (var domain in project.Domains)
            {
                if (domain != null)
                {
                    AddTLKString(domain.Name);
                    AddTLKString(domain.Description);
                }
            }

            // Feats
            foreach (var feat in project.Feats)
            {
                if (feat != null)
                {
                    AddTLKString(feat.Name);
                    AddTLKString(feat.Description);
                }
            }

            // MasterFeats
            foreach (var masterFeat in project.MasterFeats)
            {
                if (masterFeat != null)
                {
                    AddTLKString(masterFeat.Name);
                    AddTLKString(masterFeat.Description);
                }
            }

            // Poisons
            foreach (var poison in project.Poisons)
            {
                if (poison != null)
                {
                    AddTLKString(poison.Name);
                }
            }

            // Skills
            foreach (var skill in project.Skills)
            {
                if (skill != null)
                {
                    AddTLKString(skill.Name);
                    AddTLKString(skill.Description);
                }
            }

            // Soundsets
            foreach (var soundset in project.Soundsets)
            {
                if (soundset != null)
                {
                    AddTLKString(soundset.Name);
                    foreach (var entry in soundset.Entries)
                        AddTLKString(entry.Text);
                }
            }

            // Spells
            foreach (var spell in project.Spells)
            {
                if (spell != null)
                {
                    AddTLKString(spell.Name);
                    AddTLKString(spell.Description);
                    AddTLKString(spell.AlternativeCastMessage);
                }
            }

            // Appearances
            foreach (var appearance in project.Appearances)
            {
                if (appearance != null)
                {
                    AddTLKString(appearance.Name);
                }
            }

            // BaseItems
            foreach (var baseItem in project.BaseItems)
            {
                if (baseItem != null)
                {
                    AddTLKString(baseItem.Name);
                    AddTLKString(baseItem.Description);
                    AddTLKString(baseItem.StatsText);
                }
            }

            // Companions
            foreach (var companion in project.Companions)
            {
                if (companion != null)
                {
                    AddTLKString(companion.Name);
                    AddTLKString(companion.Description);
                }
            }

            // Familiars
            foreach (var familiar in project.Familiars)
            {
                if (familiar != null)
                {
                    AddTLKString(familiar.Name);
                    AddTLKString(familiar.Description);
                }
            }

            // Traps
            foreach (var trap in project.Traps)
            {
                if (trap != null)
                {
                    AddTLKString(trap.Name);
                }
            }

            // Damage Types
            foreach (var damageType in project.DamageTypes)
            {
                if (damageType != null)
                {
                    AddTLKString(damageType.Name);
                }
            }

            // Damage Type Groups
            foreach (var damageTypeGroup in project.DamageTypeGroups)
            {
                if (damageTypeGroup != null)
                {
                    AddTLKString(damageTypeGroup.FeedbackText);
                }
            }

            // Savingthrow Types
            foreach (var savingthrowType in project.SavingthrowTypes)
            {
                if (savingthrowType != null)
                {
                    AddTLKString(savingthrowType.Name);
                }
            }

            // Item Properties
            foreach (var itemProp in project.ItemProperties)
            {
                if (itemProp != null)
                {
                    AddTLKString(itemProp.Name);
                    AddTLKString(itemProp.PropertyText);
                    AddTLKString(itemProp.Description);
                }
            }

            // Item Property Tables
            foreach (var ipTable in project.ItemPropertyTables)
            {
                if (ipTable != null)
                {
                    foreach (var item in ipTable.Items)
                    {
                        if (item == null) continue;

                        AddTLKString(item.Name);
                        if (item.CustomColumnValue01.Value is TLKStringSet tlk01) AddTLKString(tlk01);
                        if (item.CustomColumnValue02.Value is TLKStringSet tlk02) AddTLKString(tlk02);
                        if (item.CustomColumnValue03.Value is TLKStringSet tlk03) AddTLKString(tlk03);
                        if (item.CustomColumnValue04.Value is TLKStringSet tlk04) AddTLKString(tlk04);
                        if (item.CustomColumnValue05.Value is TLKStringSet tlk05) AddTLKString(tlk05);
                        if (item.CustomColumnValue06.Value is TLKStringSet tlk06) AddTLKString(tlk06);
                        if (item.CustomColumnValue07.Value is TLKStringSet tlk07) AddTLKString(tlk07);
                        if (item.CustomColumnValue08.Value is TLKStringSet tlk08) AddTLKString(tlk08);
                        if (item.CustomColumnValue09.Value is TLKStringSet tlk09) AddTLKString(tlk09);
                        if (item.CustomColumnValue10.Value is TLKStringSet tlk10) AddTLKString(tlk10);
                    }
                }
            }

            // Item Property Cost Tables
            foreach (var ipcTable in project.ItemPropertyCostTables)
            {
                if (ipcTable != null)
                {
                    foreach (var item in ipcTable.Items)
                    {
                        if (item == null) continue;

                        AddTLKString(item.Name);
                        if (item.CustomColumnValue01.Value is TLKStringSet tlk01) AddTLKString(tlk01);
                        if (item.CustomColumnValue02.Value is TLKStringSet tlk02) AddTLKString(tlk02);
                        if (item.CustomColumnValue03.Value is TLKStringSet tlk03) AddTLKString(tlk03);
                        if (item.CustomColumnValue04.Value is TLKStringSet tlk04) AddTLKString(tlk04);
                        if (item.CustomColumnValue05.Value is TLKStringSet tlk05) AddTLKString(tlk05);
                        if (item.CustomColumnValue06.Value is TLKStringSet tlk06) AddTLKString(tlk06);
                        if (item.CustomColumnValue07.Value is TLKStringSet tlk07) AddTLKString(tlk07);
                        if (item.CustomColumnValue08.Value is TLKStringSet tlk08) AddTLKString(tlk08);
                        if (item.CustomColumnValue09.Value is TLKStringSet tlk09) AddTLKString(tlk09);
                        if (item.CustomColumnValue10.Value is TLKStringSet tlk10) AddTLKString(tlk10);
                    }
                }
            }

            // Item Property Params
            foreach (var ipParam in project.ItemPropertyParams)
            {
                if (ipParam != null)
                    AddTLKString(ipParam.Name);
            }

            // Custom Objects
            foreach (var co in project.CustomObjects)
            {
                if (co != null)
                {
                    var repo = project.CustomObjectRepositories[co];
                    foreach (var instance in repo)
                    {
                        if (instance == null) continue;
                        foreach (var val in instance.Values)
                        {
                            if (val.Value is TLKStringSet tlk)
                                AddTLKString(tlk);
                        }
                    }
                }
            }

            // Custom Dynamic Tables
            foreach (var cdt in project.CustomDynamicTables)
            {
                if (cdt != null)
                {
                    var repo = project.CustomDynamicTableRepositories[cdt];
                    foreach (var table in repo)
                    {
                        if (table == null) continue;

                        foreach (var item in table.Items)
                        {
                            if (item == null) continue;

                            if (item.CustomColumnValue01.Value is TLKStringSet tlk01) AddTLKString(tlk01);
                            if (item.CustomColumnValue02.Value is TLKStringSet tlk02) AddTLKString(tlk02);
                            if (item.CustomColumnValue03.Value is TLKStringSet tlk03) AddTLKString(tlk03);
                            if (item.CustomColumnValue04.Value is TLKStringSet tlk04) AddTLKString(tlk04);
                            if (item.CustomColumnValue05.Value is TLKStringSet tlk05) AddTLKString(tlk05);
                            if (item.CustomColumnValue06.Value is TLKStringSet tlk06) AddTLKString(tlk06);
                            if (item.CustomColumnValue07.Value is TLKStringSet tlk07) AddTLKString(tlk07);
                            if (item.CustomColumnValue08.Value is TLKStringSet tlk08) AddTLKString(tlk08);
                            if (item.CustomColumnValue09.Value is TLKStringSet tlk09) AddTLKString(tlk09);
                            if (item.CustomColumnValue10.Value is TLKStringSet tlk10) AddTLKString(tlk10);
                        }
                    }
                }
            }
        }

        private void ExportTLKs(EosProject project)
        {
            Log.Info("Exporting TLK: \"{0}\"", Path.GetFullPath(project.Settings.Export.TlkFolder + project.Name.ToLower().Replace(' ', '_') + ".tlk"));

            CollectTLKEntries(project);

            var tlkFile = new TlkFile();
            if ((project.Settings.Export.BaseTlkFile != "") && (File.Exists(project.Settings.Export.BaseTlkFile)))
                tlkFile.Load(project.Settings.Export.BaseTlkFile, true);
            else
                tlkFile.New(project.DefaultLanguage);

            tlkFile.PadTo(project.Settings.Export.TlkOffset);

            foreach (var tlk in customTLKIndices.Keys)
                customTLKIndices[tlk] = tlkFile.AddText(tlk[project.DefaultLanguage].Text.ReplaceLineEndings("\n"));

            tlkFile.Save(project.Settings.Export.TlkFolder + project.Name.ToLower().Replace(' ', '_') + ".tlk");
        }

        private void ExportAttackBonusTables(EosProject project)
        {
            foreach (var table in project.AttackBonusTables)
            {
                if (table != null)
                {
                    Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da"));

                    var td2 = new TwoDimensionalArrayFile();
                    td2.New("BAB");
                    foreach (var item in table.Items)
                    {
                        if (item != null)
                        {
                            var rec = td2.AddRecord();
                            rec.Set("BAB", item.AttackBonus);
                        }
                    }

                    var filename = project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da";
                    td2.Save(filename, project.Settings.Export.Compress2DA);

                    AddHAKResource(table.Name.ToLower(), NWNResourceType.TWODA, filename);
                }
            }
        }

        private void ExportBonusFeatsTables(EosProject project)
        {
            foreach (var table in project.BonusFeatTables)
            {
                if (table != null)
                {
                    Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da"));

                    var td2 = new TwoDimensionalArrayFile();
                    td2.New("Bonus");
                    foreach (var item in table.Items)
                    {
                        if (item != null)
                        {
                            var rec = td2.AddRecord();
                            rec.Set("Bonus", item.BonusFeatCount);
                        }
                    }

                    var filename = project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da";
                    td2.Save(filename, project.Settings.Export.Compress2DA);

                    AddHAKResource(table.Name.ToLower(), NWNResourceType.TWODA, filename);
                }
            }
        }

        private void ExportFeatsTables(EosProject project)
        {
            var columns = new String[]
            {
                "FeatLabel", "FeatIndex", "List", "GrantedOnLevel", "OnMenu"
            };

            foreach (var table in project.FeatTables)
            {
                if (table != null)
                {
                    Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da"));

                    var td2 = new TwoDimensionalArrayFile();
                    td2.New(columns);
                    foreach (var item in table.Items)
                    {
                        if (item != null)
                        {
                            var rec = td2.AddRecord();
                            rec.Set("FeatLabel", GetScriptConstant("FEAT_", item.Feat));
                            rec.Set("FeatIndex", project.Feats.Get2DAIndex(item?.Feat));
                            rec.Set("List", (int?)(item?.FeatList));
                            rec.Set("GrantedOnLevel", (int?)item?.GrantedOnLevel);
                            rec.Set("OnMenu", (int?)item?.Menu);
                        }
                    }

                    var filename = project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da";
                    td2.Save(filename, project.Settings.Export.Compress2DA);

                    AddHAKResource(table.Name.ToLower(), NWNResourceType.TWODA, filename);
                }
            }
        }

        private void ExportStatGainTables(EosProject project)
        {
            var columns = new String[]
            {
                "Level", "Str", "Dex", "Con", "Wis", "Int", "Cha", "NaturalAC"
            };

            foreach (var table in project.StatGainTables)
            {
                if (table != null)
                {
                    Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da"));

                    var td2 = new TwoDimensionalArrayFile();
                    td2.New(columns);
                    foreach (var item in table.Items)
                    {
                        if (item != null)
                        {
                            var rec = td2.AddRecord();
                            rec.Set("Level", item.Level);
                            rec.Set("Str", item.Strength);
                            rec.Set("Dex", item.Dexterity);
                            rec.Set("Con", item.Constitution);
                            rec.Set("Wis", item.Wisdom);
                            rec.Set("Int", item.Intelligence);
                            rec.Set("Cha", item.Charisma);
                            rec.Set("NaturalAC", item.NaturalAC);
                        }
                    }

                    var filename = project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da";
                    td2.Save(filename, project.Settings.Export.Compress2DA);

                    AddHAKResource(table.Name.ToLower(), NWNResourceType.TWODA, filename);
                }
            }
        }

        private void ExportKnownSpellsTables(EosProject project)
        {
            var columns = new String[]
            {
                "Level", "SpellLevel0", "SpellLevel1", "SpellLevel2", "SpellLevel3", "SpellLevel4",
                "SpellLevel5", "SpellLevel6", "SpellLevel7", "SpellLevel8", "SpellLevel9",
            };

            foreach (var table in project.KnownSpellsTables)
            {
                if (table != null)
                {
                    Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da"));

                    var td2 = new TwoDimensionalArrayFile();
                    td2.New(columns);
                    foreach (var item in table.Items)
                    {
                        if (item != null)
                        {
                            var rec = td2.AddRecord();
                            rec.Set("Level", item.Level);
                            rec.Set("SpellLevel0", item.SpellLevel0);
                            rec.Set("SpellLevel1", item.SpellLevel1);
                            rec.Set("SpellLevel2", item.SpellLevel2);
                            rec.Set("SpellLevel3", item.SpellLevel3);
                            rec.Set("SpellLevel4", item.SpellLevel4);
                            rec.Set("SpellLevel5", item.SpellLevel5);
                            rec.Set("SpellLevel6", item.SpellLevel6);
                            rec.Set("SpellLevel7", item.SpellLevel7);
                            rec.Set("SpellLevel8", item.SpellLevel8);
                            rec.Set("SpellLevel9", item.SpellLevel9);
                        }
                    }

                    var filename = project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da";
                    td2.Save(filename, project.Settings.Export.Compress2DA);

                    AddHAKResource(table.Name.ToLower(), NWNResourceType.TWODA, filename);
                }
            }
        }

        private void ExportPrerequisiteTables(EosProject project)
        {
            var columns = new String[]
            {
                "LABEL", "ReqType", "ReqParam1", "ReqParam2",
            };

            foreach (var table in project.PrerequisiteTables)
            {
                if (table != null)
                {
                    Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da"));

                    var td2 = new TwoDimensionalArrayFile();
                    td2.New(columns);
                    foreach (var item in table.Items)
                    {
                        if (item != null)
                        {
                            var rec = td2.AddRecord();
                            if (item.RequirementParam1 is BaseModel labelModel)
                                rec.Set("LABEL", labelModel);
                            switch (item.RequirementType)
                            {
                                case RequirementType.BAB:
                                    rec.Set("LABEL", "Base_Attack");
                                    break;

                                case RequirementType.ARCSPELL:
                                    rec.Set("LABEL", "Arcane");
                                    break;

                                case RequirementType.CLASSNOT:
                                    rec.Set("LABEL", ((CharacterClass?)item.RequirementParam1)?.Name[project.DefaultLanguage].Text.Replace(" ", ""));
                                    break;

                                case RequirementType.CLASSOR:
                                    rec.Set("LABEL", ((CharacterClass?)item.RequirementParam1)?.Name[project.DefaultLanguage].Text.Replace(" ", "") + "Or");
                                    break;

                                case RequirementType.RACE:
                                    rec.Set("LABEL", ((Race?)item.RequirementParam1)?.Name[project.DefaultLanguage].Text.Replace(" ", ""));
                                    break;

                                case RequirementType.FEAT:
                                case RequirementType.FEATOR:
                                    rec.Set("LABEL", ((Feat?)item.RequirementParam1)?.Name[project.DefaultLanguage].Text.Replace(" ", ""));
                                    break;

                                case RequirementType.SKILL:
#if SPACEPOPE
                                case RequirementType.SKILLOR:
#endif
                                    rec.Set("LABEL", ((Skill?)item.RequirementParam1)?.Name[project.DefaultLanguage].Text.Replace(" ", ""));
                                    break;

                                case RequirementType.VAR:
                                    rec.Set("LABEL", "ScriptVar");
                                    break;

#if SPACEPOPE
                                case RequirementType.ARCCAST:
                                    rec.Set("LABEL", "Arcane");
                                    break;

                                case RequirementType.DIVCAST:
                                case RequirementType.DIVSPELL:
                                    rec.Set("LABEL", "Divine");
                                    break;

                                case RequirementType.PANTHEONOR:
                                    rec.Set("LABEL", "Pantheon");
                                    break;

                                case RequirementType.DEITYOR:
                                    rec.Set("LABEL", "Deity");
                                    break;
#endif

                                default: 
                                    rec.Set("LABEL", "Label");
                                    break;
                            }

                            rec.Set("ReqType", item.RequirementType.ToString());
                            switch (item.RequirementType)
                            {
                                case RequirementType.CLASSNOT:
                                case RequirementType.CLASSOR:
                                    rec.Set("ReqParam1", project.Classes.Get2DAIndex((CharacterClass?)item.RequirementParam1));
                                    break;

                                case RequirementType.RACE:
                                    rec.Set("ReqParam1", project.Races.Get2DAIndex((Race?)item.RequirementParam1));
                                    break;

                                case RequirementType.FEAT:
                                case RequirementType.FEATOR:
                                    rec.Set("ReqParam1", project.Feats.Get2DAIndex((Feat?)item.RequirementParam1));
                                    break;

                                case RequirementType.SKILL:
                                    rec.Set("ReqParam1", project.Skills.Get2DAIndex((Skill?)item.RequirementParam1));
                                    break;

                                default:
                                    rec.Set("ReqParam1", item.RequirementParam1);
                                    break;
                            }

                            rec.Set("ReqParam2", item.RequirementParam2);
                        }
                    }

                    var filename = project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da";
                    td2.Save(filename, project.Settings.Export.Compress2DA);

                    AddHAKResource(table.Name.ToLower(), NWNResourceType.TWODA, filename);
                }
            }
        }

        private void ExportRacialFeatsTables(EosProject project)
        {
            var columns = new String[]
            {
                "FeatLabel", "FeatIndex",
            };

            foreach (var table in project.RacialFeatsTables)
            {
                if (table != null)
                {
                    Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da"));

                    var td2 = new TwoDimensionalArrayFile();
                    td2.New(columns);
                    foreach (var item in table.Items)
                    {
                        if (item != null)
                        {
                            var rec = td2.AddRecord();
                            rec.Set("FeatLabel", GetScriptConstant("FEAT_", item.Feat));
                            rec.Set("FeatIndex", project.Feats.Get2DAIndex(item?.Feat));
                        }
                    }

                    var filename = project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da";
                    td2.Save(filename, project.Settings.Export.Compress2DA);

                    AddHAKResource(table.Name.ToLower(), NWNResourceType.TWODA, filename);
                }
            }
        }

        private void ExportSavingThrowTables(EosProject project)
        {
            var columns = new String[]
            {
                "Level", "FortSave", "RefSave", "WillSave",
            };

            foreach (var table in project.SavingThrowTables)
            {
                if (table != null)
                {
                    Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da"));

                    var td2 = new TwoDimensionalArrayFile();
                    td2.New(columns);
                    foreach (var item in table.Items)
                    {
                        if (item != null)
                        {
                            var rec = td2.AddRecord();
                            rec.Set("Level", item.Level);
                            rec.Set("FortSave", item.FortitudeSave);
                            rec.Set("RefSave", item.ReflexSave);
                            rec.Set("WillSave", item.WillpowerSave);
                        }
                    }

                    var filename = project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da";
                    td2.Save(filename, project.Settings.Export.Compress2DA);

                    AddHAKResource(table.Name.ToLower(), NWNResourceType.TWODA, filename);
                }
            }
        }

        private void ExportSkillsTables(EosProject project)
        {
            var columns = new String[]
            {
                "SkillLabel", "SkillIndex", "ClassSkill",
            };

            foreach (var table in project.SkillTables)
            {
                if (table != null)
                {
                    Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da"));

                    var td2 = new TwoDimensionalArrayFile();
                    td2.New(columns);
                    td2.Columns.SetMaxLength("SkillLabel", project.Settings.Export.LabelMaxLength);
                    foreach (var item in table.Items)
                    {
                        if (item != null)
                        {
                            var rec = td2.AddRecord();
                            rec.Set("SkillLabel", MakeLabel(item.Skill?.Name[project.DefaultLanguage].Text, ""));
                            rec.Set("SkillIndex", project.Skills.Get2DAIndex(item.Skill));
                            rec.Set("ClassSkill", item.IsClassSkill ? 1 : 0);
                        }
                    }

                    var filename = project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da";
                    td2.Save(filename, project.Settings.Export.Compress2DA);

                    AddHAKResource(table.Name.ToLower(), NWNResourceType.TWODA, filename);
                }
            }
        }

        private void ExportSpellPreferencesTables(EosProject project)
        {
            var columns = new String[]
            {
                "SpellIndex", "Label",
            };

            foreach (var table in project.SpellPreferencesTables)
            {
                if (table != null)
                {
                    Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da"));

                    var td2 = new TwoDimensionalArrayFile();
                    td2.New(columns);
                    td2.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);
                    foreach (var item in table.Items)
                    {
                        if ((item != null) && (item.IsValid()))
                        {
                            var rec = td2.AddRecord();
                            rec.Set("SpellIndex", project.Spells.Get2DAIndex(item.Spell));
                            rec.Set("Label", MakeLabel(item.Spell?.Name[project.DefaultLanguage].Text, "_"));
                        }
                    }

                    var filename = project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da";
                    td2.Save(filename, project.Settings.Export.Compress2DA);

                    AddHAKResource(table.Name.ToLower(), NWNResourceType.TWODA, filename);
                }
            }
        }

        private void ExportFeatPreferencesTables(EosProject project)
        {
            var columns = new String[]
            {
                "FeatIndex", "Label",
            };

            foreach (var table in project.FeatPreferencesTables)
            {
                if (table != null)
                {
                    Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da"));

                    var td2 = new TwoDimensionalArrayFile();
                    td2.New(columns);
                    td2.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);
                    foreach (var item in table.Items)
                    {
                        if ((item != null) && (item.IsValid()))
                        {
                            var rec = td2.AddRecord();
                            rec.Set("FeatIndex", project.Feats.Get2DAIndex(item.Feat));
                            rec.Set("Label", MakeLabel(item.Feat?.Name[project.DefaultLanguage].Text, "_"));
                        }
                    }

                    var filename = project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da";
                    td2.Save(filename, project.Settings.Export.Compress2DA);

                    AddHAKResource(table.Name.ToLower(), NWNResourceType.TWODA, filename);
                }
            }
        }

        private void ExportSkillPreferencesTables(EosProject project)
        {
            var columns = new String[]
            {
                "SkillIndex", "Label",
            };

            foreach (var table in project.SkillPreferencesTables)
            {
                if (table != null)
                {
                    Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da"));

                    var td2 = new TwoDimensionalArrayFile();
                    td2.New(columns);
                    td2.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);
                    foreach (var item in table.Items)
                    {
                        if ((item != null) && (item.IsValid()))
                        {
                            var rec = td2.AddRecord();
                            rec.Set("SkillIndex", project.Skills.Get2DAIndex(item.Skill));
                            rec.Set("Label", MakeLabel(item.Skill?.Name[project.DefaultLanguage].Text, ""));
                        }
                    }

                    var filename = project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da";
                    td2.Save(filename, project.Settings.Export.Compress2DA);

                    AddHAKResource(table.Name.ToLower(), NWNResourceType.TWODA, filename);
                }
            }
        }

        private void ExportPackageEquipmentTables(EosProject project)
        {
            var columns = new String[]
            {
                "Label",
            };

            foreach (var table in project.PackageEquipmentTables)
            {
                if (table != null)
                {
                    Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da"));

                    var td2 = new TwoDimensionalArrayFile();
                    td2.New(columns);
                    foreach (var item in table.Items)
                    {
                        if ((item != null) && (item.IsValid()))
                        {
                            var rec = td2.AddRecord();
                            rec.Set("Label", item.BlueprintResRef);
                        }
                    }

                    var filename = project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da";
                    td2.Save(filename, project.Settings.Export.Compress2DA);

                    AddHAKResource(table.Name.ToLower(), NWNResourceType.TWODA, filename);
                }
            }
        }

        private int GetNumSpellLevels(SpellSlotTableItem spellSlotItem)
        {
            if (spellSlotItem.SpellLevel9 != null) return 10;
            if (spellSlotItem.SpellLevel8 != null) return 9;
            if (spellSlotItem.SpellLevel7 != null) return 8;
            if (spellSlotItem.SpellLevel6 != null) return 7;
            if (spellSlotItem.SpellLevel5 != null) return 6;
            if (spellSlotItem.SpellLevel4 != null) return 5;
            if (spellSlotItem.SpellLevel3 != null) return 4;
            if (spellSlotItem.SpellLevel2 != null) return 3;
            if (spellSlotItem.SpellLevel1 != null) return 2;
            if (spellSlotItem.SpellLevel0 != null) return 1;

            return 0;
        }

        private void ExportSpellSlotTables(EosProject project)
        {
            var columns = new String[]
            {
                "Level", "NumSpellLevels", "SpellLevel0", "SpellLevel1", "SpellLevel2", "SpellLevel3", "SpellLevel4",
                "SpellLevel5", "SpellLevel6", "SpellLevel7", "SpellLevel8", "SpellLevel9",
            };

            foreach (var table in project.SpellSlotTables)
            {
                if (table != null)
                {
                    Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da"));

                    var td2 = new TwoDimensionalArrayFile();
                    td2.New(columns);
                    foreach (var item in table.Items)
                    {
                        if (item != null)
                        {
                            var rec = td2.AddRecord();
                            rec.Set("Level", item.Level);
                            rec.Set("NumSpellLevels", GetNumSpellLevels(item));
                            rec.Set("SpellLevel0", item.SpellLevel0);
                            rec.Set("SpellLevel1", item.SpellLevel1);
                            rec.Set("SpellLevel2", item.SpellLevel2);
                            rec.Set("SpellLevel3", item.SpellLevel3);
                            rec.Set("SpellLevel4", item.SpellLevel4);
                            rec.Set("SpellLevel5", item.SpellLevel5);
                            rec.Set("SpellLevel6", item.SpellLevel6);
                            rec.Set("SpellLevel7", item.SpellLevel7);
                            rec.Set("SpellLevel8", item.SpellLevel8);
                            rec.Set("SpellLevel9", item.SpellLevel9);
                        }
                    }

                    var filename = project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da";
                    td2.Save(filename, project.Settings.Export.Compress2DA);

                    AddHAKResource(table.Name.ToLower(), NWNResourceType.TWODA, filename);
                }
            }
        }

        private void ExportClasses(EosProject project)
        {
            if (project.Classes.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "classes.2da"));

            var classes2da = Load2da("classes");
            if (classes2da != null)
            {
                classes2da.Columns.AddColumn("Short");
                classes2da.Columns.AddColumn("SkipSpellSelection");
                classes2da.Columns.SetHex("AlignRestrict");
                classes2da.Columns.SetHex("AlignRstrctType");

                if (project.Settings.Export.LowercaseFilenames)
                {
                    classes2da.Columns.SetLowercase("Icon");
                    classes2da.Columns.SetLowercase("AttackBonusTable");
                    classes2da.Columns.SetLowercase("FeatsTable");
                    classes2da.Columns.SetLowercase("SavingThrowTable");
                    classes2da.Columns.SetLowercase("SkillsTable");
                    classes2da.Columns.SetLowercase("BonusFeatsTable");
                    classes2da.Columns.SetLowercase("SpellGainTable");
                    classes2da.Columns.SetLowercase("SpellKnownTable");
                    classes2da.Columns.SetLowercase("PreReqTable");
                    classes2da.Columns.SetLowercase("StatGainTable");
                }

                classes2da.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);

                AddExtensionColumns(classes2da, project.Classes.Extensions);

                foreach (var cls in project.Classes.OrderBy(cls => cls?.Index))
                {
                    if (cls != null)
                    {
                        var index = -1;
                        if (cls.Overrides != null)
                            index = MasterRepository.Standard.Classes.GetByID(cls.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.Classes.GetCustomDataStartIndex() + cls.Index >= classes2da.Count)
                            {
                                classes2da.AddRecord();
                            }

                            index = project.Classes.GetCustomDataStartIndex() + (cls.Index ?? 0);
                        }

                        var record = classes2da[index];
                        record.Set("Label", MakeLabel(cls.Name[project.DefaultLanguage].Text, "_"));
                        record.Set("Short", GetTLKIndex(cls.Abbreviation));
                        record.Set("Name", GetTLKIndex(cls.Name));
                        record.Set("Plural", GetTLKIndex(cls.NamePlural));
                        record.Set("Lower", GetTLKIndex(cls.NameLower));
                        record.Set("Description", GetTLKIndex(cls.Description));
                        record.Set("Icon", cls.Icon);
                        record.Set("HitDie", cls.HitDie);
                        record.Set("AttackBonusTable", cls.AttackBonusTable?.Name);
                        record.Set("FeatsTable", cls.Feats?.Name);
                        record.Set("SavingThrowTable", cls.SavingThrows?.Name);
                        record.Set("SkillsTable", cls.Skills?.Name);
                        record.Set("BonusFeatsTable", cls.BonusFeats?.Name);
                        record.Set("SkillPointBase", cls.SkillPointsPerLevel);
                        record.Set("SpellGainTable", cls.SpellSlots?.Name);
                        record.Set("SpellKnownTable", cls.KnownSpells?.Name);
                        record.Set("PlayerClass", cls.Playable);
                        record.Set("SpellCaster", cls.IsSpellCaster);
                        record.Set("Str", cls.RecommendedStr);
                        record.Set("Dex", cls.RecommendedDex);
                        record.Set("Con", cls.RecommendedCon);
                        record.Set("Wis", cls.RecommendedWis);
                        record.Set("Int", cls.RecommendedInt);
                        record.Set("Cha", cls.RecommendedCha);
                        record.Set("PrimaryAbil", cls.PrimaryAbility.ToString());

                        AlignmentTuple? align = Alignments.Get2daAlignment(cls.AllowedAlignments);
                        record.Set("AlignRestrict", align?.Flags ?? 0);
                        record.Set("AlignRstrctType", align?.Axis ?? 0);
                        record.Set("InvertRestrict", align?.Inverted ?? false);

                        record.Set("Constant", GetScriptConstant("CLASS_TYPE_", cls));
                        record.Set("PreReqTable", cls.Requirements?.Name);
                        record.Set("MaxLevel", cls.MaxLevel > 60 ? 0 : cls.MaxLevel);
                        record.Set("XPPenalty", cls.MulticlassXPPenalty);
                        record.Set("ArcSpellLvlMod", cls.ArcaneCasterLevelMod);
                        record.Set("DivSpellLvlMod", cls.DivineCasterLevelMod);
                        record.Set("EpicLevel", cls.PreEpicMaxLevel > 60 ? -1 : cls.PreEpicMaxLevel);
                        record.Set("Package", project.ClassPackages.Get2DAIndex(cls.DefaultPackage));
                        record.Set("StatGainTable", cls.StatGainTable?.Name);
                        record.Set("MemorizesSpells", cls.IsSpellCaster ? cls.MemorizesSpells : null);
                        record.Set("SpellbookRestricted", cls.IsSpellCaster ? cls.SpellbookRestricted : null);
                        record.Set("PickDomains", cls.IsSpellCaster ? cls.PicksDomain : null);
                        record.Set("PickSchool", cls.IsSpellCaster ? cls.PicksSchool : null);
                        record.Set("LearnScroll", cls.IsSpellCaster ? cls.CanLearnFromScrolls : null);
                        record.Set("Arcane", cls.IsSpellCaster ? cls.IsArcaneCaster : null);
                        record.Set("ASF", cls.IsSpellCaster ? cls.HasSpellFailure : null);
                        record.Set("SpellcastingAbil", cls.IsSpellCaster ? cls.SpellcastingAbility.ToString() : null);
                        record.Set("SpellTableColumn", cls.Spellbook?.Name);
                        record.Set("CLMultiplier", cls.IsSpellCaster ? cls.CasterLevelMultiplier : null);
                        record.Set("MinCastingLevel", cls.IsSpellCaster ? cls.MinCastingLevel : null);
                        record.Set("MinAssociateLevel", cls.IsSpellCaster ? cls.MinAssociateLevel: null);
                        record.Set("CanCastSpontaneously", cls.IsSpellCaster ? cls.CanCastSpontaneously : null);
                        record.Set("SkipSpellSelection", cls.IsSpellCaster ? cls.SkipSpellSelection : null);

                        WriteExtensionValues(record, cls.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "classes.2da";
                classes2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("classes", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportClassPackages(EosProject project)
        {
            if (project.ClassPackages.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "packages.2da"));

            var packages2da = Load2da("packages");
            if (packages2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    packages2da.Columns.SetLowercase("SpellPref2DA");
                    packages2da.Columns.SetLowercase("FeatPref2DA");
                    packages2da.Columns.SetLowercase("SkillPref2DA");
                    packages2da.Columns.SetLowercase("Equip2DA");
                }

                packages2da.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);

                AddExtensionColumns(packages2da, project.ClassPackages.Extensions);

                foreach (var package in project.ClassPackages.OrderBy(package => package?.Index))
                {
                    if (package != null)
                    {
                        var index = -1;
                        if (package.Overrides != null)
                            index = MasterRepository.Standard.ClassPackages.GetByID(package.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.ClassPackages.GetCustomDataStartIndex() + package.Index >= packages2da.Count)
                            {
                                packages2da.AddRecord();
                            }

                            index = project.ClassPackages.GetCustomDataStartIndex() + (package.Index ?? 0);
                        }

                        var record = packages2da[index];
                        record.Set("Label", MakeLabel(package.Name[project.DefaultLanguage].Text, "_"));
                        record.Set("Name", GetTLKIndex(package.Name));
                        record.Set("Description", GetTLKIndex(package.Description));
                        record.Set("ClassID", project.Classes.Get2DAIndex(package.ForClass));
                        record.Set("Attribute", package.PreferredAbility.ToString());
                        record.Set("Gold", package.Gold);
                        record.Set("School", (int?)package.SpellSchool);
                        record.Set("Domain1", project.Domains.Get2DAIndex(package.Domain1));
                        record.Set("Domain2", project.Domains.Get2DAIndex(package.Domain1));

                        if (package.ForClass?.IsSpellCaster ?? false)
                        {
                            if (package.ForClass.IsArcaneCaster)
                                record.Set("Associate", project.Familiars.Get2DAIndex(package.AssociateFamiliar));
                            else
                                record.Set("Associate", project.Companions.Get2DAIndex(package.AssociateCompanion));
                        }

                        record.Set("SpellPref2DA", package.SpellPreferences?.Name); // TODO
                        record.Set("FeatPref2DA", package.FeatPreferences?.Name); // TODO
                        record.Set("SkillPref2DA", package.SkillPreferences?.Name); // TODO
                        record.Set("Equip2DA", package.StartingEquipment?.Name); // TODO
                        record.Set("Soundset", 0);
                        record.Set("PlayerClass", package.Playable);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "packages.2da";
                packages2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("packages", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportDiseases(EosProject project)
        {
            if (project.Diseases.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "disease.2da"));

            var disease2da = Load2da("disease");
            if (disease2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    disease2da.Columns.SetLowercase("End_Incu_Script");
                    disease2da.Columns.SetLowercase("24_Hour_Script");
                }

                disease2da.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);

                AddExtensionColumns(disease2da, project.Diseases.Extensions);
                foreach (var disease in project.Diseases.OrderBy(disease => disease?.Index))
                {
                    if (disease != null)
                    {
                        var index = -1;
                        if (disease.Overrides != null)
                            index = MasterRepository.Standard.Diseases.GetByID(disease.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.Diseases.GetCustomDataStartIndex() + disease.Index >= disease2da.Count)
                            {
                                disease2da.AddRecord();
                            }

                            index = project.Diseases.GetCustomDataStartIndex() + (disease.Index ?? 0);
                        }

                        var record = disease2da[index];
                        record.Set("Label", MakeLabel(disease.Name[project.DefaultLanguage].Text, "_"));
                        record.Set("Name", GetTLKIndex(disease.Name));
                        record.Set("First_Save", disease.FirstSaveDC);
                        record.Set("Subs_Save", disease.SecondSaveDC);
                        record.Set("Incu_Hours", disease.IncubationHours);
                        record.Set("Dice_1", disease.AbilityDamage1Dice);
                        record.Set("Dam_1", disease.AbilityDamage1DiceCount);
                        record.Set("Type_1", disease.AbilityDamage1Type?.ToString());
                        record.Set("Dice_2", disease.AbilityDamage2Dice);
                        record.Set("Dam_2", disease.AbilityDamage2DiceCount);
                        record.Set("Type_2", disease.AbilityDamage2Type?.ToString());
                        record.Set("Dice_3", disease.AbilityDamage3Dice);
                        record.Set("Dam_3", disease.AbilityDamage3DiceCount);
                        record.Set("Type_3", disease.AbilityDamage3Type?.ToString());
                        record.Set("Type", "EXTRA");
                        record.Set("End_Incu_Script", disease.IncubationEndScript);
                        record.Set("24_Hour_Script", disease.DailyEffectScript);

                        WriteExtensionValues(record, disease.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "disease.2da";
                disease2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("disease", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportDomains(EosProject project)
        {
            if (project.Domains.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "domains.2da"));

            var domains2da = Load2da("domains");
            if (domains2da != null)
            {
                domains2da.Columns.AddColumn("Level_0");

                if (project.Settings.Export.LowercaseFilenames)
                {
                    domains2da.Columns.SetLowercase("Icon");
                }

                domains2da.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);

                AddExtensionColumns(domains2da, project.Domains.Extensions);
                foreach (var domain in project.Domains.OrderBy(domain => domain?.Index))
                {
                    if (domain != null)
                    {
                        var index = -1;
                        if (domain.Overrides != null)
                            index = MasterRepository.Standard.Domains.GetByID(domain.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.Domains.GetCustomDataStartIndex() + domain.Index >= domains2da.Count)
                            {
                                domains2da.AddRecord();
                            }

                            index = project.Domains.GetCustomDataStartIndex() + (domain.Index ?? 0);
                        }

                        var record = domains2da[index];
                        record.Set("Label", MakeLabel(domain.Name[project.DefaultLanguage].Text, "_")?.ToUpper());
                        record.Set("Name", GetTLKIndex(domain.Name));
                        record.Set("Description", GetTLKIndex(domain.Description));
                        record.Set("Icon", domain.Icon);
                        record.Set("Level_0", project.Spells.Get2DAIndex(domain.Level0Spell));
                        record.Set("Level_1", project.Spells.Get2DAIndex(domain.Level1Spell));
                        record.Set("Level_2", project.Spells.Get2DAIndex(domain.Level2Spell));
                        record.Set("Level_3", project.Spells.Get2DAIndex(domain.Level3Spell));
                        record.Set("Level_4", project.Spells.Get2DAIndex(domain.Level4Spell));
                        record.Set("Level_5", project.Spells.Get2DAIndex(domain.Level5Spell));
                        record.Set("Level_6", project.Spells.Get2DAIndex(domain.Level6Spell));
                        record.Set("Level_7", project.Spells.Get2DAIndex(domain.Level7Spell));
                        record.Set("Level_8", project.Spells.Get2DAIndex(domain.Level8Spell));
                        record.Set("Level_9", project.Spells.Get2DAIndex(domain.Level9Spell));
                        record.Set("GrantedFeat", project.Feats.Get2DAIndex(domain.GrantedFeat));
                        record.Set("CastableFeat", domain.FeatIsActive);

                        WriteExtensionValues(record, domain.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "domains.2da";
                domains2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("domains", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportMasterFeats(EosProject project)
        {
            if (project.MasterFeats.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "masterfeats.2da"));

            var masterFeats2da = Load2da("masterfeats");
            if (masterFeats2da != null)
            {
                AddExtensionColumns(masterFeats2da, project.MasterFeats.Extensions);
                foreach (var masterFeat in project.MasterFeats.OrderBy(feat => feat?.Index))
                {
                    if (masterFeat != null)
                    {
                        var index = -1;
                        if (masterFeat.Overrides != null)
                            index = MasterRepository.Standard.MasterFeats.GetByID(masterFeat.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.MasterFeats.GetCustomDataStartIndex() + masterFeat.Index >= masterFeats2da.Count)
                            {
                                masterFeats2da.AddRecord();
                            }

                            index = project.MasterFeats.GetCustomDataStartIndex() + (masterFeat.Index ?? 0);
                        }

                        var record = masterFeats2da[index];
                        record.Set("STRREF", GetTLKIndex(masterFeat.Name));
                        record.Set("DESCRIPTION", GetTLKIndex(masterFeat.Description));
                        record.Set("ICON", masterFeat.Icon);

                        WriteExtensionValues(record, masterFeat.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "masterfeats.2da";
                masterFeats2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("masterfeats", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportFeats(EosProject project)
        {
            if (project.Feats.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "feat.2da"));

            var feat2da = Load2da("feat");
            if (feat2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    feat2da.Columns.SetLowercase("Icon");
                }

                feat2da.Columns.SetMaxLength("LABEL", project.Settings.Export.LabelMaxLength);

                AddExtensionColumns(feat2da, project.Feats.Extensions);
                foreach (var feat in project.Feats.OrderBy(feat => feat?.Index))
                {
                    if (feat != null)
                    {
                        var index = -1;
                        if (feat.Overrides != null)
                            index = MasterRepository.Standard.Feats.GetByID(feat.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.Feats.GetCustomDataStartIndex() + feat.Index >= feat2da.Count)
                            {
                                feat2da.AddRecord();
                            }

                            index = project.Feats.GetCustomDataStartIndex() + (feat.Index ?? 0);
                        }

                        var record = feat2da[index];
                        if (feat.Disabled)
                        {
                            record.Clear();
                            continue;
                        }

                        record.Set("LABEL", GetScriptConstant("FEAT_", feat));
                        record.Set("FEAT", GetTLKIndex(feat.Name));
                        record.Set("DESCRIPTION", GetTLKIndex(feat.Description));
                        record.Set("Icon", feat.Icon);
                        record.Set("MINATTACKBONUS", feat.MinAttackBonus > 0 ? feat.MinAttackBonus : null);
                        record.Set("MINSTR", feat.MinStr > 0 ? feat.MinStr : null);
                        record.Set("MINDEX", feat.MinDex > 0 ? feat.MinDex : null);
                        record.Set("MININT", feat.MinInt > 0 ? feat.MinInt : null);
                        record.Set("MINWIS", feat.MinWis > 0 ? feat.MinWis : null);
                        record.Set("MINCON", feat.MinCon > 0 ? feat.MinCon : null);
                        record.Set("MINCHA", feat.MinCha > 0 ? feat.MinCha : null);
                        record.Set("MINSPELLLVL", feat.MinSpellLevel > 0 ? feat.MinSpellLevel : null);
                        record.Set("PREREQFEAT1", project.Feats.Get2DAIndex(feat.RequiredFeat1));
                        record.Set("PREREQFEAT2", project.Feats.Get2DAIndex(feat.RequiredFeat2));
                        record.Set("GAINMULTIPLE", 0);
                        record.Set("EFFECTSSTACK", 0);
                        record.Set("ALLCLASSESCANUSE", feat.UseableByAllClasses);
                        record.Set("CATEGORY", (int?)feat.Category);
                        record.Set("MAXCR", feat.OnUseEffect?.InnateLevel * 2);
                        record.Set("SPELLID", project.Spells.Get2DAIndex(feat.OnUseEffect));
                        record.Set("SUCCESSOR", project.Feats.Get2DAIndex(feat.SuccessorFeat));
                        record.Set("CRValue", feat.CRModifier);
                        record.Set("USESPERDAY", feat.UsesPerDay != 0 ? feat.UsesPerDay : null);
                        record.Set("MASTERFEAT", project.MasterFeats.Get2DAIndex(feat.MasterFeat));
                        record.Set("TARGETSELF", (feat.TargetSelf ?? false) ? 1 : null);
                        record.Set("OrReqFeat0", project.Feats.Get2DAIndex(feat.RequiredFeatSelection1));
                        record.Set("OrReqFeat1", project.Feats.Get2DAIndex(feat.RequiredFeatSelection2));
                        record.Set("OrReqFeat2", project.Feats.Get2DAIndex(feat.RequiredFeatSelection3));
                        record.Set("OrReqFeat3", project.Feats.Get2DAIndex(feat.RequiredFeatSelection4));
                        record.Set("OrReqFeat4", project.Feats.Get2DAIndex(feat.RequiredFeatSelection5));
                        record.Set("REQSKILL", project.Skills.Get2DAIndex(feat.RequiredSkill1));
                        record.Set("ReqSkillMinRanks", feat.RequiredSkill1Minimum != 0 ? feat.RequiredSkill1Minimum : null);
                        record.Set("REQSKILL2", project.Skills.Get2DAIndex(feat.RequiredSkill2));
                        record.Set("ReqSkillMinRanks2", feat.RequiredSkill2Minimum != 0 ? feat.RequiredSkill2Minimum : null);
                        record.Set("Constant", GetScriptConstant("FEAT_", feat));
                        record.Set("TOOLSCATEGORIES", (int)feat.ToolsetCategory);
                        record.Set("HostileFeat", feat.OnUseEffect != null ? feat.IsHostile : null);
                        record.Set("MinLevel", feat.MinLevel > 1 ? feat.MinLevel : null);
                        record.Set("MinLevelClass", project.Classes.Get2DAIndex(feat.MinLevelClass));
                        record.Set("MaxLevel", feat.MaxLevel > 0 ? feat.MaxLevel : null);
                        record.Set("MinFortSave", feat.MinFortitudeSave > 0 ? feat.MinFortitudeSave : null);
                        record.Set("PreReqEpic", feat.RequiresEpic);
                        record.Set("ReqAction", feat.UseActionQueue);

                        WriteExtensionValues(record, feat.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "feat.2da";
                feat2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("feat", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportPoisons(EosProject project)
        {
            if (project.Poisons.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "poison.2da"));

            var poison2da = Load2da("poison");
            if (poison2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    poison2da.Columns.SetLowercase("Script_1");
                    poison2da.Columns.SetLowercase("Script_2");
                    poison2da.Columns.SetLowercase("VFX_Impact");
                }

                poison2da.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);

                AddExtensionColumns(poison2da, project.Poisons.Extensions);
                foreach (var poison in project.Poisons.OrderBy(poison => poison?.Index))
                {
                    if (poison != null)
                    {
                        var index = -1;
                        if (poison.Overrides != null)
                            index = MasterRepository.Standard.Poisons.GetByID(poison.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.Poisons.GetCustomDataStartIndex() + poison.Index >= poison2da.Count)
                            {
                                poison2da.AddRecord();
                            }

                            index = project.Poisons.GetCustomDataStartIndex() + (poison.Index ?? 0);
                        }

                        var record = poison2da[index];
                        record.Set("Label", MakeLabel(poison.Name[project.DefaultLanguage].Text, "_"));
                        record.Set("Name", GetTLKIndex(poison.Name));
                        record.Set("Save_DC", poison.SaveDC);
                        record.Set("Handle_DC", poison.HandleDC);
                        record.Set("Dice_1", poison.InitialAbilityDamageDice);
                        record.Set("Dam_1", poison.InitialAbilityDamageDiceCount);
                        record.Set("Default_1", poison.InitialAbilityDamageType?.ToString());
                        record.Set("Script_1", poison.InitialEffectScript);
                        record.Set("Dice_2", poison.SecondaryAbilityDamageDice);
                        record.Set("Dam_2", poison.SecondaryAbilityDamageDiceCount);
                        record.Set("Default_2", poison.SecondaryAbilityDamageType?.ToString());
                        record.Set("Script_2", poison.SecondaryEffectScript);
                        record.Set("Cost", poison.Cost);
                        record.Set("OnHitApplied", poison.OnHitApplied);
                        record.Set("VFX_Impact", poison.ImpactVFX);

                        WriteExtensionValues(record, poison.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "poison.2da";
                poison2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("poison", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportRaces(EosProject project)
        {
            if (project.Races.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "racialtypes.2da"));

            var racialtypes2da = Load2da("racialtypes");
            if (racialtypes2da != null)
            {
                racialtypes2da.Columns.AddColumn("FavoredEnemyFeat");

                if (project.Settings.Export.LowercaseFilenames)
                {
                    racialtypes2da.Columns.SetLowercase("Icon");
                    racialtypes2da.Columns.SetLowercase("NameGenTableA");
                    racialtypes2da.Columns.SetLowercase("NameGenTableB");
                }

                racialtypes2da.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);

                AddExtensionColumns(racialtypes2da, project.Races.Extensions);
                foreach (var race in project.Races.OrderBy(race => race?.Index))
                {
                    if (race != null)
                    {
                        var index = -1;
                        if (race.Overrides != null)
                            index = MasterRepository.Standard.Races.GetByID(race.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.Races.GetCustomDataStartIndex() + race.Index >= racialtypes2da.Count)
                            {
                                racialtypes2da.AddRecord();
                            }

                            index = project.Races.GetCustomDataStartIndex() + (race.Index ?? 0);
                        }

                        var record = racialtypes2da[index];
                        record.Set("Label", MakeLabel(race.Name[project.DefaultLanguage].Text, "_"));
                        record.Set("Abrev", race.Name[project.DefaultLanguage].Text.Substring(0, 2));
                        record.Set("Name", GetTLKIndex(race.Name));
                        record.Set("ConverName", GetTLKIndex(race.Adjective));
                        record.Set("ConverNameLower", GetTLKIndex(race.AdjectiveLower));
                        record.Set("NamePlural", GetTLKIndex(race.NamePlural));
                        record.Set("Description", GetTLKIndex(race.Description));
                        record.Set("Icon", race.Icon);
                        record.Set("Appearance", project.Appearances.Get2DAIndex(race.Appearance));
                        record.Set("StrAdjust", race.StrAdjustment);
                        record.Set("DexAdjust", race.DexAdjustment);
                        record.Set("IntAdjust", race.IntAdjustment);
                        record.Set("ChaAdjust", race.ChaAdjustment);
                        record.Set("WisAdjust", race.WisAdjustment);
                        record.Set("ConAdjust", race.ConAdjustment);
                        record.Set("Endurance", 0);
                        record.Set("Favored", project.Classes.Get2DAIndex(race.FavoredClass));
                        record.Set("FeatsTable", race.Feats?.Name.ToUpper());
                        record.Set("Biography", GetTLKIndex(race.Biography));
                        record.Set("PlayerRace", race.Playable);
                        record.Set("Constant", GetScriptConstant("RACIAL_TYPE_", race));
                        record.Set("AGE", race.DefaultAge);
                        record.Set("ToolsetDefaultClass", project.Classes.Get2DAIndex(race.ToolsetDefaultClass));
                        record.Set("CRModifier", race.CRModifier);
                        record.Set("NameGenTableA", race.NameGenTableA);
                        record.Set("NameGenTableB", race.NameGenTableB);
                        record.Set("ExtraFeatsAtFirstLevel", race.FirstLevelExtraFeats > 0 ? race.FirstLevelExtraFeats : null);
                        record.Set("ExtraSkillPointsPerLevel", race.ExtraSkillPointsPerLevel > 0 ? race.ExtraSkillPointsPerLevel : null);
                        record.Set("FirstLevelSkillPointsMultiplier", race.FirstLevelSkillPointsMultiplier);
                        record.Set("AbilitiesPointBuyNumber", race.FirstLevelAbilityPoints);
                        record.Set("NormalFeatEveryNthLevel", race.FeatEveryNthLevel);
                        record.Set("NumberNormalFeatsEveryNthLevel", race.FeatEveryNthLevelCount);
                        record.Set("SkillPointModifierAbility", race.SkillPointModifierAbility?.ToString());
                        record.Set("FavoredEnemyFeat", project.Feats.Get2DAIndex(race.FavoredEnemyFeat));

                        WriteExtensionValues(record, race.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "racialtypes.2da";
                racialtypes2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("racialtypes", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportSkills(EosProject project)
        {
            if (project.Skills.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "skills.2da"));

            var skills2da = Load2da("skills");
            if (skills2da != null)
            {
                skills2da.Columns.AddColumn("HideFromLevelUp");

                if (project.Settings.Export.LowercaseFilenames)
                {
                    skills2da.Columns.SetLowercase("Icon");
                }

                skills2da.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);

                AddExtensionColumns(skills2da, project.Skills.Extensions);
                foreach (var skill in project.Skills.OrderBy(skill => skill?.Index))
                {
                    if (skill != null)
                    {
                        var index = -1;
                        if (skill.Overrides != null)
                            index = MasterRepository.Standard.Skills.GetByID(skill.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.Skills.GetCustomDataStartIndex() + skill.Index >= skills2da.Count)
                            {
                                skills2da.AddRecord();
                            }

                            index = project.Skills.GetCustomDataStartIndex() + (skill.Index ?? 0);
                        }

                        var record = skills2da[index];
                        record.Set("Label", MakeLabel(skill.Name[project.DefaultLanguage].Text, ""));
                        record.Set("Name", GetTLKIndex(skill.Name));
                        record.Set("Description", GetTLKIndex(skill.Description));
                        record.Set("Icon", skill.Icon);
                        record.Set("Untrained", skill.CanUseUntrained);
                        record.Set("KeyAbility", skill.KeyAbility.ToString());
                        record.Set("ArmorCheckPenalty", skill.UseArmorPenalty);
                        record.Set("AllClassesCanUse", skill.AllClassesCanUse);
                        record.Set("Category", (int?)skill.AIBehaviour);
                        //record.Set("MaxCR", null);
                        record.Set("Constant", GetScriptConstant("SKILL_", skill));
                        record.Set("HostileSkill", skill.IsHostile);
                        record.Set("HideFromLevelUp", skill.HideFromLevelUp);

                        WriteExtensionValues(record, skill.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "skills.2da";
                skills2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("skills", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportAreaEffects(EosProject project)
        {
            if (project.AreaEffects.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "vfx_persistent.2da"));

            var aoe2da = Load2da("vfx_persistent");
            if (aoe2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    aoe2da.Columns.SetLowercase("ONENTER");
                    aoe2da.Columns.SetLowercase("ONEXIT");
                    aoe2da.Columns.SetLowercase("HEARTBEAT");
                    aoe2da.Columns.SetLowercase("MODEL01");
                    aoe2da.Columns.SetLowercase("MODEL02");
                    aoe2da.Columns.SetLowercase("MODEL03");
                    aoe2da.Columns.SetLowercase("SoundImpact");
                    aoe2da.Columns.SetLowercase("SoundDuration");
                    aoe2da.Columns.SetLowercase("SoundCessation");
                    aoe2da.Columns.SetLowercase("SoundOneShot");
                    aoe2da.Columns.SetLowercase("MODELMIN01");
                    aoe2da.Columns.SetLowercase("MODELMIN02");
                    aoe2da.Columns.SetLowercase("MODELMIN03");
                }

                AddExtensionColumns(aoe2da, project.AreaEffects.Extensions);
                foreach (var aoe in project.AreaEffects.OrderBy(aoe => aoe?.Index))
                {
                    if (aoe != null)
                    {
                        var index = -1;
                        if (aoe.Overrides != null)
                            index = MasterRepository.Standard.AreaEffects.GetByID(aoe.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.AreaEffects.GetCustomDataStartIndex() + aoe.Index >= aoe2da.Count)
                            {
                                aoe2da.AddRecord();
                            }

                            index = project.AreaEffects.GetCustomDataStartIndex() + (aoe.Index ?? 0);
                        }

                        var record = aoe2da[index];
                        record.Set("LABEL", aoe.Name);
                        record.Set("SHAPE", aoe.Shape.ToString());
                        record.Set("RADIUS", aoe.Radius);
                        record.Set("WIDTH", aoe.Width);
                        record.Set("LENGTH", aoe.Length);
                        record.Set("ONENTER", aoe.OnEnterScript);
                        record.Set("ONEXIT", aoe.OnExitScript);
                        record.Set("HEARTBEAT", aoe.OnHeartbeatScript);
                        record.Set("OrientWithGround", aoe.OrientWithGround);
                        record.Set("DurationVFX", project.VisualEffects.Get2DAIndex(aoe.VisualEffect));
                        record.Set("MODEL01", aoe.Model1);
                        record.Set("MODEL02", aoe.Model2);
                        record.Set("MODEL03", aoe.Model3);
                        record.Set("NUMACT01", aoe.Model1Amount);
                        record.Set("NUMACT02", aoe.Model2Amount);
                        record.Set("NUMACT03", aoe.Model3Amount);
                        record.Set("DURATION01", aoe.Model1Duration);
                        record.Set("DURATION02", aoe.Model2Duration);
                        record.Set("DURATION03", aoe.Model3Duration);
                        record.Set("EDGEWGHT01", aoe.Model1EdgeWeight);
                        record.Set("EDGEWGHT02", aoe.Model2EdgeWeight);
                        record.Set("EDGEWGHT03", aoe.Model3EdgeWeight);
                        record.Set("SoundImpact", aoe.ImpactSound);
                        record.Set("SoundDuration", aoe.LoopSound);
                        record.Set("SoundCessation", aoe.CessationSound);
                        record.Set("SoundOneShot", aoe.RandomSound);
                        record.Set("SoundOneShotPercentage", aoe.RandomSoundChance);
                        record.Set("MODELMIN01", aoe.LowQualityModel1);
                        record.Set("MODELMIN02", aoe.LowQualityModel2);
                        record.Set("MODELMIN03", aoe.LowQualityModel3);

                        WriteExtensionValues(record, aoe.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "vfx_persistent.2da";
                aoe2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("vfx_persistent", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportAppearanceSoundsets(EosProject project)
        {
            if (project.AppearanceSoundsets.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "appearancesndset.2da"));

            var appearancesndset2da = Load2da("appearancesndset");
            if (appearancesndset2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    appearancesndset2da.Columns.SetLowercase("Looping");
                    appearancesndset2da.Columns.SetLowercase("FallFwd");
                    appearancesndset2da.Columns.SetLowercase("FallBck");
                }

                appearancesndset2da.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);

                AddExtensionColumns(appearancesndset2da, project.AppearanceSoundsets.Extensions);
                foreach (var appearanceSoundset in project.AppearanceSoundsets.OrderBy(appearanceSoundset => appearanceSoundset?.Index))
                {
                    if (appearanceSoundset != null)
                    {
                        var index = -1;
                        if (appearanceSoundset.Overrides != null)
                            index = MasterRepository.Standard.AppearanceSoundsets.GetByID(appearanceSoundset.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.AppearanceSoundsets.GetCustomDataStartIndex() + appearanceSoundset.Index >= appearancesndset2da.Count)
                            {
                                appearancesndset2da.AddRecord();
                            }

                            index = project.AppearanceSoundsets.GetCustomDataStartIndex() + (appearanceSoundset.Index ?? 0);
                        }

                        var record = appearancesndset2da[index];
                        record.Set("Label", MakeLabel(appearanceSoundset.Name, "_"));
                        record.Set("ArmorType", appearanceSoundset.ArmorType?.ToString().ToLower());
                        record.Set("WeapTypeL", project.WeaponSounds.Get2DAIndex(appearanceSoundset.LeftAttack));
                        record.Set("WeapTypeR", project.WeaponSounds.Get2DAIndex(appearanceSoundset.RightAttack));
                        record.Set("WeapTypeS", project.WeaponSounds.Get2DAIndex(appearanceSoundset.StraightAttack));
                        record.Set("WeapTypeClsLw", project.WeaponSounds.Get2DAIndex(appearanceSoundset.LowCloseAttack));
                        record.Set("WeapTypeClsH", project.WeaponSounds.Get2DAIndex(appearanceSoundset.HighCloseAttack));
                        record.Set("WeapTypeRch", project.WeaponSounds.Get2DAIndex(appearanceSoundset.ReachAttack));
                        record.Set("MissIndex", project.WeaponSounds.Get2DAIndex(appearanceSoundset.Miss));
                        record.Set("Looping", appearanceSoundset.Looping);
                        record.Set("FallFwd", appearanceSoundset.FallForward);
                        record.Set("FallBck", appearanceSoundset.FallBackward);

                        WriteExtensionValues(record, appearanceSoundset.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "appearancesndset.2da";
                appearancesndset2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("appearancesndset", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportAppearances(EosProject project)
        {
            if (project.Appearances.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "appearance.2da"));

            var appearance2da = Load2da("appearance");
            if (appearance2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    //appearance2da.Columns.SetLowercase("RACE");
                    appearance2da.Columns.SetLowercase("PORTRAIT");
                }

                appearance2da.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);

                AddExtensionColumns(appearance2da, project.Appearances.Extensions);
                foreach (var appearance in project.Appearances.OrderBy(appearance => appearance?.Index))
                {
                    if (appearance != null)
                    {
                        var index = -1;
                        if (appearance.Overrides != null)
                            index = MasterRepository.Standard.Appearances.GetByID(appearance.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.Appearances.GetCustomDataStartIndex() + appearance.Index >= appearance2da.Count)
                            {
                                appearance2da.AddRecord();
                            }

                            index = project.Appearances.GetCustomDataStartIndex() + (appearance.Index ?? 0);
                        }

                        var record = appearance2da[index];
                        record.Set("Label", MakeLabel(appearance.Name[project.DefaultLanguage].Text, "_"));
                        record.Set("STRING_REF", GetTLKIndex(appearance.Name));
                        record.Set("NAME", appearance.Name[project.DefaultLanguage].Text.Replace(" ", "_"));
                        record.Set("RACE", appearance.RaceModel);
                        record.Set("ENVMAP", appearance.EnvironmentMap);
                        record.Set("BLOODCOLR", appearance.BloodColor.ToString());

                        var modelType = appearance.ModelType.ToString();
                        if (appearance.CanHaveWings) modelType += "W";
                        if (appearance.CanHaveTails) modelType += "T";
                        record.Set("MODELTYPE", modelType);

                        record.Set("WEAPONSCALE", appearance.WeaponScale);
                        record.Set("WING_TAIL_SCALE", appearance.WingTailScale);
                        record.Set("HELMET_SCALE_M", appearance.HelmetScaleMale);
                        record.Set("HELMET_SCALE_F", appearance.HelmetScaleFemale);
                        record.Set("MOVERATE", appearance.MovementRate.ToString());
                        record.Set("WALKDIST", appearance.WalkAnimationDistance);
                        record.Set("RUNDIST", appearance.RunAnimationDistance);
                        record.Set("PERSPACE", appearance.PersonalSpaceRadius);
                        record.Set("CREPERSPACE", appearance.CreaturePersonalSpaceRadius);
                        record.Set("HEIGHT", appearance.CameraHeight);
                        record.Set("HITDIST", appearance.HitDistance);
                        record.Set("PREFATCKDIST", appearance.PreferredAttackDistance);
                        record.Set("TARGETHEIGHT", appearance.TargetHeight.ToString());
                        record.Set("ABORTONPARRY", appearance.AbortAttackAnimationOnParry);
                        record.Set("RACIALTYPE", 0); // ?
                        record.Set("HASLEGS", appearance.HasLegs);
                        record.Set("HASARMS", appearance.HasArms);
                        record.Set("PORTRAIT", appearance.Portrait);
                        record.Set("SIZECATEGORY", (int)appearance.SizeCategory);
                        record.Set("PERCEPTIONDIST", (int)appearance.PerceptionRange);
                        record.Set("FOOTSTEPTYPE", (int)appearance.FootstepSound);
                        record.Set("SOUNDAPPTYPE", project.AppearanceSoundsets.Get2DAIndex(appearance.AppearanceSoundset));
                        record.Set("HEADTRACK", appearance.HeadTracking);
                        record.Set("HEAD_ARC_H", appearance.HorizontalHeadTrackingRange);
                        record.Set("HEAD_ARC_V", appearance.VerticalHeadTrackingRange);
                        record.Set("HEAD_NAME", appearance.ModelHeadNodeName);
                        record.Set("BODY_BAG", (int)appearance.BodyBag);
                        record.Set("TARGETABLE", appearance.Targetable);

                        WriteExtensionValues(record, appearance.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "appearance.2da";
                appearance2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("appearance", NWNResourceType.TWODA, filename);
            }
        }


        private void ExportWeaponSounds(EosProject project)
        {
            if (project.WeaponSounds.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "weaponsounds.2da"));

            var weaponsounds2da = Load2da("weaponsounds");
            if (weaponsounds2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    weaponsounds2da.Columns.SetLowercase("Leather0");
                    weaponsounds2da.Columns.SetLowercase("Leather1");
                    weaponsounds2da.Columns.SetLowercase("Chain0");
                    weaponsounds2da.Columns.SetLowercase("Chain1");
                    weaponsounds2da.Columns.SetLowercase("Plate0");
                    weaponsounds2da.Columns.SetLowercase("Plate1");
                    weaponsounds2da.Columns.SetLowercase("Stone0");
                    weaponsounds2da.Columns.SetLowercase("Stone1");
                    weaponsounds2da.Columns.SetLowercase("Wood0");
                    weaponsounds2da.Columns.SetLowercase("Wood1");
                    weaponsounds2da.Columns.SetLowercase("Chitin0");
                    weaponsounds2da.Columns.SetLowercase("Chitin1");
                    weaponsounds2da.Columns.SetLowercase("Scale0");
                    weaponsounds2da.Columns.SetLowercase("Scale1");
                    weaponsounds2da.Columns.SetLowercase("Ethereal0");
                    weaponsounds2da.Columns.SetLowercase("Ethereal1");
                    weaponsounds2da.Columns.SetLowercase("Crystal0");
                    weaponsounds2da.Columns.SetLowercase("Crystal1");
                    weaponsounds2da.Columns.SetLowercase("Miss0");
                    weaponsounds2da.Columns.SetLowercase("Miss1");
                    weaponsounds2da.Columns.SetLowercase("Parry0");
                    weaponsounds2da.Columns.SetLowercase("Critical0");
                }

                AddExtensionColumns(weaponsounds2da, project.WeaponSounds.Extensions);
                foreach (var weaponSound in project.WeaponSounds.OrderBy(weaponSound => weaponSound?.Index))
                {
                    if (weaponSound != null)
                    {
                        var index = -1;
                        if (weaponSound.Overrides != null)
                            index = MasterRepository.Standard.WeaponSounds.GetByID(weaponSound.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.WeaponSounds.GetCustomDataStartIndex() + weaponSound.Index >= weaponsounds2da.Count)
                            {
                                weaponsounds2da.AddRecord();
                            }

                            index = project.WeaponSounds.GetCustomDataStartIndex() + (weaponSound.Index ?? 0);
                        }

                        var record = weaponsounds2da[index];
                        record.Set("Label", weaponSound.Name);
                        record.Set("Leather0", weaponSound.Leather0);
                        record.Set("Leather1", weaponSound.Leather1);
                        record.Set("Chain0", weaponSound.Chain0);
                        record.Set("Chain1", weaponSound.Chain1);
                        record.Set("Plate0", weaponSound.Plate0);
                        record.Set("Plate1", weaponSound.Plate1);
                        record.Set("Stone0", weaponSound.Stone0);
                        record.Set("Stone1", weaponSound.Stone1);
                        record.Set("Wood0", weaponSound.Wood0);
                        record.Set("Wood1", weaponSound.Wood1);
                        record.Set("Chitin0", weaponSound.Chitin0);
                        record.Set("Chitin1", weaponSound.Chitin1);
                        record.Set("Scale0", weaponSound.Scale0);
                        record.Set("Scale1", weaponSound.Scale1);
                        record.Set("Ethereal0", weaponSound.Ethereal0);
                        record.Set("Ethereal1", weaponSound.Ethereal1);
                        record.Set("Crystal0", weaponSound.Crystal0);
                        record.Set("Crystal1", weaponSound.Crystal1);
                        record.Set("Miss0", weaponSound.Miss0);
                        record.Set("Miss1", weaponSound.Miss1);
                        record.Set("Parry0", weaponSound.Parry);
                        record.Set("Critical0", weaponSound.Critical);

                        WriteExtensionValues(record, weaponSound.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "weaponsounds.2da";
                weaponsounds2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("weaponsounds", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportInventorySounds(EosProject project)
        {
            if (project.InventorySounds.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "inventorysnds.2da"));

            var inventorysounds2da = Load2da("inventorysnds");
            if (inventorysounds2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    inventorysounds2da.Columns.SetLowercase("InventorySound");
                }

                AddExtensionColumns(inventorysounds2da, project.InventorySounds.Extensions);
                foreach (var inventorySound in project.InventorySounds.OrderBy(inventorySound => inventorySound?.Index))
                {
                    if (inventorySound != null)
                    {
                        var index = -1;
                        if (inventorySound.Overrides != null)
                            index = MasterRepository.Standard.InventorySounds.GetByID(inventorySound.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.InventorySounds.GetCustomDataStartIndex() + inventorySound.Index >= inventorysounds2da.Count)
                            {
                                inventorysounds2da.AddRecord();
                            }

                            index = project.InventorySounds.GetCustomDataStartIndex() + (inventorySound.Index ?? 0);
                        }

                        var record = inventorysounds2da[index];
                        record.Set("Label", inventorySound.Name);
                        record.Set("InventorySound", inventorySound.Sound);

                        WriteExtensionValues(record, inventorySound.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "inventorysnds.2da";
                inventorysounds2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("inventorysnds", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportPolymorphs(EosProject project)
        {
            if (project.Polymorphs.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "polymorph.2da"));

            var polymorph2da = Load2da("polymorph");
            if (polymorph2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    polymorph2da.Columns.SetLowercase("Portrait");
                    polymorph2da.Columns.SetLowercase("CreatureWeapon1");
                    polymorph2da.Columns.SetLowercase("CreatureWeapon2");
                    polymorph2da.Columns.SetLowercase("CreatureWeapon3");
                    polymorph2da.Columns.SetLowercase("HideItem");
                    polymorph2da.Columns.SetLowercase("EQUIPPED");
                }

                AddExtensionColumns(polymorph2da, project.Polymorphs.Extensions);
                foreach (var polymorph in project.Polymorphs.OrderBy(polymorph => polymorph?.Index))
                {
                    if (polymorph != null)
                    {
                        var index = -1;
                        if (polymorph.Overrides != null)
                            index = MasterRepository.Standard.Polymorphs.GetByID(polymorph.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.Polymorphs.GetCustomDataStartIndex() + polymorph.Index >= polymorph2da.Count)
                            {
                                polymorph2da.AddRecord();
                            }

                            index = project.Polymorphs.GetCustomDataStartIndex() + (polymorph.Index ?? 0);
                        }

                        var record = polymorph2da[index];
                        record.Set("Name", polymorph.Name);
                        record.Set("AppearanceType", project.Appearances.Get2DAIndex(polymorph.Appearance));
                        record.Set("RacialType", project.Races.Get2DAIndex(polymorph.RacialType));
                        record.Set("PortraitId", project.Portraits.Get2DAIndex(polymorph.Portrait));
                        record.Set("Portrait", polymorph.PortraitResRef);
                        record.Set("CreatureWeapon1", polymorph.CreatureWeapon1);
                        record.Set("CreatureWeapon2", polymorph.CreatureWeapon2);
                        record.Set("CreatureWeapon3", polymorph.CreatureWeapon3);
                        record.Set("HideItem", polymorph.HideItem);
                        record.Set("EQUIPPED", polymorph.MainHandItem);
                        record.Set("STR", polymorph.Strength);
                        record.Set("CON", polymorph.Constitution);
                        record.Set("DEX", polymorph.Dexterity);
                        record.Set("NATURALACBONUS", polymorph.NaturalACBonus != 0 ? polymorph.NaturalACBonus : null);
                        record.Set("HPBONUS", polymorph.HPBonus != 0 ? polymorph.HPBonus : null);
                        //record.Set("SoundSet", project.Soundsets.Get2DAIndex(polymorph.Soundset)); // Unused -> not written/changed
                        record.Set("SPELL1", project.Spells.Get2DAIndex(polymorph.Spell1));
                        record.Set("SPELL2", project.Spells.Get2DAIndex(polymorph.Spell2));
                        record.Set("SPELL3", project.Spells.Get2DAIndex(polymorph.Spell3));
                        record.Set("MergeW", polymorph.MergeWeapon ? polymorph.MergeWeapon : null);
                        record.Set("MergeI", polymorph.MergeAccessories ? polymorph.MergeAccessories : null);
                        record.Set("MergeA", polymorph.MergeArmor ? polymorph.MergeArmor : null);

                        WriteExtensionValues(record, polymorph.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "polymorph.2da";
                polymorph2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("polymorph", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportSoundsets(EosProject project)
        {
            if (project.Soundsets.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "soundset.2da"));

            var soundset2da = Load2da("soundset");
            if (soundset2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    soundset2da.Columns.SetLowercase("RESREF");
                }

                soundset2da.Columns.SetMaxLength("LABEL", project.Settings.Export.LabelMaxLength);

                foreach (var soundset in project.Soundsets.OrderBy(soundset => soundset?.Index))
                {
                    if (soundset != null)
                    {
                        var index = -1;
                        if (soundset.Overrides != null)
                            index = MasterRepository.Standard.Soundsets.GetByID(soundset.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.Soundsets.GetCustomDataStartIndex() + soundset.Index >= soundset2da.Count)
                            {
                                soundset2da.AddRecord();
                            }

                            index = project.Soundsets.GetCustomDataStartIndex() + (soundset.Index ?? 0);
                        }

                        var record = soundset2da[index];
                        record.Set("LABEL", MakeLabel(soundset.Name[project.DefaultLanguage].Text, ""));
                        record.Set("RESREF", soundset.SoundsetResource);
                        record.Set("STRREF", GetTLKIndex(soundset.Name));
                        record.Set("GENDER", (int)soundset.Gender);
                        record.Set("TYPE", (int)soundset.Type);

                        var ssfFile = new SsfFile();
                        for (int i = 0; i < soundset.Entries.Count; i++)
                            ssfFile.Set((SoundsetEntryType)i, GetTLKIndex(soundset.Entries[i].Text), soundset.Entries[i].SoundFile);
                        var ssfFilename = project.Settings.Export.SsfFolder + soundset.SoundsetResource + ".ssf";
                        ssfFile.Save(ssfFilename);

                        AddHAKResource(soundset.SoundsetResource, NWNResourceType.SSF, ssfFilename);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "soundset.2da";
                soundset2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("soundset", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportBaseItems(EosProject project)
        {
            if (project.BaseItems.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "baseitems.2da"));

            var baseitems2da = Load2da("baseitems");
            if (baseitems2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    baseitems2da.Columns.SetLowercase("DefaultModel");
                    baseitems2da.Columns.SetLowercase("DefaultIcon");
                }
                baseitems2da.Columns.SetHex("EquipableSlots");

                AddExtensionColumns(baseitems2da, project.BaseItems.Extensions);
                foreach (var baseItem in project.BaseItems.OrderBy(feat => feat?.Index))
                {
                    if (baseItem != null)
                    {
                        var index = -1;
                        if (baseItem.Overrides != null)
                            index = MasterRepository.Standard.BaseItems.GetByID(baseItem.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.BaseItems.GetCustomDataStartIndex() + baseItem.Index >= baseitems2da.Count)
                            {
                                baseitems2da.AddRecord();
                            }

                            index = project.BaseItems.GetCustomDataStartIndex() + (baseItem.Index ?? 0);
                        }

                        var record = baseitems2da[index];
                        record.Set("Name", GetTLKIndex(baseItem.Name));
                        record.Set("label", baseItem.Name[project.DefaultLanguage].Text.Replace(" ", ""));
                        record.Set("InvSlotWidth", baseItem.InventorySlotWidth);
                        record.Set("InvSlotHeight", baseItem.InventorySlotHeight);
                        record.Set("EquipableSlots", (int)baseItem.EquipableSlots);
                        record.Set("CanRotateIcon", baseItem.CanRotateIcon);
                        record.Set("ModelType", (int)baseItem.ModelType);
                        record.Set("ItemClass", baseItem.ItemModel);
                        record.Set("GenderSpecific", baseItem.GenderSpecific);
                        record.Set("DefaultModel", baseItem.DefaultModel);
                        record.Set("DefaultIcon", baseItem.Icon);
                        record.Set("Container", baseItem.IsContainer);
                        record.Set("WeaponWield", baseItem.WeaponWieldType == WeaponWieldType.Standard ? null : (int)baseItem.WeaponWieldType);
                        record.Set("WeaponType", (int?)baseItem.WeaponDamageType == null ? 0 : (int?)baseItem.WeaponDamageType);
                        record.Set("WeaponSize", (int?)baseItem.WeaponSize);
                        record.Set("RangedWeapon", project.BaseItems.Get2DAIndex(baseItem.AmmunitionBaseItem));
                        record.Set("PrefAttackDist", baseItem.PreferredAttackDistance);
                        record.Set("MinRange", baseItem.MinimumModelCount);
                        record.Set("MaxRange", baseItem.MaximumModelCount);
                        record.Set("NumDice", baseItem.DamageDiceCount);
                        record.Set("DieToRoll", baseItem.DamageDice);
                        record.Set("CritThreat", baseItem.CriticalThreatRange);
                        record.Set("CritHitMult", baseItem.CriticalMultiplier);
                        record.Set("Category", baseItem.Category == ItemCategory.None ? null : (int)baseItem.Category);
                        record.Set("BaseCost", baseItem.BaseCost);
                        record.Set("Stacking", baseItem.MaxStackSize);
                        record.Set("ItemMultiplier", baseItem.ItemCostMultiplier);
                        record.Set("Description", GetTLKIndex(baseItem.Description));
                        record.Set("InvSoundType", project.InventorySounds.Get2DAIndex(baseItem.InventorySound));
                        record.Set("MaxProps", baseItem.MaxSpellProperties);
                        record.Set("MinProps", baseItem.MinSpellProperties);
                        record.Set("PropColumn", project.ItemPropertySets.Get2DAIndex(baseItem.ItemPropertySet));
                        record.Set("StorePanel", (int?)baseItem.StorePanel);
                        record.Set("ReqFeat0", project.Feats.Get2DAIndex(baseItem.RequiredFeat1));
                        record.Set("ReqFeat1", project.Feats.Get2DAIndex(baseItem.RequiredFeat2));
                        record.Set("ReqFeat2", project.Feats.Get2DAIndex(baseItem.RequiredFeat3));
                        record.Set("ReqFeat3", project.Feats.Get2DAIndex(baseItem.RequiredFeat4));
                        record.Set("ReqFeat4", project.Feats.Get2DAIndex(baseItem.RequiredFeat5));
                        record.Set("AC_Enchant", (int?)baseItem.ArmorClassType);
                        record.Set("BaseAC", baseItem.BaseShieldAC);
                        record.Set("ArmorCheckPen", baseItem.ArmorCheckPenalty);
                        record.Set("BaseItemStatRef", GetTLKIndex(baseItem.StatsText));
                        record.Set("ChargesStarting", baseItem.DefaultChargeCount);
                        record.Set("RotateOnGround", (int)baseItem.GroundModelRotation);
                        record.Set("TenthLBS", (int)(baseItem.Weight * 10));
                        record.Set("WeaponMatType", project.WeaponSounds.Get2DAIndex(baseItem.WeaponSound));
                        record.Set("AmmunitionType", (int?)baseItem.AmmunitionType);
                        record.Set("QBBehaviour", baseItem.QuickbarBehaviour == QuickbarBehaviour.Default ? null : (int)baseItem.QuickbarBehaviour);
                        record.Set("ArcaneSpellFailure", baseItem.ArcaneSpellFailure);
                        record.Set("%AnimSlashL", baseItem.LeftSlashAnimationPercent);
                        record.Set("%AnimSlashR", baseItem.RightSlashAnimationPercent);
                        record.Set("%AnimSlashS", baseItem.StraightSlashAnimationPercent);
                        record.Set("StorePanelSort", baseItem.StorePanelOrder);
                        record.Set("ILRStackSize", baseItem.ItemLevelRestrictionStackSize);
                        record.Set("WeaponFocusFeat", project.Feats.Get2DAIndex(baseItem.WeaponFocusFeat));
                        record.Set("EpicWeaponFocusFeat", project.Feats.Get2DAIndex(baseItem.EpicWeaponFocusFeat));
                        record.Set("WeaponSpecializationFeat", project.Feats.Get2DAIndex(baseItem.WeaponSpecializationFeat));
                        record.Set("EpicWeaponSpecializationFeat", project.Feats.Get2DAIndex(baseItem.EpicWeaponSpecializationFeat));
                        record.Set("WeaponImprovedCriticalFeat", project.Feats.Get2DAIndex(baseItem.ImprovedCriticalFeat));
                        record.Set("EpicWeaponOverwhelmingCriticalFeat", project.Feats.Get2DAIndex(baseItem.OverwhelmingCriticalFeat));
                        record.Set("EpicWeaponDevastatingCriticalFeat", project.Feats.Get2DAIndex(baseItem.DevastatingCriticalFeat));
                        record.Set("WeaponOfChoiceFeat", project.Feats.Get2DAIndex(baseItem.WeaponOfChoiceFeat));
                        record.Set("IsMonkWeapon", baseItem.IsMonkWeapon ? true : null);
                        record.Set("WeaponFinesseMinimumCreatureSize", (int?)baseItem.WeaponFinesseMinimumCreatureSize);

                        WriteExtensionValues(record, baseItem.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "baseitems.2da";
                baseitems2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("baseitems", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportCompanions(EosProject project)
        {
            if (project.Companions.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "hen_companion.2da"));

            var companions2da = Load2da("hen_companion");
            if (companions2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    companions2da.Columns.SetLowercase("BASERESREF");
                }

                companions2da.Columns.SetMaxLength("NAME", project.Settings.Export.LabelMaxLength);

                AddExtensionColumns(companions2da, project.Companions.Extensions);
                foreach (var companion in project.Companions.OrderBy(companion => companion?.Index))
                {
                    if (companion != null)
                    {
                        var index = -1;
                        if (companion.Overrides != null)
                            index = MasterRepository.Standard.Companions.GetByID(companion.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.Companions.GetCustomDataStartIndex() + companion.Index >= companions2da.Count)
                            {
                                companions2da.AddRecord();
                            }

                            index = project.Companions.GetCustomDataStartIndex() + (companion.Index ?? 0);
                        }

                        var record = companions2da[index];
                        record.Set("NAME", MakeLabel(companion.Name[project.DefaultLanguage].Text, "_"));
                        record.Set("BASERESREF", companion.Template);
                        record.Set("STRREF", GetTLKIndex(companion.Name));
                        record.Set("DESCRIPTION", GetTLKIndex(companion.Description));

                        WriteExtensionValues(record, companion.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "hen_companion.2da";
                companions2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("hen_companion", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportFamiliars(EosProject project)
        {
            if (project.Familiars.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "hen_familiar.2da"));

            var familiars2da = Load2da("hen_familiar");
            if (familiars2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    familiars2da.Columns.SetLowercase("BASERESREF");
                }

                familiars2da.Columns.SetMaxLength("NAME", project.Settings.Export.LabelMaxLength);

                AddExtensionColumns(familiars2da, project.Familiars.Extensions);
                foreach (var familiar in project.Familiars.OrderBy(familiar => familiar?.Index))
                {
                    if (familiar != null)
                    {
                        var index = -1;
                        if (familiar.Overrides != null)
                            index = MasterRepository.Standard.Familiars.GetByID(familiar.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.Familiars.GetCustomDataStartIndex() + familiar.Index >= familiars2da.Count)
                            {
                                familiars2da.AddRecord();
                            }

                            index = project.Familiars.GetCustomDataStartIndex() + (familiar.Index ?? 0);
                        }

                        var record = familiars2da[index];
                        record.Set("NAME", MakeLabel(familiar.Name[project.DefaultLanguage].Text, "_"));
                        record.Set("BASERESREF", familiar.Template);
                        record.Set("STRREF", GetTLKIndex(familiar.Name));
                        record.Set("DESCRIPTION", GetTLKIndex(familiar.Description));

                        WriteExtensionValues(record, familiar.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "hen_familiar.2da";
                familiars2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("hen_familiar", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportTraps(EosProject project)
        {
            if (project.Traps.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "traps.2da"));

            var traps2da = Load2da("traps");
            if (traps2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    traps2da.Columns.SetLowercase("TrapScript");
                    traps2da.Columns.SetLowercase("ResRef");
                    traps2da.Columns.SetLowercase("IconResRef");
                }

                traps2da.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);

                AddExtensionColumns(traps2da, project.Traps.Extensions);
                foreach (var trap in project.Traps.OrderBy(trap => trap?.Index))
                {
                    if (trap != null)
                    {
                        var index = -1;
                        if (trap.Overrides != null)
                            index = MasterRepository.Standard.Traps.GetByID(trap.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.Traps.GetCustomDataStartIndex() + trap.Index >= traps2da.Count)
                            {
                                traps2da.AddRecord();
                            }

                            index = project.Traps.GetCustomDataStartIndex() + (trap.Index ?? 0);
                        }

                        var record = traps2da[index];
                        record.Set("Label", MakeLabel(trap.Name[project.DefaultLanguage].Text, ""));
                        record.Set("TrapScript", trap.TrapScript);
                        record.Set("SetDC", trap.SetDC);
                        record.Set("DetectDCMod", trap.DetectDC);
                        record.Set("DisarmDCMod", trap.DisarmDC);
                        record.Set("TrapName", GetTLKIndex(trap.Name));
                        record.Set("ResRef", trap.BlueprintResRef);
                        record.Set("IconResRef", trap.Icon);

                        WriteExtensionValues(record, trap.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "traps.2da";
                traps2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("traps", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportVisualEffects(EosProject project)
        {
            if (project.VisualEffects.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "visualeffects.2da"));

            var vfx2da = Load2da("visualeffects");
            if (vfx2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    vfx2da.Columns.SetLowercase("Imp_HeadCon_Node");
                    vfx2da.Columns.SetLowercase("Imp_Impact_Node");
                    vfx2da.Columns.SetLowercase("Imp_Root_S_Node");
                    vfx2da.Columns.SetLowercase("Imp_Root_M_Node");
                    vfx2da.Columns.SetLowercase("Imp_Root_L_Node");
                    vfx2da.Columns.SetLowercase("Imp_Root_H_Node");
                    vfx2da.Columns.SetLowercase("SoundImpact");
                    vfx2da.Columns.SetLowercase("SoundDuration");
                    vfx2da.Columns.SetLowercase("SoundCessastion");
                    vfx2da.Columns.SetLowercase("LowViolence");
                    vfx2da.Columns.SetLowercase("LowQuality");
                }
                
                // vfx2da.Columns.RenameColumn("SoundCessastion", "SoundCessation"); // Fix, but would make bad sounds, so keept it for now
                vfx2da.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);

                AddExtensionColumns(vfx2da, project.VisualEffects.Extensions);
                foreach (var vfx in project.VisualEffects.OrderBy(vfx => vfx?.Index))
                {
                    if (vfx != null)
                    {
                        var index = -1;
                        if (vfx.Overrides != null)
                            index = MasterRepository.Standard.VisualEffects.GetByID(vfx.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.VisualEffects.GetCustomDataStartIndex() + vfx.Index >= vfx2da.Count)
                            {
                                vfx2da.AddRecord();
                            }

                            index = project.VisualEffects.GetCustomDataStartIndex() + (vfx.Index ?? 0);
                        }

                        var record = vfx2da[index];
                        record.Set("Label", MakeLabel(vfx.Name, "_"));
                        record.Set("Type_FD", vfx.Type.ToString());
                        record.Set("OrientWithGround", vfx.OrientWithGround);
                        record.Set("Imp_HeadCon_Node", vfx.ImpactHeadEffect);
                        record.Set("Imp_Impact_Node", vfx.ImpactImpactEffect);
                        record.Set("Imp_Root_S_Node", vfx.ImpactRootSmallEffect);
                        record.Set("Imp_Root_M_Node", vfx.ImpactRootMediumEffect);
                        record.Set("Imp_Root_L_Node", vfx.ImpactRootLargeEffect);
                        record.Set("Imp_Root_H_Node", vfx.ImpactRootHugeEffect);
                        record.Set("ProgFX_Impact", project.ProgrammedEffects.Get2DAIndex(vfx.ImpactProgFX));
                        record.Set("SoundImpact", vfx.ImpactSound);
                        record.Set("ProgFX_Duration", project.ProgrammedEffects.Get2DAIndex(vfx.DurationProgFX));
                        record.Set("SoundDuration", vfx.DurationSound);
                        record.Set("ProgFX_Cessation", project.ProgrammedEffects.Get2DAIndex(vfx.CessationProgFX));
                        record.Set("SoundCessation", vfx.CessationSound);
                        record.Set("ShakeType", vfx.ShakeType == VFXShakeType.None ? null : (int)vfx.ShakeType);
                        record.Set("ShakeDelay", vfx.ShakeDelay);
                        record.Set("ShakeDuration", vfx.ShakeDuration);
                        record.Set("LowViolence", vfx.LowViolenceModel);
                        record.Set("LowQuality", vfx.LowQualityModel);
                        record.Set("OrientWithObject", vfx.OrientWithObject);

                        WriteExtensionValues(record, vfx.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "visualeffects.2da";
                vfx2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("visualeffects", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportProgrammedEffects(EosProject project)
        {
            if (project.ProgrammedEffects.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "progfx.2da"));

            var progFX2da = Load2da("progfx");
            if (progFX2da != null)
            {
                var exportLowercase = project.Settings.Export.LowercaseFilenames;

                progFX2da.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);

                AddExtensionColumns(progFX2da, project.ProgrammedEffects.Extensions);
                foreach (var progFX in project.ProgrammedEffects.OrderBy(progFX => progFX?.Index))
                {
                    if (progFX != null)
                    {
                        var index = -1;
                        if (progFX.Overrides != null)
                            index = MasterRepository.Standard.ProgrammedEffects.GetByID(progFX.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.ProgrammedEffects.GetCustomDataStartIndex() + progFX.Index >= progFX2da.Count)
                            {
                                progFX2da.AddRecord();
                            }

                            index = project.ProgrammedEffects.GetCustomDataStartIndex() + (progFX.Index ?? 0);
                        }

                        var record = progFX2da[index];
                        record.Set("Label", MakeLabel(progFX.Name, ""));
                        record.Set("Type", progFX.Type == ProgrammedEffectType.Invalid ? null : (int)progFX.Type);
                        switch (progFX.Type)
                        {
                            case ProgrammedEffectType.SkinOverlay:
                                record.Set("Param1", exportLowercase ? progFX.T1ModelName.ToLower() : progFX.T1ModelName);
                                record.Set("Param2", progFX.T1ArmorType.ToString().ToLower());
                                record.Set("Param3", project.VisualEffects.Get2DAIndex(progFX.T1OnHitVFX));
                                record.Set("Param4", project.VisualEffects.Get2DAIndex(progFX.T1OnHitVFXSmall));
                                break;

                            case ProgrammedEffectType.EnvironmentMapping:
                                record.Set("Param1", exportLowercase ? progFX.T2EnvironmentMap.ToLower() : progFX.T2EnvironmentMap);
                                break;

                            case ProgrammedEffectType.GlowEffect:
                                var glowR = (progFX.T3GlowColor >> 16) & 0xFF;
                                var glowG = (progFX.T3GlowColor >> 8)  & 0xFF;
                                var glowB = (progFX.T3GlowColor)       & 0xFF;
                                record.Set("Param1", Math.Round(glowR / 255.0, 2));
                                record.Set("Param2", Math.Round(glowG / 255.0, 2));
                                record.Set("Param3", Math.Round(glowB / 255.0, 2));
                                break;

                            case ProgrammedEffectType.Lighting:
                                record.Set("Param1", progFX.T4LightModelAnimation);
                                record.Set("Param2", progFX.T4AnimationSpeed);
                                record.Set("Param3", progFX.T4CastShadows);
                                record.Set("Param4", progFX.T4Priority);
                                record.Set("Param5", progFX.T4RemoveCloseToOtherLights);
                                record.Set("Param6", progFX.T4RemoveAllOtherLights);
                                record.Set("Param7", exportLowercase ? progFX.T4LightModel.ToLower() : progFX.T4LightModel);
                                break;

                            case ProgrammedEffectType.AlphaTransparency:
                                record.Set("Param1", progFX.T5OpacityFrom);
                                var alphaR = (progFX.T5TransparencyColor >> 16) & 0xFF;
                                var alphaG = (progFX.T5TransparencyColor >> 8) & 0xFF;
                                var alphaB = (progFX.T5TransparencyColor) & 0xFF;
                                
                                if (progFX.T5TransparencyColorKeepRed)
                                    record.Set("Param2", -1);
                                else
                                    record.Set("Param2", Math.Round(alphaR / 255.0, 2));

                                if (progFX.T5TransparencyColorKeepGreen)
                                    record.Set("Param3", -1);
                                else
                                    record.Set("Param3", Math.Round(alphaG / 255.0, 2));

                                if (progFX.T5TransparencyColorKeepBlue)
                                    record.Set("Param4", -1);
                                else
                                    record.Set("Param4", Math.Round(alphaB / 255.0, 2));

                                record.Set("Param5", progFX.T5FadeInterval);
                                record.Set("Param6", progFX.T5OpacityTo);
                                break;

                            case ProgrammedEffectType.PulsingAura:
                                var aura1R = (progFX.T6Color1 >> 16) & 0xFF;
                                var aura1G = (progFX.T6Color1 >> 8) & 0xFF;
                                var aura1B = (progFX.T6Color1) & 0xFF;
                                record.Set("Param1", Math.Round(aura1R / 255.0, 2));
                                record.Set("Param2", Math.Round(aura1G / 255.0, 2));
                                record.Set("Param3", Math.Round(aura1B / 255.0, 2));
                                var aura2R = (progFX.T6Color2 >> 16) & 0xFF;
                                var aura2G = (progFX.T6Color2 >> 8) & 0xFF;
                                var aura2B = (progFX.T6Color2) & 0xFF;
                                record.Set("Param4", Math.Round(aura2R / 255.0, 2));
                                record.Set("Param5", Math.Round(aura2G / 255.0, 2));
                                record.Set("Param6", Math.Round(aura2B / 255.0, 2));
                                record.Set("Param7", progFX.T6FadeDuration);
                                break;

                            case ProgrammedEffectType.Beam:
                                record.Set("Param1", exportLowercase ? progFX.T7BeamModel.ToLower() : progFX.T7BeamModel);
                                record.Set("Param2", progFX.T7BeamAnimation);
                                break;

                            case ProgrammedEffectType.MIRV:
                                record.Set("Param1", exportLowercase ? progFX.T10ProjectileModel.ToLower() : progFX.T10ProjectileModel);
                                record.Set("Param2", project.Spells.Get2DAIndex(progFX.T10Spell));
                                record.Set("Param3", (int)progFX.T10Orientation);
                                record.Set("Param4", (int)progFX.T10ProjectilePath);
                                record.Set("Param5", progFX.T10TravelTime.ToString().ToLower());
                                break;

                            case ProgrammedEffectType.VariantMIRV:
                                record.Set("Param1", exportLowercase ? progFX.T11ProjectileModel.ToLower() : progFX.T11ProjectileModel);
                                record.Set("Param2", progFX.T11FireSound);
                                record.Set("Param3", progFX.T11ImpactSound);
                                record.Set("Param4", (int)progFX.T11ProjectilePath);
                                break;

                            case ProgrammedEffectType.SpellCastFailure:
                                record.Set("Param1", progFX.T12ModelNode);
                                record.Set("Param2", exportLowercase ? progFX.T12EffectModel.ToLower() : progFX.T12EffectModel);
                                break;
                        }

                        WriteExtensionValues(record, progFX.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "progfx.2da";
                progFX2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("progfx", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportItemProperties(EosProject project)
        {
            if (project.ItemProperties.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "itempropdef.2da"));

            var itempropdef2da = Load2da("itempropdef");
            if (itempropdef2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    itempropdef2da.Columns.SetLowercase("SubTypeResRef");
                }

                AddExtensionColumns(itempropdef2da, project.ItemProperties.Extensions);
                foreach (var itemProp in project.ItemProperties.OrderBy(itemProp => itemProp?.Index))
                {
                    if (itemProp != null)
                    {
                        var index = -1;
                        if (itemProp.Overrides != null)
                            index = MasterRepository.Standard.ItemProperties.GetByID(itemProp.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.ItemProperties.GetCustomDataStartIndex() + itemProp.Index >= itempropdef2da.Count)
                            {
                                itempropdef2da.AddRecord();
                            }

                            index = project.ItemProperties.GetCustomDataStartIndex() + (itemProp.Index ?? 0);
                        }

                        var record = itempropdef2da[index];
                        record.Set("Name", GetTLKIndex(itemProp.Name));
                        record.Set("Label", itemProp.Name[project.DefaultLanguage].Text.Replace(" ", ""));
                        if (itemProp.SubType != null)
                            record.Set("SubTypeResRef", itemProp.SubType.Name.Trim());
                        else
                            record.Set("SubTypeResRef", itemProp.SubTypeResRef.Trim());
                        record.Set("Cost", itemProp.Cost);
                        record.Set("CostTableResRef", project.ItemPropertyCostTables.Get2DAIndex(itemProp.CostTable));
                        record.Set("Param1ResRef", project.ItemPropertyParams.Get2DAIndex(itemProp.Param));
                        record.Set("GameStrRef", GetTLKIndex(itemProp.PropertyText));
                        record.Set("Description", GetTLKIndex(itemProp.Description));

                        WriteExtensionValues(record, itemProp.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "itempropdef.2da";
                itempropdef2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("itempropdef", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportItemPropertyCategories(EosProject project)
        {
            if ((project.ItemPropertySets.Count == 0) && (!project.ItemProperties.Any(ip => (ip != null) && (ip.Overrides == null)))) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "itemprops.2da"));

            var itemprops2da = Load2da("itemprops");
            if (itemprops2da != null)
            {
                itemprops2da.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);

                // Add missing rows 
                foreach (var itemProp in project.ItemProperties.OrderBy(itemProp => itemProp?.Index))
                {
                    if (itemProp != null)
                    {
                        var index = -1;
                        if (itemProp.Overrides != null)
                            index = MasterRepository.Standard.ItemProperties.GetByID(itemProp.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.ItemProperties.GetCustomDataStartIndex() + itemProp.Index >= itemprops2da.Count)
                            {
                                itemprops2da.AddRecord();
                            }

                            index = project.ItemProperties.GetCustomDataStartIndex() + (itemProp.Index ?? 0);
                        }

                        var record = itemprops2da[index];
                        record.Set("StringRef", GetTLKIndex(itemProp.Name));
                        record.Set("Label", MakeLabel(itemProp.Name[project.DefaultLanguage].Text, ""));
                    }
                }

                // Write column data (property sets)
                foreach (var itemPropSet in project.ItemPropertySets.OrderBy(itemPropSet => itemPropSet?.Index))
                {
                    if (itemPropSet != null)
                    {
                        var index = -1;
                        if (itemPropSet.Overrides != null)
                            index = MasterRepository.Standard.ItemPropertySets.GetByID(itemPropSet.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            index = project.ItemPropertySets.GetCustomDataStartIndex() + (itemPropSet.Index ?? 0);
                            itemprops2da.Columns.InsertColumn(index, $"{index}_{itemPropSet.Name.Replace(" ", "_")}");
                        }

                        // Set column to null
                        for (int i=0; i < itemprops2da.Count; i++)
                            itemprops2da[i].Set(index, null);

                        foreach (var itemPropSetItem in itemPropSet.ItemProperties)
                        {
                            if (itemPropSetItem?.ItemProperty == null) continue;

                            var propIndex = -1;
                            if (itemPropSetItem.ItemProperty.Overrides != null)
                                propIndex = MasterRepository.Standard.ItemProperties.GetByID(itemPropSetItem.ItemProperty.Overrides ?? Guid.Empty)?.Index ?? -1;
                            else if (itemPropSetItem.ItemProperty.IsReadonly)
                                propIndex = itemPropSetItem.ItemProperty.Index ?? -1;
                            else
                                propIndex = project.ItemProperties.GetCustomDataStartIndex() + (itemPropSetItem.ItemProperty.Index ?? 0);

                            itemprops2da[propIndex].Set(index, true);
                        }
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "itemprops.2da";
                itemprops2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("itemprops", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportItemPropertyTables(EosProject project)
        {
            foreach (var table in project.ItemPropertyTables)
            {
                if (table != null)
                {
                    Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da"));

                    var columns = new List<String>
                    {
                        "Name", "Label",
                    };

                    var custColumns = new List<CustomObjectProperty>
                    {
                        table.CustomColumn01,
                        table.CustomColumn02,
                        table.CustomColumn03,
                        table.CustomColumn04,
                        table.CustomColumn05,
                        table.CustomColumn06,
                        table.CustomColumn07,
                        table.CustomColumn08,
                        table.CustomColumn09,
                        table.CustomColumn10,
                    };

                    for (int i = 0; i < custColumns.Count; i++)
                    {
                        if (custColumns[i].Column.Trim() == "") break;
                        columns.Add(custColumns[i].Column.Trim());
                    }

                    var td2 = new TwoDimensionalArrayFile();
                    td2.New(columns.ToArray());

                    if (project.Settings.Export.LowercaseFilenames)
                    {
                        for (int i = 0; i < custColumns.Count; i++)
                        {
                            if (custColumns[i].Column.Trim() == "") break;

                            if (custColumns[i].DataType?.ID.Equals(new Guid("e4897c44-4117-45d4-b3fc-37b82fd88247")) ?? false)
                                td2.Columns.SetLowercase(custColumns[i].Column.Trim());
                        }
                    }

                    td2.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);

                    foreach (var item in table.Items)
                    {
                        if (item != null)
                        {
                            var rec = td2.AddRecord();
                            rec.Set("Name", GetTLKIndex(item.Name));
                            rec.Set("Label", MakeLabel(item.Name[project.DefaultLanguage].Text, ""));

                            var custValues = new List<CustomValueInstance>
                            {
                                item.CustomColumnValue01,
                                item.CustomColumnValue02,
                                item.CustomColumnValue03,
                                item.CustomColumnValue04,
                                item.CustomColumnValue05,
                                item.CustomColumnValue06,
                                item.CustomColumnValue07,
                                item.CustomColumnValue08,
                                item.CustomColumnValue09,
                                item.CustomColumnValue10,
                            };

                            for (int i = 0; i < custColumns.Count; i++)
                            {
                                if (custColumns[i].Column.Trim() == "") break;
                                if (custColumns[i].DataType?.To2DA == null) continue;

                                if (custValues[i].Value is TLKStringSet tlkValue)
                                    rec.Set(custColumns[i].Column.Trim(), GetTLKIndex(tlkValue));
                                else
                                    rec.Set(custColumns[i].Column.Trim(), custColumns[i].DataType?.To2DA(custValues[i].Value, project.Settings.Export.LowercaseFilenames, GetTLKIndex));
                            }
                        }
                    }

                    var filename = project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da";
                    td2.Save(filename, project.Settings.Export.Compress2DA);

                    AddHAKResource(table.Name.ToLower(), NWNResourceType.TWODA, filename);
                }
            }
        }

        private void ExportItemPropertyCostTables(EosProject project)
        {
            if (project.ItemPropertyCostTables.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "iprp_costtable.2da"));

            var costtable2da = Load2da("iprp_costtable");
            if (costtable2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    costtable2da.Columns.SetLowercase("Name");
                }

                AddExtensionColumns(costtable2da, project.ItemPropertyCostTables.Extensions);
                foreach (var costTable in project.ItemPropertyCostTables.OrderBy(costTable => costTable?.Index))
                {
                    if (costTable != null)
                    {
                        var index = -1;
                        if (costTable.Overrides != null)
                            index = MasterRepository.Standard.ItemPropertyCostTables.GetByID(costTable.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.ItemPropertyCostTables.GetCustomDataStartIndex() + costTable.Index >= costtable2da.Count)
                            {
                                costtable2da.AddRecord();
                            }

                            index = project.ItemPropertyCostTables.GetCustomDataStartIndex() + (costTable.Index ?? 0);
                        }

                        var record = costtable2da[index];
                        record.Set("Name", costTable.Name);
                        record.Set("Label", costTable.Name); // !
                        record.Set("ClientLoad", costTable.ClientLoad);

                        WriteExtensionValues(record, costTable.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "iprp_costtable.2da";
                costtable2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("iprp_costtable", NWNResourceType.TWODA, filename);
            }

            // Export tables
            foreach (var table in project.ItemPropertyCostTables)
            {
                if (table != null)
                {
                    Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da"));

                    var columns = new List<String>
                    {
                        "Name", "Label", "Cost"
                    };

                    var custColumns = new List<CustomObjectProperty>
                    {
                        table.CustomColumn01,
                        table.CustomColumn02,
                        table.CustomColumn03,
                        table.CustomColumn04,
                        table.CustomColumn05,
                        table.CustomColumn06,
                        table.CustomColumn07,
                        table.CustomColumn08,
                        table.CustomColumn09,
                        table.CustomColumn10,
                    };

                    for (int i = 0; i < custColumns.Count; i++)
                    {
                        if (custColumns[i].Column.Trim() == "") break;
                        columns.Add(custColumns[i].Column.Trim());
                    }

                    var td2 = new TwoDimensionalArrayFile();
                    td2.New(columns.ToArray());

                    if (project.Settings.Export.LowercaseFilenames)
                    {
                        for (int i = 0; i < custColumns.Count; i++)
                        {
                            if (custColumns[i].Column.Trim() == "") break;

                            if (custColumns[i].DataType?.ID.Equals(new Guid("e4897c44-4117-45d4-b3fc-37b82fd88247")) ?? false)
                                td2.Columns.SetLowercase(custColumns[i].Column.Trim());
                        }
                    }

                    td2.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);

                    foreach (var item in table.Items)
                    {
                        if (item != null)
                        {
                            var rec = td2.AddRecord();
                            rec.Set("Name", GetTLKIndex(item.Name));
                            rec.Set("Label", MakeLabel(item.Name[project.DefaultLanguage].Text, ""));
                            rec.Set("Cost", item.Cost);

                            var custValues = new List<CustomValueInstance>
                            {
                                item.CustomColumnValue01,
                                item.CustomColumnValue02,
                                item.CustomColumnValue03,
                                item.CustomColumnValue04,
                                item.CustomColumnValue05,
                                item.CustomColumnValue06,
                                item.CustomColumnValue07,
                                item.CustomColumnValue08,
                                item.CustomColumnValue09,
                                item.CustomColumnValue10,
                            };

                            for (int i = 0; i < custColumns.Count; i++)
                            {
                                if (custColumns[i].Column.Trim() == "") break;
                                if (custColumns[i].DataType?.To2DA == null) continue;

                                if (custValues[i].Value is TLKStringSet tlkValue)
                                    rec.Set(custColumns[i].Column.Trim(), GetTLKIndex(tlkValue));
                                else
                                    rec.Set(custColumns[i].Column.Trim(), custColumns[i].DataType?.To2DA(custValues[i].Value, project.Settings.Export.LowercaseFilenames, GetTLKIndex));
                            }
                        }
                    }

                    var filename = project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da";
                    td2.Save(filename, project.Settings.Export.Compress2DA);

                    AddHAKResource(table.Name.ToLower(), NWNResourceType.TWODA, filename);
                }
            }
        }

        private void ExportItemPropertiyParams(EosProject project)
        {
            if (project.ItemPropertyParams.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "iprp_paramtable.2da"));

            var paramtable2da = Load2da("iprp_paramtable");
            if (paramtable2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    paramtable2da.Columns.SetLowercase("TableResRef");
                }

                paramtable2da.Columns.SetMaxLength("Lable", project.Settings.Export.LabelMaxLength);

                AddExtensionColumns(paramtable2da, project.ItemPropertyParams.Extensions);
                foreach (var paramTable in project.ItemPropertyParams.OrderBy(paramTable => paramTable?.Index))
                {
                    if (paramTable != null)
                    {
                        var index = -1;
                        if (paramTable.Overrides != null)
                            index = MasterRepository.Standard.ItemPropertyParams.GetByID(paramTable.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.ItemPropertyParams.GetCustomDataStartIndex() + paramTable.Index >= paramtable2da.Count)
                            {
                                paramtable2da.AddRecord();
                            }

                            index = project.ItemPropertyParams.GetCustomDataStartIndex() + (paramTable.Index ?? 0);
                        }

                        var record = paramtable2da[index];
                        record.Set("Name", GetTLKIndex(paramTable.Name));
                        record.Set("Lable", MakeLabel(paramTable.Name[project.DefaultLanguage].Text, "")); // "Lable" is NOT an typo! At least not in my code

                        if (paramTable.ItemPropertyTable != null)
                            record.Set("TableResRef", paramTable.ItemPropertyTable.Name.Trim());
                        else
                            record.Set("TableResRef", paramTable.TableResRef.Trim());

                        WriteExtensionValues(record, paramTable.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "iprp_paramtable.2da";
                paramtable2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("iprp_paramtable", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportDamageTypes(EosProject project)
        {
            if (project.DamageTypes.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "damagetypes.2da"));

            var damagetypes2da = Load2da("damagetypes");
            var damagehitvisual2da = Load2da("damagehitvisual");
            if ((damagetypes2da != null) && (damagehitvisual2da != null))
            {
                damagetypes2da.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);
                damagehitvisual2da.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);

                AddExtensionColumns(damagetypes2da, project.DamageTypes.Extensions);
                foreach (var damageType in project.DamageTypes.OrderBy(damageType => damageType?.Index))
                {
                    if (damageType != null)
                    {
                        var index = -1;
                        if (damageType.Overrides != null)
                            index = MasterRepository.Standard.DamageTypes.GetByID(damageType.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.DamageTypes.GetCustomDataStartIndex() + damageType.Index >= damagetypes2da.Count)
                            {
                                damagetypes2da.AddRecord();
                                damagehitvisual2da.AddRecord();
                            }

                            index = project.DamageTypes.GetCustomDataStartIndex() + (damageType.Index ?? 0);
                        }

                        var record = damagetypes2da[index];
                        record.Set("Label", MakeLabel(damageType.Name[project.DefaultLanguage].Text, ""));
                        record.Set("CharsheetStrref", GetTLKIndex(damageType.Name));
                        record.Set("DamageTypeGroup", project.DamageTypeGroups.Get2DAIndex(damageType.Group));
                        record.Set("DamageRangedProjectile", project.RangedDamageTypes.Get2DAIndex(damageType.RangedDamageType) ?? 0);

                        WriteExtensionValues(record, damageType.ExtensionValues, project.Settings.Export.LowercaseFilenames);

                        var dhvRecord = damagehitvisual2da[index];
                        dhvRecord.Set("Label", MakeLabel(damageType.Name[project.DefaultLanguage].Text, ""));
                        dhvRecord.Set("VisualEffectID", project.VisualEffects.Get2DAIndex(damageType.MeleeImpactVFX));
                        dhvRecord.Set("RangedEffectID", project.VisualEffects.Get2DAIndex(damageType.RangedImpactVFX));
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "damagetypes.2da";
                damagetypes2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("damagetypes", NWNResourceType.TWODA, filename);

                var dhvFilename = project.Settings.Export.TwoDAFolder + "damagehitvisual.2da";
                damagehitvisual2da.Save(dhvFilename, project.Settings.Export.Compress2DA);

                AddHAKResource("damagehitvisual", NWNResourceType.TWODA, dhvFilename);
            }
        }

        private void ExportDamageTypeGroups(EosProject project)
        {
            if (project.DamageTypeGroups.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "damagetypegroups.2da"));

            var damagetypegroups2da = Load2da("damagetypegroups");
            if (damagetypegroups2da != null)
            {
                damagetypegroups2da.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);

                AddExtensionColumns(damagetypegroups2da, project.DamageTypeGroups.Extensions);
                foreach (var damageTypeGroup in project.DamageTypeGroups.OrderBy(damageTypeGroup => damageTypeGroup?.Index))
                {
                    if (damageTypeGroup != null)
                    {
                        var index = -1;
                        if (damageTypeGroup.Overrides != null)
                            index = MasterRepository.Standard.DamageTypeGroups.GetByID(damageTypeGroup.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.DamageTypeGroups.GetCustomDataStartIndex() + damageTypeGroup.Index >= damagetypegroups2da.Count)
                            {
                                damagetypegroups2da.AddRecord();
                            }

                            index = project.DamageTypeGroups.GetCustomDataStartIndex() + (damageTypeGroup.Index ?? 0);
                        }

                        var record = damagetypegroups2da[index];
                        record.Set("Label", MakeLabel(damageTypeGroup.Name, ""));
                        record.Set("FeedbackStrref", GetTLKIndex(damageTypeGroup.FeedbackText));
                        if (damageTypeGroup.Color != null)
                        {
                            var colorR = (damageTypeGroup.Color >> 16) & 0xFF;
                            var colorG = (damageTypeGroup.Color >> 8) & 0xFF;
                            var colorB = (damageTypeGroup.Color) & 0xFF;
                            record.Set("ColorR", colorR);
                            record.Set("ColorG", colorG);
                            record.Set("ColorB", colorB);
                        }

                        WriteExtensionValues(record, damageTypeGroup.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "damagetypegroups.2da";
                damagetypegroups2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("damagetypegroups", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportSavingthrowTypes(EosProject project)
        {
            if (project.SavingthrowTypes.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "savingthrowtypes.2da"));

            var savingthrowtypes2da = Load2da("savingthrowtypes");
            if (savingthrowtypes2da != null)
            {
                savingthrowtypes2da.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);

                AddExtensionColumns(savingthrowtypes2da, project.SavingthrowTypes.Extensions);
                foreach (var savingthrowType in project.SavingthrowTypes.OrderBy(savingthrowType => savingthrowType?.Index))
                {
                    if (savingthrowType != null)
                    {
                        var index = -1;
                        if (savingthrowType.Overrides != null)
                            index = MasterRepository.Standard.SavingthrowTypes.GetByID(savingthrowType.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.SavingthrowTypes.GetCustomDataStartIndex() + savingthrowType.Index >= savingthrowtypes2da.Count)
                            {
                                savingthrowtypes2da.AddRecord();
                            }

                            index = project.SavingthrowTypes.GetCustomDataStartIndex() + (savingthrowType.Index ?? 0);
                        }

                        var record = savingthrowtypes2da[index];
                        record.Set("Label", MakeLabel(savingthrowType.Name[project.DefaultLanguage].Text, "_"));
                        record.Set("Strref", GetTLKIndex(savingthrowType.Name));
                        record.Set("Immunity", (int?)savingthrowType.Immunity);
                        record.Set("ImmunityOnlyIfSpell", savingthrowType.ImmunityOnlyForSpells);

                        WriteExtensionValues(record, savingthrowType.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "savingthrowtypes.2da";
                savingthrowtypes2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("savingthrowtypes", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportAmmunitions(EosProject project)
        {
            if (project.Ammunitions.Count == 0) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "ammunitiontypes.2da"));

            var ammunitiontypes2da = Load2da("ammunitiontypes");
            if (ammunitiontypes2da != null)
            {
                ammunitiontypes2da.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);

                AddExtensionColumns(ammunitiontypes2da, project.Ammunitions.Extensions);
                foreach (var ammunition in project.Ammunitions.OrderBy(ammunition => ammunition?.Index))
                {
                    if (ammunition != null)
                    {
                        var index = -1;
                        if (ammunition.Overrides != null)
                            index = MasterRepository.Standard.Ammunitions.GetByID(ammunition.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.Ammunitions.GetCustomDataStartIndex() + ammunition.Index >= ammunitiontypes2da.Count)
                            {
                                ammunitiontypes2da.AddRecord();
                            }

                            index = project.Ammunitions.GetCustomDataStartIndex() + (ammunition.Index ?? 0);
                        }

                        var record = ammunitiontypes2da[index];
                        record.Set("label", MakeLabel(ammunition.Name, "_"));
                        record.Set("Model", ammunition.Model);
                        record.Set("ShotSound", ammunition.ShotSound);
                        record.Set("ImpactSound", ammunition.ImpactSound);
                        record.Set("AmmunitionType", (int)ammunition.AmmunitionType);
                        record.Set("DamageRangedProjectile", project.RangedDamageTypes.Get2DAIndex(ammunition.RangedDamageType) ?? 0);

                        WriteExtensionValues(record, ammunition.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "ammunitiontypes.2da";
                ammunitiontypes2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("ammunitiontypes", NWNResourceType.TWODA, filename);
            }
        }

        private void SetSpellbookSpellLevels(EosProject project, Spellbook spellbook, ObservableCollection<SpellbookEntry> spellLevel, int level, TwoDimensionalArrayFile spells2da)
        {
            foreach (var spell in spellLevel)
            {
                if (spell.Spell != null)
                {
                    var overrideSpell = project.Spells.GetOverride(spell.Spell);
                    if (!(overrideSpell ?? spell.Spell)?.Disabled ?? false)
                    {
                        var spellIndex = project.Spells.Get2DAIndex(spell.Spell);
                        if (spellIndex != null)
                            spells2da[spellIndex ?? -1].Set(spellbook.Name, level);
                    }
                }
            }
        }

        private int GetSubFeatId(Spell subSpell)
        {
            if (subSpell.ParentSpell == null) return -1;

            if (subSpell.ParentSpell.SubSpell1 == subSpell)
                return 1;
            if (subSpell.ParentSpell.SubSpell2 == subSpell)
                return 2;
            if (subSpell.ParentSpell.SubSpell3 == subSpell)
                return 3;
            if (subSpell.ParentSpell.SubSpell4 == subSpell)
                return 4;
            if (subSpell.ParentSpell.SubSpell5 == subSpell)
                return 5;
            if (subSpell.ParentSpell.SubSpell6 == subSpell)
                return 6;
            if (subSpell.ParentSpell.SubSpell7 == subSpell)
                return 7;
            if (subSpell.ParentSpell.SubSpell8 == subSpell)
                return 8;

            return -1;
        }

        private Feat? GetSpellFeat(Spell spell)
        {
            foreach (var feat in MasterRepository.Feats)
            {
                if ((feat != null) && (feat.OnUseEffect == spell))
                    return feat;
            }

            return null;
        }

        private void ExportSpells(EosProject project)
        {
            if ((project.Spells.Count == 0) && (project.Spellbooks.Count == 0)) return;

            Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + "spells.2da"));

            var spells2da = Load2da("spells");
            if (spells2da != null)
            {
                spells2da.Columns.AddColumn("SubRadSpell6");
                spells2da.Columns.AddColumn("SubRadSpell7");
                spells2da.Columns.AddColumn("SubRadSpell8");
                spells2da.Columns.AddColumn("TargetShape");
                spells2da.Columns.AddColumn("TargetFlags");
                spells2da.Columns.AddColumn("TargetSizeX");
                spells2da.Columns.AddColumn("TargetSizeY");

                for (int i = 0; i < project.Spellbooks.Count; i++)
                {
                    var spellbook = project.Spellbooks[i];
                    if ((spellbook != null) && (spellbook.Overrides == null))
                        spells2da.Columns.AddColumn(spellbook.Name);
                }

                spells2da.Columns.SetHex("MetaMagic");
                spells2da.Columns.SetHex("TargetType");

                if (project.Settings.Export.LowercaseFilenames)
                {
                    spells2da.Columns.SetLowercase("IconResRef");
                    spells2da.Columns.SetLowercase("ImpactScript");
                    spells2da.Columns.SetLowercase("ConjHeadVisual");
                    spells2da.Columns.SetLowercase("ConjHandVisual");
                    spells2da.Columns.SetLowercase("ConjGrndVisual");
                    spells2da.Columns.SetLowercase("ConjSoundVFX");
                    spells2da.Columns.SetLowercase("ConjSoundMale");
                    spells2da.Columns.SetLowercase("ConjSoundFemale");
                    spells2da.Columns.SetLowercase("CastHeadVisual");
                    spells2da.Columns.SetLowercase("CastHandVisual");
                    spells2da.Columns.SetLowercase("CastGrndVisual");
                    spells2da.Columns.SetLowercase("CastSound");
                    spells2da.Columns.SetLowercase("ProjModel");
                    spells2da.Columns.SetLowercase("ProjSound");
                }

                spells2da.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);

                AddExtensionColumns(spells2da, project.Spells.Extensions);
                foreach (var spell in project.Spells.OrderBy(spell => spell?.Index))
                {
                    if (spell != null)
                    {
                        var index = -1;
                        if (spell.Overrides != null)
                            index = MasterRepository.Standard.Spells.GetByID(spell.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            while (project.Spells.GetCustomDataStartIndex() + spell.Index >= spells2da.Count)
                            {
                                spells2da.AddRecord();
                            }

                            index = project.Spells.GetCustomDataStartIndex() + (spell.Index ?? 0);
                        }

                        var record = spells2da[index];
                        if (spell.Disabled)
                        {
                            record.Clear();
                            continue;
                        }

                        record.Set("Label", MakeLabel(spell.Name[project.DefaultLanguage].Text, "_"));
                        record.Set("Name", GetTLKIndex(spell.Name));
                        record.Set("IconResRef", spell.Icon);
                        record.Set("School", spell.School.ToString());
                        record.Set("Range", spell.Range.ToString());

                        String vs = "";
                        if (spell.Components.HasFlag(SpellComponent.V))
                            vs += "v";
                        if (spell.Components.HasFlag(SpellComponent.S))
                            vs += "s";
                        record.Set("VS", vs != "" ? vs : "-");
                        record.Set("MetaMagic", (int)spell.AvailableMetaMagic);
                        record.Set("TargetType", (int)spell.TargetTypes);
                        record.Set("ImpactScript", spell.ImpactScript);
                        // Spellbook Columns
                        record.Set("Innate", spell.InnateLevel);
                        record.Set("ConjTime", spell.ConjurationTime);
                        record.Set("ConjAnim", spell.ConjuringAnimation?.ToString().ToLower());
                        record.Set("ConjHeadVisual", spell.ConjurationHeadEffect);
                        record.Set("ConjHandVisual", spell.ConjurationHandEffect);
                        record.Set("ConjGrndVisual", spell.ConjurationGroundEffect);
                        record.Set("ConjSoundVFX", spell.ConjurationSound);
                        record.Set("ConjSoundMale", spell.ConjurationMaleSound);
                        record.Set("ConjSoundFemale", spell.ConjurationFemaleSound);
                        record.Set("CastAnim", spell.CastingAnimation?.ToString().ToLower());
                        record.Set("CastTime", spell.CastTime);
                        record.Set("CastHeadVisual", spell.CastingHeadEffect);
                        record.Set("CastHandVisual", spell.CastingHandEffect);
                        record.Set("CastGrndVisual", spell.CastingGroundEffect);
                        record.Set("CastSound", spell.CastingSound);
                        record.Set("Proj", spell.HasProjectile);
                        record.Set("ProjModel", spell.ProjectileModel);
                        record.Set("ProjType", spell.ProjectileType?.ToString().ToLower());
                        record.Set("ProjSpwnPoint", spell.ProjectileSpawnPoint?.ToString().ToLower());
                        record.Set("ProjSound", spell.ProjectileSound);
                        record.Set("ProjOrientation", (spell.ProjectileOrientation ?? ProjectileOrientation.None) != ProjectileOrientation.None ? spell.ProjectileOrientation?.ToString().ToLower() : null);
                        // ImmunityType (Unused)
                        // ItemImmunity (Unused)
                        record.Set("SubRadSpell1", project.Spells.Get2DAIndex(spell.SubSpell1));
                        record.Set("SubRadSpell2", project.Spells.Get2DAIndex(spell.SubSpell2));
                        record.Set("SubRadSpell3", project.Spells.Get2DAIndex(spell.SubSpell3));
                        record.Set("SubRadSpell4", project.Spells.Get2DAIndex(spell.SubSpell4));
                        record.Set("SubRadSpell5", project.Spells.Get2DAIndex(spell.SubSpell5));
                        record.Set("Category", (int?)spell.Category);
                        record.Set("Master", project.Spells.Get2DAIndex(spell.ParentSpell));
                        record.Set("UserType", (int)spell.Type);
                        record.Set("SpellDesc", GetTLKIndex(spell.Description));
                        record.Set("UseConcentration", spell.UseConcentration);
                        record.Set("SpontaneouslyCast", spell.IsCastSpontaneously);
                        record.Set("AltMessage", GetTLKIndex(spell.AlternativeCastMessage));
                        record.Set("HostileSetting", spell.IsHostile);
                        if (spell.Type == SpellType.Feat)
                        {
                            if (spell.ParentSpell != null)
                            {
                                var parentFeat = GetSpellFeat(spell.ParentSpell);
                                var subFeatId = GetSubFeatId(spell);

                                record.Set("FeatID", (0x10000 * subFeatId) + project.Feats.Get2DAIndex(parentFeat));
                            }
                            else
                            {
                                var spellFeat = GetSpellFeat(spell);
                                if (spellFeat != null)
                                {
                                    record.Set("FeatID", project.Feats.Get2DAIndex(spellFeat));
                                }
                            }
                        }
                        record.Set("Counter1", project.Spells.Get2DAIndex(spell.CounterSpell1));
                        record.Set("Counter2", project.Spells.Get2DAIndex(spell.CounterSpell2));
                        record.Set("HasProjectile", spell.HasProjectileVisuals);

                        // Additional columns
                        record.Set("SubRadSpell6", project.Spells.Get2DAIndex(spell.SubSpell6));
                        record.Set("SubRadSpell7", project.Spells.Get2DAIndex(spell.SubSpell7));
                        record.Set("SubRadSpell8", project.Spells.Get2DAIndex(spell.SubSpell8));

                        record.Set("TargetShape", spell.TargetShape?.ToString().ToLower());
                        record.Set("TargetFlags", (int?)spell.TargetingFlags);
                        record.Set("TargetSizeX", spell.TargetSizeX);
                        record.Set("TargetSizeY", spell.TargetSizeY);

                        WriteExtensionValues(record, spell.ExtensionValues, project.Settings.Export.LowercaseFilenames);
                    }
                }

                // Clear overridden spellbooks
                foreach (var spellbook in project.Spellbooks)
                {
                    if ((spellbook != null) && (spellbook.Overrides != null))
                    { 
                        for (int i = 0; i < spells2da.Count; i++)
                            spells2da[i].Set(spellbook.Name, null);
                    }
                }

                // Fill custom spellbook columns
                foreach (var spellbook in project.Spellbooks)
                {
                    if (spellbook == null) continue;

                    SetSpellbookSpellLevels(project, spellbook, spellbook.Level0, 0, spells2da);
                    SetSpellbookSpellLevels(project, spellbook, spellbook.Level1, 1, spells2da);
                    SetSpellbookSpellLevels(project, spellbook, spellbook.Level2, 2, spells2da);
                    SetSpellbookSpellLevels(project, spellbook, spellbook.Level3, 3, spells2da);
                    SetSpellbookSpellLevels(project, spellbook, spellbook.Level4, 4, spells2da);
                    SetSpellbookSpellLevels(project, spellbook, spellbook.Level5, 5, spells2da);
                    SetSpellbookSpellLevels(project, spellbook, spellbook.Level6, 6, spells2da);
                    SetSpellbookSpellLevels(project, spellbook, spellbook.Level7, 7, spells2da);
                    SetSpellbookSpellLevels(project, spellbook, spellbook.Level8, 8, spells2da);
                    SetSpellbookSpellLevels(project, spellbook, spellbook.Level9, 9, spells2da);
                }

                var filename = project.Settings.Export.TwoDAFolder + "spells.2da";
                spells2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource("spells", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportCustomObjects(EosProject project)
        {
            foreach (var template in project.CustomObjects)
            {
                if (template == null) continue;

                Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + template.ResourceName.ToLower() + ".2da"));

                var repo = project.CustomObjectRepositories[template];
                if (repo.Count == 0) return;

                var custom2da = new TwoDimensionalArrayFile();
                var columns = new List<string>();
                columns.Add("Label");
                foreach (var prop in template.Items)
                {
                    if ((prop == null) || (prop.DataType?.IsVisualOnly ?? false)) continue;
                    columns.Add(prop.Column);
                }
                custom2da.New(columns.ToArray());

                custom2da.Columns.SetMaxLength("Label", project.Settings.Export.LabelMaxLength);

                foreach (var instance in repo.OrderBy(instance => instance?.Index))
                {
                    if (instance == null) continue;

                    var rec = custom2da.AddRecord();
                    while (instance.Index >= custom2da.Count)
                    {
                        rec = custom2da.AddRecord();
                    }

                    if (!instance.Disabled)
                    {
                        rec.Set("Label", MakeLabel(instance.Name, "_"));
                        foreach (var value in instance.Values)
                        {
                            if ((!value.Property.DataType?.IsVisualOnly ?? false) && (value.Property.DataType?.To2DA != null))
                                rec.Set(value.Property.Column, value.Property.DataType?.To2DA(value.Value, project.Settings.Export.LowercaseFilenames, GetTLKIndex));
                        }
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + template.ResourceName.ToLower() + ".2da";
                custom2da.Save(filename, project.Settings.Export.Compress2DA);

                AddHAKResource(template.ResourceName.ToLower(), NWNResourceType.TWODA, filename);
            }
        }

        private void ExportCustomTables(EosProject project)
        {
            foreach (var template in project.CustomTables)
            {
                if (template == null) continue;

                var repo = project.CustomTableRepositories[template];
                if (repo.Count == 0) return;

                var columns = new List<string>();
                var columnLower = new List<bool>();
                foreach (var prop in template.Items)
                {
                    if ((prop == null) || (prop.DataType?.IsVisualOnly ?? false)) continue;
                    columns.Add(prop.Column);
                    columnLower.Add(prop.DataType?.ID.Equals(new Guid("e4897c44-4117-45d4-b3fc-37b82fd88247")) ?? false);
                }

                foreach (var instance in repo)
                {
                    if (instance == null) continue;

                    Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + instance.Name.ToLower() + ".2da"));

                    var custom2da = new TwoDimensionalArrayFile();
                    custom2da.New(columns.ToArray());

                    if (project.Settings.Export.LowercaseFilenames)
                    {
                        for (int i = 0; i < columns.Count; i++)
                        {
                            if (columnLower[i])
                                custom2da.Columns.SetLowercase(columns[i].Trim());
                        }
                    }

                    foreach (var item in instance.Items)
                    {
                        if (item != null)
                        {
                            var rec = custom2da.AddRecord();
                            foreach (var value in item.Values)
                            {
                                if ((!value.Property.DataType?.IsVisualOnly ?? false) && (value.Property.DataType?.To2DA != null))
                                    rec.Set(value.Property.Column, value.Property.DataType?.To2DA(value.Value, project.Settings.Export.LowercaseFilenames, GetTLKIndex));
                            }
                        }
                    }

                    var filename = project.Settings.Export.TwoDAFolder + instance.Name.ToLower() + ".2da";
                    custom2da.Save(filename, project.Settings.Export.Compress2DA);

                    AddHAKResource(instance.Name.ToLower(), NWNResourceType.TWODA, filename);
                }
            }
        }

        private void ExportCustomDynamicTables(EosProject project)
        {
            foreach (var template in project.CustomDynamicTables)
            {
                if (template == null) continue;

                var repo = project.CustomDynamicTableRepositories[template];
                if (repo.Count == 0) return;

                foreach (var instance in repo)
                {
                    if (instance == null) continue;

                    Log.Info("Exporting 2DA: \"{0}\"", Path.GetFullPath(project.Settings.Export.TwoDAFolder + instance.Name.ToLower() + ".2da"));

                    var columns = new List<String>();
                    var custColumns = new List<CustomObjectProperty>
                    {
                        instance.CustomColumn01,
                        instance.CustomColumn02,
                        instance.CustomColumn03,
                        instance.CustomColumn04,
                        instance.CustomColumn05,
                        instance.CustomColumn06,
                        instance.CustomColumn07,
                        instance.CustomColumn08,
                        instance.CustomColumn09,
                        instance.CustomColumn10,
                    };

                    for (int i = 0; i < custColumns.Count; i++)
                    {
                        if (custColumns[i].Column.Trim() == "") break;
                        columns.Add(custColumns[i].Column.Trim());
                    }

                    var custom2da = new TwoDimensionalArrayFile();
                    custom2da.New(columns.ToArray());

                    if (project.Settings.Export.LowercaseFilenames)
                    {
                        for (int i = 0; i < custColumns.Count; i++)
                        {
                            if (custColumns[i].Column.Trim() == "") break;

                            if (custColumns[i].DataType?.ID.Equals(new Guid("e4897c44-4117-45d4-b3fc-37b82fd88247")) ?? false) // Resource Reference
                                custom2da.Columns.SetLowercase(custColumns[i].Column.Trim());
                        }
                    }

                    foreach (var item in instance.Items)
                    {
                        if (item != null)
                        {
                            var rec = custom2da.AddRecord();

                            var custValues = new List<CustomValueInstance>
                            {
                                item.CustomColumnValue01,
                                item.CustomColumnValue02,
                                item.CustomColumnValue03,
                                item.CustomColumnValue04,
                                item.CustomColumnValue05,
                                item.CustomColumnValue06,
                                item.CustomColumnValue07,
                                item.CustomColumnValue08,
                                item.CustomColumnValue09,
                                item.CustomColumnValue10,
                            };

                            for (int i = 0; i < custColumns.Count; i++)
                            {
                                if (custColumns[i].Column.Trim() == "") break;
                                if (custColumns[i].DataType?.To2DA == null) continue;

                                if (custValues[i].Value is TLKStringSet tlkValue)
                                    rec.Set(custColumns[i].Column.Trim(), GetTLKIndex(tlkValue));
                                else
                                    rec.Set(custColumns[i].Column.Trim(), custColumns[i].DataType?.To2DA(custValues[i].Value, project.Settings.Export.LowercaseFilenames, GetTLKIndex));
                            }
                        }
                    }

                    var filename = project.Settings.Export.TwoDAFolder + instance.Name.ToLower() + ".2da";
                    custom2da.Save(filename, project.Settings.Export.Compress2DA);

                    AddHAKResource(instance.Name.ToLower(), NWNResourceType.TWODA, filename);
                }
            }
        }

        private String CleanString(String str, bool toUppercase = true)
        {
            if (toUppercase) str = str.Trim().ToUpper();

            var result = "";
            foreach (var c in str)
            {
                if (char.IsLetterOrDigit(c) || (c == '_'))
                    result += c;
                else if (char.IsWhiteSpace(c))
                    result += "_";
            }

            return result;
        }

        private String GetScriptConstant(String prefix, BaseModel? model, String namePrefix = "")
        {
            if (model == null) return "";

            var modelOverride = MasterRepository.Standard.GetOverride(model);
            model = modelOverride ?? model;

            var result = "";
            if (!modelScriptConstants.TryGetValue(model, out result))
            {
                if (model.ScriptConstant != "")
                    result = prefix + CleanString(model.ScriptConstant);
                else
                {
                    var name = model.TlkDisplayName ?? model.GetLabel();
                    if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        name = name.Substring(prefix.Length);
                    if (name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase))
                        name = name.Substring(namePrefix.Length);
                    result = prefix + CleanString(name);
                }

                if (generatedConstants.Contains(result))
                {
                    var tmpResult = result;
                    if (model.Hint.Trim() != "")
                        tmpResult = result + "_" + CleanString(model.Hint);

                    var numberedResult = tmpResult;
                    var number = 2;
                    while (generatedConstants.Contains(numberedResult))
                    {
                        numberedResult = tmpResult + "_" + number.ToString();
                        number++;
                    }

                    result = numberedResult;
                }

                generatedConstants.Add(result);
                modelScriptConstants.Add(model, result);
            }

            return result;
        }

        private string? MakeLabel(string? name, string separator)
        {
            var result = name;
            if (result != null)
            {
                result = result.Replace(" ", separator);
                result = result.Replace(":", separator);
                result = result.Replace("_", separator);

                result = result.Replace("(", "");
                result = result.Replace(")", "");
                result = result.Replace("'", "");
                result = result.Replace("+", "");
                result = result.Replace("-", "");
            }

            return result;
        }

        private void ExportIncludeFile(EosProject project)
        {
            Log.Info("Exporting include file: \"{0}\"", Path.GetFullPath(project.Settings.Export.IncludeFolder + project.Settings.Export.IncludeFilename + ".nss"));

            var incFile = new List<String>();

            // Races
            var customRaces = project.Races.Where(race => race != null && race.Overrides == null);
            if (customRaces.Any())
            {
                incFile.Add("// Racial Types");
                foreach (var race in customRaces)
                {
                    if (race == null) continue;
                    var index = project.Races.GetCustomDataStartIndex() + race.Index;
                    incFile.Add("const int " + GetScriptConstant("RACIAL_TYPE_", race) + " = " + index.ToString() + ";");
                }
                incFile.Add("");
            }

            // Classes
            var customClasses = project.Classes.Where(cls => (cls != null) && ((cls.Overrides == null) || (MasterRepository.Standard.Classes.GetByID(cls.Overrides ?? Guid.Empty)?.Hint == "Cut")));
            if (customClasses.Any())
            {
                incFile.Add("// Classes");
                foreach (var cls in customClasses)
                {
                    if (cls == null) continue;

                    int? index = -1;
                    if (cls.Overrides != null)
                        index = MasterRepository.Standard.Classes.GetByID(cls.Overrides ?? Guid.Empty)?.Index ?? -1;
                    else
                        index = project.Classes.GetCustomDataStartIndex() + cls.Index;
                    incFile.Add("const int " + GetScriptConstant("CLASS_TYPE_", cls) + " = " + index.ToString() + ";");
                }
                incFile.Add("");
            }

            // Packages
            var customPackages = project.ClassPackages.Where(package => package != null && package.Overrides == null);
            if (customPackages.Any())
            {
                incFile.Add("// Packages");
                foreach (var package in customPackages)
                {
                    if (package == null) continue;
                    var index = project.ClassPackages.GetCustomDataStartIndex() + package.Index;
                    incFile.Add("const int " + GetScriptConstant("PACKAGE_", package) + " = " + index.ToString() + ";");
                }
                incFile.Add("");
            }

            // Domains
            var customDomains = project.Domains.Where(domain => domain != null && domain.Overrides == null);
            if (customDomains.Any())
            {
                incFile.Add("// Domains");
                foreach (var domain in customDomains)
                {
                    if (domain == null) continue;
                    var index = project.Domains.GetCustomDataStartIndex() + domain.Index;
                    incFile.Add("const int " + GetScriptConstant("DOMAIN_", domain) + " = " + index.ToString() + ";");
                }
                incFile.Add("");
            }

            // Spells
            var customSpells = project.Spells.Where(spell => spell != null && spell.Overrides == null);
            if (customSpells.Any())
            {
                incFile.Add("// Spells");
                foreach (var spell in customSpells)
                {
                    if (spell == null) continue;
                    var index = project.Spells.GetCustomDataStartIndex() + spell.Index;
                    incFile.Add("const int " + GetScriptConstant("SPELL_", spell) + " = " + index.ToString() + ";");
                }
                incFile.Add("");
            }

            // Master Feats
            var customMasterFeats = project.MasterFeats.Where(masterFeat => masterFeat != null && masterFeat.Overrides == null);
            if (customMasterFeats.Any())
            {
                incFile.Add("// Master Feats");
                foreach (var masterFeat in customMasterFeats)
                {
                    if (masterFeat == null) continue;
                    var index = project.MasterFeats.GetCustomDataStartIndex() + masterFeat.Index;
                    incFile.Add("const int " + GetScriptConstant("MASTER_FEAT_", masterFeat) + " = " + index.ToString() + ";");
                }
                incFile.Add("");
            }

            // Feats
            var customFeats = project.Feats.Where(feat => (feat != null) && ((feat.Overrides == null) || (MasterRepository.Standard.Feats.GetByID(feat.Overrides ?? Guid.Empty)?.Hint == "Cut")));
            if (customFeats.Any())
            {
                incFile.Add("// Feats");
                foreach (var feat in customFeats)
                {
                    if (feat == null) continue;

                    int? index = -1;
                    if (feat.Overrides != null)
                        index = MasterRepository.Standard.Feats.GetByID(feat.Overrides ?? Guid.Empty)?.Index ?? -1;
                    else
                        index = project.Feats.GetCustomDataStartIndex() + feat.Index;

                    incFile.Add("const int " + GetScriptConstant("FEAT_", feat) + " = " + index.ToString() + ";");
                }
                incFile.Add("");
            }

            // Skills
            var customSkills = project.Skills.Where(skill => skill != null && skill.Overrides == null);
            if (customSkills.Any())
            {
                incFile.Add("// Skills");
                foreach (var skill in customSkills)
                {
                    if (skill == null) continue;
                    var index = project.Skills.GetCustomDataStartIndex() + skill.Index;
                    incFile.Add("const int " + GetScriptConstant("SKILL_", skill) + " = " + index.ToString() + ";");
                }
                incFile.Add("");
            }

            // Diseases
            var customDiseases = project.Diseases.Where(disease => disease != null && disease.Overrides == null);
            if (customDiseases.Any())
            {
                incFile.Add("// Diseases");
                foreach (var disease in customDiseases)
                {
                    if (disease == null) continue;
                    var index = project.Diseases.GetCustomDataStartIndex() + disease.Index;
                    incFile.Add("const int " + GetScriptConstant("DISEASE_", disease) + " = " + index.ToString() + ";");
                }
                incFile.Add("");
            }

            // Poisons
            var customPoisons = project.Poisons.Where(poison => poison != null && poison.Overrides == null);
            if (customPoisons.Any())
            {
                incFile.Add("// Poisons");
                foreach (var poison in customPoisons)
                {
                    if (poison == null) continue;
                    var index = project.Poisons.GetCustomDataStartIndex() + poison.Index;
                    incFile.Add("const int " + GetScriptConstant("POISON_", poison) + " = " + index.ToString() + ";");
                }
                incFile.Add("");
            }

            // Area Effects
            var customAoEs = project.AreaEffects.Where(aoe => aoe != null && aoe.Overrides == null);
            if (customAoEs.Any())
            {
                incFile.Add("// Area Effects");
                foreach (var aoe in customAoEs)
                {
                    if (aoe == null) continue;
                    var index = project.AreaEffects.GetCustomDataStartIndex() + aoe.Index;
                    incFile.Add("const int " + GetScriptConstant("AOE_", aoe, "VFX_") + " = " + index.ToString() + ";");
                }
                incFile.Add("");
            }

            // Soundsets
            var customSoundsets = project.Soundsets.Where(soundset => soundset != null && soundset.Overrides == null);
            if (customSoundsets.Any())
            {
                incFile.Add("// Soundsets");
                foreach (var soundset in customSoundsets)
                {
                    if (soundset == null) continue;
                    var index = project.Soundsets.GetCustomDataStartIndex() + soundset.Index;
                    incFile.Add("const int " + GetScriptConstant("SOUNDSET_", soundset) + " = " + index.ToString() + ";");
                }
                incFile.Add("");
            }

            // Polymorphs
            var customPolymorphs = project.Polymorphs.Where(polymorph => polymorph != null && polymorph.Overrides == null);
            if (customPolymorphs.Any())
            {
                incFile.Add("// Polymorphs");
                foreach (var polymorph in customPolymorphs)
                {
                    if (polymorph == null) continue;
                    var index = project.Polymorphs.GetCustomDataStartIndex() + polymorph.Index;
                    incFile.Add("const int " + GetScriptConstant("POLYMORPH_TYPE_", polymorph) + " = " + index.ToString() + ";");
                }
                incFile.Add("");
            }

            // Appearances
            var customAppearances = project.Appearances.Where(appearance => appearance != null && appearance.Overrides == null);
            if (customAppearances.Any())
            {
                incFile.Add("// Appearances");
                foreach (var appearance in customAppearances)
                {
                    if (appearance == null) continue;
                    var index = project.Appearances.GetCustomDataStartIndex() + appearance.Index;
                    incFile.Add("const int " + GetScriptConstant("APPEARANCE_TYPE_", appearance) + " = " + index.ToString() + ";");
                }
                incFile.Add("");
            }

            // Base Items
            var customBaseItems = project.BaseItems.Where(baseItem => baseItem != null && baseItem.Overrides == null);
            if (customBaseItems.Any())
            {
                incFile.Add("// Base Items");
                foreach (var baseItem in customBaseItems)
                {
                    if (baseItem == null) continue;
                    var index = project.BaseItems.GetCustomDataStartIndex() + baseItem.Index;
                    incFile.Add("const int " + GetScriptConstant("BASE_ITEM_", baseItem) + " = " + index.ToString() + ";");
                }
                incFile.Add("");
            }

            // Companions
            var customCompanions = project.Companions.Where(companion => companion != null && companion.Overrides == null);
            if (customCompanions.Any())
            {
                incFile.Add("// Companions");
                foreach (var companion in customCompanions)
                {
                    if (companion == null) continue;
                    var index = project.Companions.GetCustomDataStartIndex() + companion.Index;
                    incFile.Add("const int " + GetScriptConstant("ANIMAL_COMPANION_CREATURE_TYPE_", companion) + " = " + index.ToString() + ";");
                }
                incFile.Add("");
            }

            // Familiars
            var customFamiliars = project.Familiars.Where(familiar => familiar != null && familiar.Overrides == null);
            if (customFamiliars.Any())
            {
                incFile.Add("// Familiars");
                foreach (var familiar in customFamiliars)
                {
                    if (familiar == null) continue;
                    var index = project.Familiars.GetCustomDataStartIndex() + familiar.Index;
                    incFile.Add("const int " + GetScriptConstant("FAMILIAR_CREATURE_TYPE_", familiar) + " = " + index.ToString() + ";");
                }
                incFile.Add("");
            }

            // Traps
            var customTraps = project.Traps.Where(trap => trap != null && trap.Overrides == null);
            if (customTraps.Any())
            {
                incFile.Add("// Traps");
                foreach (var trap in customTraps)
                {
                    if (trap == null) continue;
                    var index = project.Traps.GetCustomDataStartIndex() + trap.Index;
                    incFile.Add("const int " + GetScriptConstant("TRAP_BASE_TYPE_", trap) + " = " + index.ToString() + ";");
                }
                incFile.Add("");
            }

            // Item Properties
            var customItemProperties = project.ItemProperties.Where(itemProp => itemProp != null && itemProp.Overrides == null);
            if (customItemProperties.Any())
            {
                incFile.Add("// Item Properties");
                foreach (var itemProp in customItemProperties)
                {
                    if (itemProp == null) continue;
                    var index = project.ItemProperties.GetCustomDataStartIndex() + itemProp.Index;
                    incFile.Add("const int " + GetScriptConstant("ITEM_PROPERTY_", itemProp) + " = " + index.ToString() + ";");
                }
                incFile.Add("");
            }

            // Savingthrow Types
            var customSavingthrowTypes = project.SavingthrowTypes.Where(save => save != null && save.Overrides == null);
            if (customSavingthrowTypes.Any())
            {
                incFile.Add("// Savingthrow Types");
                foreach (var saveType in customSavingthrowTypes)
                {
                    if (saveType == null) continue;
                    var index = project.SavingthrowTypes.GetCustomDataStartIndex() + saveType.Index;
                    incFile.Add("const int " + GetScriptConstant("SAVING_THROW_TYPE_", saveType) + " = " + index.ToString() + ";");
                }
                incFile.Add("");
            }

            // VFX
            var customVFX = project.VisualEffects.Where(vfx => vfx != null && vfx.Overrides == null);
            if (customVFX.Any())
            {
                incFile.Add("// Visual Effects");
                foreach (var vfx in customVFX)
                {
                    if (vfx == null) continue;
                    var index = project.VisualEffects.GetCustomDataStartIndex() + vfx.Index;

                    if (vfx.ScriptConstant != "")
                        incFile.Add("const int " + GetScriptConstant("VFX_", vfx) + " = " + index.ToString() + ";");
                    else
                        incFile.Add("const int " + vfx.Name.ToUpper() + " = " + index.ToString() + ";");
                }
                incFile.Add("");
            }

            Directory.CreateDirectory(project.Settings.Export.IncludeFolder);
            var filename = project.Settings.Export.IncludeFolder + project.Settings.Export.IncludeFilename + ".nss";
            File.WriteAllLines(filename, incFile);

            AddERFResource(project.Settings.Export.IncludeFilename.ToLower(), NWNResourceType.NSS, filename);
        }

        private void CreateHAK(EosProject project)
        {
            Log.Info("Exporting HAK file: \"{0}\"", Path.GetFullPath(project.Settings.Export.HakFolder + CleanString(project.Name, false).ToLower() + ".hak"));

            var hak = new ErfFile();
            hak.Description[project.DefaultLanguage].Text = project.Name + "\n\n";
            foreach (var key in hakResources.Keys)
                hak.AddResource(key.resRef, key.resType, hakResources[key]);

            // External Resources
            foreach (var resource in MasterRepository.Resources.GetExternalResources())
            {
                if ((resource.ResRef != null) && (!erfTypes.Contains(resource.Type)))
                    hak.AddResource(resource.ResRef, resource.Type, resource.FilePath);
            }

            Directory.CreateDirectory(project.Settings.Export.HakFolder);
            hak.Save(project.Settings.Export.HakFolder + CleanString(project.Name, false).ToLower() + ".hak");
        }

        private void CreateERF(EosProject project)
        {
            Log.Info("Exporting ERF file: \"{0}\"", Path.GetFullPath(project.Settings.Export.ErfFolder + CleanString(project.Name, false).ToLower() + ".erf"));

            var erf = new ErfFile();
            erf.Description[project.DefaultLanguage].Text = project.Name + "\n\n";
            foreach (var key in erfResources.Keys)
                erf.AddResource(key.resRef, key.resType, erfResources[key]);

            // External Resources
            foreach (var resource in MasterRepository.Resources.GetExternalResources())
            {
                if ((resource.ResRef != null) && (erfTypes.Contains(resource.Type)))
                    erf.AddResource(resource.ResRef, resource.Type, resource.FilePath);
            }

            if (erf.Count > 0)
            {
                Directory.CreateDirectory(project.Settings.Export.ErfFolder);
                erf.Save(project.Settings.Export.ErfFolder + CleanString(project.Name, false).ToLower() + ".erf");
            }
        }

        public void Export(EosProject project)
        {
            Log.Info("Exporting project \"{0}\"...", project.Name);
            try
            {
                if (File.Exists(Path.Combine(EosConfig.NwnBasePath, "data", "nwn_retail.key")))
                    ovrBif.Load(EosConfig.NwnBasePath, Path.Combine("data", "nwn_retail.key"));
                dialogTlk.Load(EosConfig.NwnBasePath);

                tableDataDict.Clear();
                modelIndices.Clear();

                Directory.CreateDirectory(project.Settings.Export.TwoDAFolder);
                Directory.CreateDirectory(project.Settings.Export.SsfFolder);
                Directory.CreateDirectory(project.Settings.Export.HakFolder);
                Directory.CreateDirectory(project.Settings.Export.TlkFolder);
                Directory.CreateDirectory(project.Settings.Export.IncludeFolder);

                ExportTLKs(project);

                ExportAttackBonusTables(project);
                ExportBonusFeatsTables(project);
                ExportStatGainTables(project);
                ExportSpellSlotTables(project);
                ExportKnownSpellsTables(project);
                ExportSavingThrowTables(project);
                ExportSkillsTables(project);
                ExportRacialFeatsTables(project);
                ExportFeatsTables(project);
                ExportPrerequisiteTables(project);
                ExportSpellPreferencesTables(project);
                ExportFeatPreferencesTables(project);
                ExportSkillPreferencesTables(project);
                ExportPackageEquipmentTables(project);

                ExportClasses(project);
                ExportClassPackages(project);
                ExportDiseases(project);
                ExportDomains(project);
                ExportFeats(project);
                ExportPoisons(project);
                ExportRaces(project);
                ExportSkills(project);
                ExportSoundsets(project);
                ExportSpells(project);
                ExportAreaEffects(project);
                ExportAppearances(project);
                ExportAppearanceSoundsets(project);
                ExportWeaponSounds(project);
                ExportInventorySounds(project);
                ExportPolymorphs(project);
                ExportMasterFeats(project);
                ExportBaseItems(project);
                ExportCompanions(project);
                ExportFamiliars(project);
                ExportTraps(project);
                ExportVisualEffects(project);
                ExportProgrammedEffects(project);
                ExportItemProperties(project);
                ExportItemPropertyCategories(project);
                ExportItemPropertyTables(project);
                ExportItemPropertyCostTables(project);
                ExportItemPropertiyParams(project);
                ExportDamageTypes(project);
                ExportDamageTypeGroups(project);
                ExportSavingthrowTypes(project);
                ExportAmmunitions(project);

                ExportCustomObjects(project);
                ExportCustomTables(project);
                ExportCustomDynamicTables(project);

                ExportIncludeFile(project);

                CreateHAK(project);
                CreateERF(project);
            }
            catch(Exception e)
            {
                Log.Error(e.Message);
                throw;
            }

            Log.Info("Project export successful!");
        }
    }
}
