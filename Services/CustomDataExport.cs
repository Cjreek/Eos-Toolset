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
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml.Linq;

namespace Eos.Services
{
    internal class CustomDataExport
    {
        private String ExportFolder = "";
        private String ExportTLKFolder = "";
        private String ExportHAKFolder = "";
        private String Export2DAFolder = "";

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

        private void AddTLKString(TLKStringSet tlk)
        {
            if ((tlk.OriginalIndex ?? -1) < 0)
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

                if (!isEmpty)
                    customTLKIndices.Add(tlk, -1);
            }
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
                //    AddTLKString(cls.NameLower);
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
            tlkFile.New(project.DefaultLanguage);

            foreach (var tlk in customTLKIndices.Keys)
                customTLKIndices[tlk] = tlkFile.AddText(tlk[project.DefaultLanguage].Text);

            tlkFile.Save(ExportTLKFolder + project.Name.ToLower().Replace(' ', '_') + ".tlk");
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

                    var filename = Export2DAFolder + table.Name.ToLower() + ".2da";
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

                    var filename = Export2DAFolder + table.Name.ToLower() + ".2da";
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
                            rec.Set("FeatLabel", item.Feat?.Name[project.DefaultLanguage].Text); // TODO: generate?/get FEAT constant
                            rec.Set("FeatIndex", project.Feats.Get2DAIndex(item?.Feat));
                            rec.Set("List", (int?)(item?.FeatList));
                            rec.Set("GrantedOnLevel", (int?)item?.GrantedOnLevel);
                            rec.Set("OnMenu", (int?)item?.Menu);
                        }
                    }

                    var filename = Export2DAFolder + table.Name.ToLower() + ".2da";
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

                    var filename = Export2DAFolder + table.Name.ToLower() + ".2da";
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

                    var filename = Export2DAFolder + table.Name.ToLower() + ".2da";
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

                    var filename = Export2DAFolder + table.Name.ToLower() + ".2da";
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
                            rec.Set("FeatLabel", item.Feat?.Name[project.DefaultLanguage].Text); // TODO: generate?/get FEAT constant
                            rec.Set("FeatIndex", project.Feats.Get2DAIndex(item?.Feat));
                        }
                    }

                    var filename = Export2DAFolder + table.Name.ToLower() + ".2da";
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

                    var filename = Export2DAFolder + table.Name.ToLower() + ".2da";
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

                    var filename = Export2DAFolder + table.Name.ToLower() + ".2da";
                    td2.Save(filename);

                    AddHAKResource(table.Name.ToLower(), NWNResourceType.TWODA, filename);
                }
            }
        }

        private int GetNumSpellLevels(SpellSlotTableItem spellSlotItem)
        {
            return ((spellSlotItem.SpellLevel0 ?? 0) > 0 ? 1 : 0) +
                   ((spellSlotItem.SpellLevel0 ?? 0) > 0 ? 1 : 0) +
                   ((spellSlotItem.SpellLevel0 ?? 0) > 0 ? 1 : 0) +
                   ((spellSlotItem.SpellLevel0 ?? 0) > 0 ? 1 : 0) +
                   ((spellSlotItem.SpellLevel0 ?? 0) > 0 ? 1 : 0) +
                   ((spellSlotItem.SpellLevel0 ?? 0) > 0 ? 1 : 0) +
                   ((spellSlotItem.SpellLevel0 ?? 0) > 0 ? 1 : 0) +
                   ((spellSlotItem.SpellLevel0 ?? 0) > 0 ? 1 : 0) +
                   ((spellSlotItem.SpellLevel0 ?? 0) > 0 ? 1 : 0) +
                   ((spellSlotItem.SpellLevel0 ?? 0) > 0 ? 1 : 0);
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
                            rec.Set("SpellLevel0", item.SpellLevel0 > 0 ? item.SpellLevel0 : null);
                            rec.Set("SpellLevel1", item.SpellLevel1 > 0 ? item.SpellLevel1 : null);
                            rec.Set("SpellLevel2", item.SpellLevel2 > 0 ? item.SpellLevel2 : null);
                            rec.Set("SpellLevel3", item.SpellLevel3 > 0 ? item.SpellLevel3 : null);
                            rec.Set("SpellLevel4", item.SpellLevel4 > 0 ? item.SpellLevel4 : null);
                            rec.Set("SpellLevel5", item.SpellLevel5 > 0 ? item.SpellLevel5 : null);
                            rec.Set("SpellLevel6", item.SpellLevel6 > 0 ? item.SpellLevel6 : null);
                            rec.Set("SpellLevel7", item.SpellLevel7 > 0 ? item.SpellLevel7 : null);
                            rec.Set("SpellLevel8", item.SpellLevel8 > 0 ? item.SpellLevel8 : null);
                            rec.Set("SpellLevel9", item.SpellLevel9 > 0 ? item.SpellLevel9 : null);
                        }
                    }

                    var filename = Export2DAFolder + table.Name.ToLower() + ".2da";
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
                for (int i=0; i < project.Classes.Count; i++)
                {
                    var cls = project.Classes[i];
                    if (cls != null)
                    {
                        var index = -1;
                        if (cls.Overrides != null)
                            index = MasterRepository.Standard.Classes.GetByID(cls.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            classes2da.AddRecord();
                            index = classes2da.Count - 1;
                        }

                        var record = classes2da[index];
                        record.Set("Label", cls.Name[project.DefaultLanguage].Text.Replace(" ", "_"));
                        record.Set("Name", GetTLKIndex(cls.Name));
                        record.Set("Plural", GetTLKIndex(cls.NamePlural));
                        record.Set("Lower", 0); // TODO: automatic lower index
                        record.Set("Description", GetTLKIndex(cls.Description));
                        record.Set("Icon", cls.Icon?.ToUpper());
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

                        record.Set("Constant", "CLASS_TYPE_" + cls.Name[project.DefaultLanguage].Text.Replace(" ", "_").ToUpper()); // TODO: Get constant
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
                    }
                }

                var filename = Export2DAFolder + "classes.2da";
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
                for (int i = 0; i < project.ClassPackages.Count; i++)
                {
                    var package = project.ClassPackages[i];
                    if (package != null)
                    {
                        var index = -1;
                        if (package.Overrides != null)
                            index = MasterRepository.Standard.ClassPackages.GetByID(package.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            packages2da.AddRecord();
                            index = packages2da.Count - 1;
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

                var filename = Export2DAFolder + "packages.2da";
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
                for (int i = 0; i < project.Diseases.Count; i++)
                {
                    var disease = project.Diseases[i];
                    if (disease != null)
                    {
                        var index = -1;
                        if (disease.Overrides != null)
                            index = MasterRepository.Standard.Diseases.GetByID(disease.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            disease2da.AddRecord();
                            index = disease2da.Count - 1;
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
                    }
                }

                var filename = Export2DAFolder + "disease.2da";
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
                for (int i = 0; i < project.Domains.Count; i++)
                {
                    var domain = project.Domains[i];
                    if (domain != null)
                    {
                        var index = -1;
                        if (domain.Overrides != null)
                            index = MasterRepository.Standard.Domains.GetByID(domain.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            domains2da.AddRecord();
                            index = domains2da.Count - 1;
                        }

                        var record = domains2da[index];
                        record.Set("Label", domain.Name[project.DefaultLanguage].Text.Replace(" ", "_").ToUpper());
                        record.Set("Name", GetTLKIndex(domain.Name));
                        record.Set("Description", GetTLKIndex(domain.Description));
                        record.Set("Icon", domain.Icon?.ToUpper());
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
                    }
                }

                var filename = Export2DAFolder + "domains.2da";
                domains2da.Save(filename);

                AddHAKResource("domains", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportFeats(EosProject project)
        {
            if (project.Feats.Count == 0) return;

            var feat2da = Load2da("feat");
            if (feat2da != null)
            {
                for (int i = 0; i < project.Feats.Count; i++)
                {
                    var feat = project.Feats[i];
                    if (feat != null)
                    {
                        var index = -1;
                        if (feat.Overrides != null)
                            index = MasterRepository.Standard.Feats.GetByID(feat.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            feat2da.AddRecord();
                            index = feat2da.Count - 1;
                        }

                        var record = feat2da[index];
                        record.Set("LABEL", feat.Name[project.DefaultLanguage].Text.Replace(" ", "_"));
                        record.Set("FEAT", GetTLKIndex(feat.Name));
                        record.Set("DESCRIPTION", GetTLKIndex(feat.Description));
                        record.Set("Icon", feat.Icon?.ToLower());
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
                        record.Set("MASTERFEAT", project.Feats.Get2DAIndex(feat.MasterFeat));
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
                        record.Set("Constant", "FEAT_" + feat.Name[project.DefaultLanguage].Text.Replace(" ", "_").ToUpper()); // TODO: Get constant
                        record.Set("TOOLSCATEGORIES", (int)feat.ToolsetCategory);
                        record.Set("HostileFeat", feat.OnUseEffect != null ? feat.IsHostile : null);
                        record.Set("MinLevel", feat.MinLevel > 1 ? feat.MinLevel : null);
                        record.Set("MinLevelClass", project.Classes.Get2DAIndex(feat.MinLevelClass));
                        record.Set("MaxLevel", feat.MaxLevel > 0 ? feat.MaxLevel : null);
                        record.Set("MinFortSave", feat.MinFortitudeSave > 0 ? feat.MinFortitudeSave : null);
                        record.Set("PreReqEpic", feat.RequiresEpic);
                        record.Set("ReqAction", feat.UseActionQueue);
                    }
                }

                var filename = Export2DAFolder + "feat.2da";
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
                for (int i = 0; i < project.Poisons.Count; i++)
                {
                    var poison = project.Poisons[i];
                    if (poison != null)
                    {
                        var index = -1;
                        if (poison.Overrides != null)
                            index = MasterRepository.Standard.Poisons.GetByID(poison.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            poison2da.AddRecord();
                            index = poison2da.Count - 1;
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
                    }
                }

                var filename = Export2DAFolder + "poison.2da";
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
                for (int i = 0; i < project.Races.Count; i++)
                {
                    var race = project.Races[i];
                    if (race != null)
                    {
                        var index = -1;
                        if (race.Overrides != null)
                            index = MasterRepository.Standard.Races.GetByID(race.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            racialtypes2da.AddRecord();
                            index = racialtypes2da.Count - 1;
                        }

                        var record = racialtypes2da[index];
                        record.Set("Label", race.Name[project.DefaultLanguage].Text.Replace(" ", "_"));
                        record.Set("Abrev", race.Name[project.DefaultLanguage].Text.Substring(0, 2));
                        record.Set("Name", GetTLKIndex(race.Name));
                        record.Set("ConverName", GetTLKIndex(race.Adjective));
                        //record.Set("ConverNameLower", 0); // TODO: Generate lower adjective
                        record.Set("NamePlural", GetTLKIndex(race.NamePlural));
                        record.Set("Description", GetTLKIndex(race.Description));
                        record.Set("Icon", race.Icon?.ToLower());
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
                        record.Set("Constant", "RACIAL_TYPE_" + race.Name[project.DefaultLanguage].Text.Replace(" ", "_").ToUpper()); // TODO: Generate/Get constant
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
                    }
                }

                var filename = Export2DAFolder + "racialtypes.2da";
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
                for (int i = 0; i < project.Skills.Count; i++)
                {
                    var skill = project.Skills[i];
                    if (skill != null)
                    {
                        var index = -1;
                        if (skill.Overrides != null)
                            index = MasterRepository.Standard.Skills.GetByID(skill.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            skills2da.AddRecord();
                            index = skills2da.Count - 1;
                        }

                        var record = skills2da[index];
                        record.Set("Label", skill.Name[project.DefaultLanguage].Text.Replace(" ", ""));
                        record.Set("Name", GetTLKIndex(skill.Name));
                        record.Set("Description", GetTLKIndex(skill.Description));
                        record.Set("Icon", skill.Icon?.ToLower());
                        record.Set("Untrained", skill.CanUseUntrained);
                        record.Set("KeyAbility", skill.KeyAbility.ToString());
                        record.Set("ArmorCheckPenalty", skill.UseArmorPenalty);
                        record.Set("AllClassesCanUse", skill.AllClassesCanUse);
                        record.Set("Category", (int?)skill.AIBehaviour);
                        //record.Set("MaxCR", null);
                        record.Set("Constant", "SKILL_" + skill.Name[project.DefaultLanguage].Text.Replace(" ", "_").ToUpper()); // TODO: Generate/Get skill constant
                        record.Set("HostileSkill", skill.IsHostile);
                        record.Set("HideFromLevelUp", skill.HideFromLevelUp);
                    }
                }

                var filename = Export2DAFolder + "skills.2da";
                skills2da.Save(filename);

                AddHAKResource("skills", NWNResourceType.TWODA, filename);
            }
        }

        private void ExportSoundsets(EosProject project)
        {
            if (project.Soundsets.Count == 0) return;

            var soundset2da = Load2da("soundset");
            if (soundset2da != null)
            {
                for (int i = 0; i < project.Soundsets.Count; i++)
                {
                    var soundset = project.Soundsets[i];
                    if (soundset != null)
                    {
                        var index = -1;
                        if (soundset.Overrides != null)
                            index = MasterRepository.Standard.Soundsets.GetByID(soundset.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            soundset2da.AddRecord();
                            index = soundset2da.Count - 1;
                        }

                        var record = soundset2da[index];
                        record.Set("LABEL", soundset.Name[project.DefaultLanguage].Text.Replace(" ", ""));
                        record.Set("RESREF", soundset.SoundsetResource);
                        record.Set("STRREF", GetTLKIndex(soundset.Name));
                        record.Set("GENDER", (int)soundset.Gender);
                        record.Set("TYPE", (int)soundset.Type);
                    }
                }

                var filename = Export2DAFolder + "soundset.2da";
                soundset2da.Save(filename);

                AddHAKResource("soundset", NWNResourceType.TWODA, filename);
            }

            // TODO: Export soundset files
        }

        private void SetSpellbookSpellLevels(EosProject project, Spellbook spellbook, ObservableCollection<SpellbookEntry> spellLevel, int level, TwoDimensionalArrayFile spells2da)
        {
            foreach (var spell in spellLevel)
            {
                var spellIndex = project.Spells.Get2DAIndex(spell.Spell);
                if (spellIndex != null)
                    spells2da[spellIndex ?? -1].Set(spellbook.Name, level);
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
                spells2da.Columns.AddColumn("TargetingFlags");
                spells2da.Columns.AddColumn("TargetSizeX");
                spells2da.Columns.AddColumn("TargetSizeY");
                for (int i = 0; i < project.Spellbooks.Count; i++)
                {
                    var spellbook = project.Spellbooks[i];
                    if ((spellbook != null) && (spellbook.Overrides == null))
                        spells2da.Columns.AddColumn(spellbook.Name);
                }

                for (int i = 0; i < project.Spells.Count; i++)
                {
                    var spell = project.Spells[i];
                    if (spell != null)
                    {
                        var index = -1;
                        if (spell.Overrides != null)
                            index = MasterRepository.Standard.Spells.GetByID(spell.Overrides ?? Guid.Empty)?.Index ?? -1;
                        else
                        {
                            spells2da.AddRecord();
                            index = spells2da.Count - 1;
                        }

                        var record = spells2da[index];
                        record.Set("Label", spell.Name[project.DefaultLanguage].Text.Replace(" ", "_"));
                        record.Set("Name", GetTLKIndex(spell.Name));
                        record.Set("IconResRef", spell.Icon);
                        record.Set("School", spell.School.ToString());
                        record.Set("Range", spell.Range.ToString());
                        record.Set("VS", spell.Components.ToString().ToLower()); // TODO: Does this work? Probably not
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
                        record.Set("HasProjectile", spell.HasProjectile);

                        // Additional columns
                        record.Set("SubRadSpell6", project.Spells.Get2DAIndex(spell.SubSpell6));
                        record.Set("SubRadSpell7", project.Spells.Get2DAIndex(spell.SubSpell7));
                        record.Set("SubRadSpell8", project.Spells.Get2DAIndex(spell.SubSpell8));

                        record.Set("TargetShape", spell.TargetShape?.ToString().ToLower());
                        record.Set("TargetingFlags", spell.TargetingFlags);
                        record.Set("TargetSizeX", spell.TargetSizeX);
                        record.Set("TargetSizeY", spell.TargetSizeY);
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

                var filename = Export2DAFolder + "spells.2da";
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

                foreach (var instance in repo)
                {
                    if (instance == null) continue;

                    var rec = custom2da.AddRecord();
                    rec.Set("Label", instance.Label);
                    foreach (var value in instance.Values)
                    {
                        if ((!value.Property.DataType?.IsVisualOnly ?? false) && (value.Property.DataType?.To2DA != null))
                            rec.Set(value.Property.Column, value.Property.DataType?.To2DA(value.Value));
                    }
                }

                var filename = Export2DAFolder + template.ResourceName.ToLower() + ".2da";
                custom2da.Save(filename);

                AddHAKResource(template.ResourceName.ToLower(), NWNResourceType.TWODA, filename);
            }
        }

        private void CreateHAK(EosProject project)
        {
            var hak = new ErfFile();
            hak.Description[TLKLanguage.English].Text = project.Name + "\n\n";
            foreach (var key in hakResources.Keys)
                hak.AddResource(key.resRef, key.resType, hakResources[key]);

            hak.Save(ExportHAKFolder + project.Name.ToLower().Replace(' ', '_') + ".hak");
        }

        public void Export(EosProject project)
        {
            tableDataDict.Clear();
            modelIndices.Clear();

            ExportFolder = project.ProjectFolder + Constants.ExportFolder;
            Export2DAFolder = project.ProjectFolder + Constants.ExportFolder;
            ExportTLKFolder = project.ProjectFolder + Constants.ExportTLKFolder;
            ExportHAKFolder = project.ProjectFolder + Constants.ExportHAKFolder;
            Directory.CreateDirectory(ExportFolder);
            Directory.CreateDirectory(Export2DAFolder);
            Directory.CreateDirectory(ExportTLKFolder);
            Directory.CreateDirectory(ExportHAKFolder);

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

            ExportCustomObjects(project);

            CreateHAK(project);
        }
    }
}
