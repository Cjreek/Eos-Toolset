using Eos.Config;
using Eos.Models;
using Eos.Models.Tables;
using Eos.Nwn;
using Eos.Nwn.Erf;
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
                if (extension != null)
                    twoDAFile.Columns.AddColumn(extension.Column);
            }
        }

        private void WriteExtensionValues(LineRecord record, ObservableCollection<CustomValueInstance> values)
        {
            foreach (var value in values)
            {
                if ((!value.Property.DataType?.IsVisualOnly ?? false) && (value.Property.DataType?.To2DA != null))
                    record.Set(value.Property.Column, value.Property.DataType?.To2DA(value.Value));
            }
        }

        private TwoDimensionalArrayFile? Load2da(String name)
        {
            var filename = EosConfig.NwnBasePath + @"\ovr\" + name + ".2da";
            if (File.Exists(filename))
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
            //if ((tlk.OriginalIndex ?? -1) < 0)
            var isEmpty = true;
            foreach (var lang in Enum.GetValues<TLKLanguage>())
            {
                if ((tlk[lang].Text.Trim() != "") || (tlk[lang].TextF.Trim() != ""))
                {
                    isEmpty = false;
                    break;
                }
            }

            if (!isEmpty)
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
        }

        private void ExportTLKs(EosProject project)
        {
            CollectTLKEntries(project);

            var tlkFile = new TlkFile();
            if (project.Settings.Export.BaseTlkFile != "")
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
                    td2.Save(filename);

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
                    td2.Save(filename);

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
                    td2.Save(filename);

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
                    td2.Save(filename);

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
                    td2.Save(filename);

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
                                    rec.Set("LABEL", ((Skill?)item.RequirementParam1)?.Name[project.DefaultLanguage].Text.Replace(" ", ""));
                                    break;

                                case RequirementType.VAR:
                                    rec.Set("LABEL", "ScriptVar");
                                    break;

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
                    td2.Save(filename);

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
                    td2.Save(filename);

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
                    td2.Save(filename);

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
                    var td2 = new TwoDimensionalArrayFile();
                    td2.New(columns);
                    foreach (var item in table.Items)
                    {
                        if (item != null)
                        {
                            var rec = td2.AddRecord();
                            rec.Set("SkillLabel", item.Skill?.Name[project.DefaultLanguage].Text.Replace(" ", ""));
                            rec.Set("SkillIndex", project.Skills.Get2DAIndex(item.Skill));
                            rec.Set("ClassSkill", item.IsClassSkill ? 1 : 0);
                        }
                    }

                    var filename = project.Settings.Export.TwoDAFolder + table.Name.ToLower() + ".2da";
                    td2.Save(filename);

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
                    td2.Save(filename);

                    AddHAKResource(table.Name.ToLower(), NWNResourceType.TWODA, filename);
                }
            }
        }

        private void ExportClasses(EosProject project)
        {
            if (project.Classes.Count == 0) return;

            var classes2da = Load2da("classes");
            if (classes2da != null)
            {
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
                        record.Set("Label", cls.Name[project.DefaultLanguage].Text.Replace(" ", "_"));
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

                        WriteExtensionValues(record, cls.ExtensionValues);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "classes.2da";
                classes2da.Save(filename);

                AddHAKResource("classes", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportClassPackages(EosProject project)
        {
            if (project.ClassPackages.Count == 0) return;

            var packages2da = Load2da("packages");
            if (packages2da != null)
            {
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
                        record.Set("Label", package.Name[project.DefaultLanguage].Text.Replace(" ", "_"));
                        record.Set("Name", GetTLKIndex(package.Name));
                        record.Set("Description", GetTLKIndex(package.Description));
                        record.Set("ClassID", project.Classes.Get2DAIndex(package.ForClass));
                        record.Set("Attribute", package.PreferredAbility.ToString());
                        record.Set("Gold", package.Gold);
                        record.Set("School", (int?)package.SpellSchool);
                        record.Set("Domain1", project.Domains.Get2DAIndex(package.Domain1));
                        record.Set("Domain2", project.Domains.Get2DAIndex(package.Domain1));
                        record.Set("Associate", null); // TODO
                        record.Set("SpellPref2DA", null); // TODO
                        record.Set("FeatPref2DA", null); // TODO
                        record.Set("SkillPref2DA", null); // TODO
                        record.Set("Equip2DA", null); // TODO
                        record.Set("Soundset", 0);
                        record.Set("PlayerClass", package.Playable);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "packages.2da";
                packages2da.Save(filename);

                AddHAKResource("packages", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportDiseases(EosProject project)
        {
            if (project.Diseases.Count == 0) return;

            var disease2da = Load2da("disease");
            if (disease2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    disease2da.Columns.SetLowercase("End_Incu_Script");
                    disease2da.Columns.SetLowercase("24_Hour_Script");
                }
                
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
                        record.Set("Label", disease.Name[project.DefaultLanguage].Text.Replace(" ", "_"));
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

                        WriteExtensionValues(record, disease.ExtensionValues);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "disease.2da";
                disease2da.Save(filename);

                AddHAKResource("disease", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportDomains(EosProject project)
        {
            if (project.Domains.Count == 0) return;

            var domains2da = Load2da("domains");
            if (domains2da != null)
            {
                domains2da.Columns.AddColumn("Level_0");

                if (project.Settings.Export.LowercaseFilenames)
                {
                    domains2da.Columns.SetLowercase("Icon");
                }

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
                        record.Set("Label", domain.Name[project.DefaultLanguage].Text.Replace(" ", "_").ToUpper());
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

                        WriteExtensionValues(record, domain.ExtensionValues);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "domains.2da";
                domains2da.Save(filename);

                AddHAKResource("domains", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportMasterFeats(EosProject project)
        {
            if (project.MasterFeats.Count == 0) return;

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

                        WriteExtensionValues(record, masterFeat.ExtensionValues);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "masterfeats.2da";
                masterFeats2da.Save(filename);

                AddHAKResource("masterfeats", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportFeats(EosProject project)
        {
            if (project.Feats.Count == 0) return;

            var feat2da = Load2da("feat");
            if (feat2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    feat2da.Columns.SetLowercase("Icon");
                }

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

                        record.Set("LABEL", feat.Name[project.DefaultLanguage].Text.Replace(" ", "_"));
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

                        WriteExtensionValues(record, feat.ExtensionValues);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "feat.2da";
                feat2da.Save(filename);

                AddHAKResource("feat", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportPoisons(EosProject project)
        {
            if (project.Poisons.Count == 0) return;

            var poison2da = Load2da("poison");
            if (poison2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    poison2da.Columns.SetLowercase("Script_1");
                    poison2da.Columns.SetLowercase("Script_2");
                    poison2da.Columns.SetLowercase("VFX_Impact");
                }

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
                        record.Set("Label", poison.Name[project.DefaultLanguage].Text.Replace(" ", "_"));
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

                        WriteExtensionValues(record, poison.ExtensionValues);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "poison.2da";
                poison2da.Save(filename);

                AddHAKResource("poison", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportRaces(EosProject project)
        {
            if (project.Races.Count == 0) return;

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
                        record.Set("Label", race.Name[project.DefaultLanguage].Text.Replace(" ", "_"));
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

                        WriteExtensionValues(record, race.ExtensionValues);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "racialtypes.2da";
                racialtypes2da.Save(filename);

                AddHAKResource("racialtypes", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportSkills(EosProject project)
        {
            if (project.Skills.Count == 0) return;

            var skills2da = Load2da("skills");
            if (skills2da != null)
            {
                skills2da.Columns.AddColumn("HideFromLevelUp");

                if (project.Settings.Export.LowercaseFilenames)
                {
                    skills2da.Columns.SetLowercase("Icon");
                }

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
                        record.Set("Label", skill.Name[project.DefaultLanguage].Text.Replace(" ", ""));
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

                        WriteExtensionValues(record, skill.ExtensionValues);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "skills.2da";
                skills2da.Save(filename);

                AddHAKResource("skills", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportAreaEffects(EosProject project)
        {
            if (project.AreaEffects.Count == 0) return;

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

                        WriteExtensionValues(record, aoe.ExtensionValues);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "vfx_persistent.2da";
                aoe2da.Save(filename);

                AddHAKResource("vfx_persistent", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportPolymorphs(EosProject project)
        {
            if (project.Polymorphs.Count == 0) return;

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

                        WriteExtensionValues(record, polymorph.ExtensionValues);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "polymorph.2da";
                polymorph2da.Save(filename);

                AddHAKResource("polymorph", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportSoundsets(EosProject project)
        {
            if (project.Soundsets.Count == 0) return;

            var soundset2da = Load2da("soundset");
            if (soundset2da != null)
            {
                if (project.Settings.Export.LowercaseFilenames)
                {
                    soundset2da.Columns.SetLowercase("RESREF");
                }
                
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
                        record.Set("LABEL", soundset.Name[project.DefaultLanguage].Text.Replace(" ", ""));
                        record.Set("RESREF", soundset.SoundsetResource);
                        record.Set("STRREF", GetTLKIndex(soundset.Name));
                        record.Set("GENDER", (int)soundset.Gender);
                        record.Set("TYPE", (int)soundset.Type);
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + "soundset.2da";
                soundset2da.Save(filename);

                AddHAKResource("soundset", NWNResourceType.TWODA, filename);
            }

            // TODO: Export soundset files
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

        private void ExportSpells(EosProject project)
        {
            if ((project.Spells.Count == 0) && (project.Spellbooks.Count == 0)) return;

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

                        record.Set("Label", spell.Name[project.DefaultLanguage].Text.Replace(" ", "_"));
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
                        record.Set("ProjOrientation", spell.ProjectileOrientation?.ToString().ToLower());
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
                            // TODO: Make less painful
                            foreach (var feat in MasterRepository.Feats)
                            {
                                if ((feat != null) && (feat.OnUseEffect == spell))
                                {
                                    record.Set("FeatID", project.Feats.Get2DAIndex(feat));
                                    break;
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

                        WriteExtensionValues(record, spell.ExtensionValues);
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
                spells2da.Save(filename);

                AddHAKResource("spells", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportCustomObjects(EosProject project)
        {
            foreach (var template in project.CustomObjects)
            {
                if (template == null) continue;
                
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

                foreach (var instance in repo.OrderBy(instance => instance?.Index))
                {
                    if (instance == null) continue;

                    var rec = custom2da.AddRecord();
                    while (instance.Index >= custom2da.Count)
                    {
                        rec = custom2da.AddRecord();
                    }

                    rec.Set("Label", instance.Label);
                    foreach (var value in instance.Values)
                    {
                        if ((!value.Property.DataType?.IsVisualOnly ?? false) && (value.Property.DataType?.To2DA != null))
                            rec.Set(value.Property.Column, value.Property.DataType?.To2DA(value.Value));
                    }
                }

                var filename = project.Settings.Export.TwoDAFolder + template.ResourceName.ToLower() + ".2da";
                custom2da.Save(filename);

                AddHAKResource(template.ResourceName.ToLower(), NWNResourceType.TWODA, filename);
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

        private String GetScriptConstant(String prefix, BaseModel? model)
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
                    result = prefix + CleanString(model.TlkDisplayName ?? "");

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

        private void ExportIncludeFile(EosProject project)
        {
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
            var customClasses = project.Classes.Where(cls => cls != null && cls.Overrides == null);
            if (customClasses.Any())
            {
                incFile.Add("// Classes");
                foreach (var cls in customClasses)
                {
                    if (cls == null) continue;
                    var index = project.Classes.GetCustomDataStartIndex() + cls.Index;
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
            var customFeats = project.Feats.Where(feat => feat != null && feat.Overrides == null);
            if (customFeats.Any())
            {
                incFile.Add("// Feats");
                foreach (var feat in customFeats)
                {
                    if (feat == null) continue;
                    var index = project.Feats.GetCustomDataStartIndex() + feat.Index;
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
                    incFile.Add("const int " + GetScriptConstant("AOE_PER_", aoe) + " = " + index.ToString() + ";");
                }
                incFile.Add("");
            }

            // Soundsets
            var customSoundsets = project.Soundsets.Where(soundset => soundset != null && soundset.Overrides == null);
            if (customSoundsets.Any())
            {
                incFile.Add("// Area Effects");
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

            Directory.CreateDirectory(project.Settings.Export.IncludeFolder);
            var filename = project.Settings.Export.IncludeFolder + project.Settings.Export.IncludeFilename + ".nss";
            File.WriteAllLines(filename, incFile);

            AddERFResource(project.Settings.Export.IncludeFilename.ToLower(), NWNResourceType.NSS, filename);
        }

        private void CreateHAK(EosProject project)
        {
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
            tableDataDict.Clear();
            modelIndices.Clear();

            Directory.CreateDirectory(project.Settings.Export.TwoDAFolder);
            Directory.CreateDirectory(project.Settings.Export.HakFolder);
            Directory.CreateDirectory(project.Settings.Export.TlkFolder);

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
            ExportPolymorphs(project);
            ExportMasterFeats(project);

            ExportCustomObjects(project);

            ExportIncludeFile(project);

            CreateHAK(project);
            CreateERF(project);
        }
    }
}
