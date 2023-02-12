using Eos.Models;
using Eos.Models.Tables;
using Eos.Nwn;
using Eos.Nwn.Bif;
using Eos.Nwn.Ssf;
using Eos.Nwn.Tlk;
using Eos.Nwn.TwoDimensionalArray;
using Eos.Repositories;
using Eos.Repositories.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;

namespace Eos.Services
{
    internal class GameDataImport
    {
        private string nwnBasePath = "";
        private TlkCollection tlk = new TlkCollection();
        private BifCollection bif = new BifCollection();
        private RepositoryCollection Standard = MasterRepository.Standard;
        private ResourceRepository Resources = MasterRepository.Resources;

        private List<KeyValuePair<TLKStringSet, int?>> tlkBuffer = new List<KeyValuePair<TLKStringSet, int?>>();
        private HashSet<String> iconResourceBuffer = new HashSet<String>();
        private TwoDimensionalArrayFile Load2da(String name)
        {
            var filename = nwnBasePath + @"\ovr\" + name + ".2da";
            if (File.Exists(filename))
            {
                using (var fs = File.OpenRead(filename))
                {
                    return new TwoDimensionalArrayFile(fs);
                }
            }
            else
            {
                var resource = bif.ReadResource(name, NWNResourceType.TWODA);
                return new TwoDimensionalArrayFile(resource.RawData);
            }
        }

        private String? AddIconResource(String? resRef, NWNResourceSource source = NWNResourceSource.BIF)
        {
            if (resRef != null)
                iconResourceBuffer.Add(resRef);
            return Resources.AddResource(source, resRef, NWNResourceType.TGA);
        }

        private Guid GenerateGuid(string prefix, int index)
        {
            var data = prefix + "###" + index.ToString();
            var guidBytes = MD5.HashData(Encoding.Unicode.GetBytes(data));
            return new Guid(guidBytes);
        }

        private bool SetText(TLKStringSet str, int? strRef)
        {
            var result = strRef != null;
            if (result)
            {
                str.OriginalIndex = strRef;
                tlkBuffer.Add(new KeyValuePair<TLKStringSet, int?>(str, strRef));
            }

            return result;
        }

        private T CreateRef<T>(int? index) where T : BaseModel, new()
        {
            T result = new T();
            result.Index = index;
            return result;
        }

        private void ImportText()
        {
            foreach (TLKLanguage lang in Enum.GetValues(typeof(TLKLanguage)))
            {
                foreach (var tlkPair in tlkBuffer)
                {
                    tlkPair.Key[lang].Text = tlk.GetString(lang, false, tlkPair.Value);
                    tlkPair.Key[lang].TextF = tlk.GetString(lang, true, tlkPair.Value);
                }
            }
        }

        private void ImportRacialFeatsTable(string tablename, Guid guid)
        {
            var racialFeatTable2da = Load2da(tablename.ToLower());

            var tmpRacialFeatsTable = new RacialFeatsTable();
            tmpRacialFeatsTable.ID = guid;
            tmpRacialFeatsTable.Name = tablename;

            tmpRacialFeatsTable.Clear();
            for (int i = 0; i < racialFeatTable2da.Count; i++)
            {
                var tmpItem = new RacialFeatsTableItem();
                tmpItem.ParentTable = tmpRacialFeatsTable;
                tmpItem.Feat = CreateRef<Feat>(racialFeatTable2da[i].AsInteger("FeatIndex"));
                tmpRacialFeatsTable.Add(tmpItem);
            }

            Standard.RacialFeatsTables.Add(tmpRacialFeatsTable);
        }

        private void ImportRaces()
        {
            var races2da = Load2da("racialtypes");

            Standard.Races.Clear();
            for (int i = 0; i < races2da.Count; i++)
            {
                var tmpRace = new Race();
                tmpRace.ID = GenerateGuid("racialtypes", i);
                tmpRace.Index = i;

                if (!SetText(tmpRace.Name, races2da[i].AsInteger("Name"))) continue;
                SetText(tmpRace.NamePlural, races2da[i].AsInteger("NamePlural"));
                SetText(tmpRace.Adjective, races2da[i].AsInteger("ConverName"));
                SetText(tmpRace.Description, races2da[i].AsInteger("Description"));
                SetText(tmpRace.Biography, races2da[i].AsInteger("Biography"));

                tmpRace.Icon = AddIconResource(races2da[i].AsString("Icon"));
                tmpRace.Appearance = CreateRef<Appearance>(races2da[i].AsInteger("Appearance"));
                tmpRace.StrAdjustment = races2da[i].AsInteger("StrAdjust") ?? 0;
                tmpRace.DexAdjustment = races2da[i].AsInteger("DexAdjust") ?? 0;
                tmpRace.IntAdjustment = races2da[i].AsInteger("IntAdjust") ?? 0;
                tmpRace.ChaAdjustment = races2da[i].AsInteger("ChaAdjust") ?? 0;
                tmpRace.WisAdjustment = races2da[i].AsInteger("WisAdjust") ?? 0;
                tmpRace.ConAdjustment = races2da[i].AsInteger("ConAdjust") ?? 0;

                tmpRace.FavoredClass = CreateRef<CharacterClass>(races2da[i].AsInteger("Favored"));

                tmpRace.Playable = races2da[i].AsBoolean("PlayerRace");
                tmpRace.DefaultAge = races2da[i].AsInteger("AGE") ?? 0;
                tmpRace.ToolsetDefaultClass = CreateRef<CharacterClass>(races2da[i].AsInteger("ToolsetDefaultClass"));
                tmpRace.CRModifier = races2da[i].AsFloat("CRModifier") ?? 1.0;

                tmpRace.NameGenTableA = races2da[i].AsString("NameGenTableA");
                tmpRace.NameGenTableB = races2da[i].AsString("NameGenTableB");

                tmpRace.FirstLevelExtraFeats = races2da[i].AsInteger("ExtraFeatsAtFirstLevel") ?? 0;
                tmpRace.ExtraSkillPointsPerLevel = races2da[i].AsInteger("ExtraSkillPointsPerLevel") ?? 0;
                tmpRace.FirstLevelSkillPointsMultiplier = races2da[i].AsInteger("FirstLevelSkillPointsMultiplier");
                tmpRace.FirstLevelAbilityPoints = races2da[i].AsInteger("AbilitiesPointBuyNumber");
                tmpRace.FeatEveryNthLevel = races2da[i].AsInteger("NormalFeatEveryNthLevel");
                tmpRace.FeatEveryNthLevelCount = races2da[i].AsInteger("NumberNormalFeatsEveryNthLevel");
                tmpRace.SkillPointModifierAbility = Enum.Parse<AbilityType>(races2da[i].AsString("SkillPointModifierAbility") ?? "", true);
                tmpRace.FavoredEnemyFeat = CreateRef<Feat>(races2da[i].AsInteger("FavoredEnemyFeat", null));

                // RacialFeatsTable
                var racialFeatsTable = races2da[i].AsString("FeatsTable");
                if (racialFeatsTable != null)
                {
                    var racialFeatsTableGuid = GenerateGuid(racialFeatsTable.ToLower(), 0);
                    if (!Standard.RacialFeatsTables.Contains(racialFeatsTableGuid))
                        ImportRacialFeatsTable(racialFeatsTable, racialFeatsTableGuid);
                    tmpRace.Feats = Standard.RacialFeatsTables.GetByID(racialFeatsTableGuid);
                }

                Standard.Races.Add(tmpRace);
            }
        }

        private void ImportAttackBonusTable(string tablename, Guid guid)
        {
            var abTable2da = Load2da(tablename.ToLower());

            var tmpAttackBonusTable = new AttackBonusTable();
            tmpAttackBonusTable.ID = guid;
            tmpAttackBonusTable.Name = tablename;

            tmpAttackBonusTable.Clear();
            for (int i = 0; i < abTable2da.Count; i++)
            {
                var tmpItem = new AttackBonusTableItem();
                tmpItem.ParentTable = tmpAttackBonusTable;
                tmpItem.Level = i + 1;
                tmpItem.AttackBonus = abTable2da[i].AsInteger("BAB") ?? 0;
                tmpAttackBonusTable.Add(tmpItem);
            }

            Standard.AttackBonusTables.Add(tmpAttackBonusTable);
        }

        private void ImportFeatsTable(string tablename, Guid guid)
        {
            var featTable2da = Load2da(tablename.ToLower());

            var tmpFeatsTable = new FeatsTable();
            tmpFeatsTable.ID = guid;
            tmpFeatsTable.Name = tablename;

            tmpFeatsTable.Clear();
            for (int i = 0; i < featTable2da.Count; i++)
            {
                var tmpItem = new FeatsTableItem();
                tmpItem.ParentTable = tmpFeatsTable;
                tmpItem.Feat = CreateRef<Feat>(featTable2da[i].AsInteger("FeatIndex"));
                tmpItem.FeatList = (FeatListType)Enum.ToObject(typeof(FeatListType), featTable2da[i].AsInteger("List") ?? 0);
                tmpItem.GrantedOnLevel = featTable2da[i].AsInteger("GrantedOnLevel") ?? -1;
                tmpItem.Menu = (FeatMenu)Enum.ToObject(typeof(FeatMenu), featTable2da[i].AsInteger("OnMenu") ?? 0);
                tmpFeatsTable.Add(tmpItem);
            }

            Standard.FeatTables.Add(tmpFeatsTable);
        }

        private void ImportSavingThrowTable(string tablename, Guid guid)
        {
            var savesTable2da = Load2da(tablename.ToLower());

            var tmpSavesTable = new SavingThrowTable();
            tmpSavesTable.ID = guid;
            tmpSavesTable.Name = tablename;

            tmpSavesTable.Clear();
            for (int i = 0; i < savesTable2da.Count; i++)
            {
                var tmpItem = new SavingThrowTableItem();
                tmpItem.ParentTable = tmpSavesTable;
                tmpItem.Level = savesTable2da[i].AsInteger("Level") ?? 0;
                tmpItem.FortitudeSave = savesTable2da[i].AsInteger("FortSave") ?? 0;
                tmpItem.ReflexSave = savesTable2da[i].AsInteger("RefSave") ?? 0;
                tmpItem.WillpowerSave = savesTable2da[i].AsInteger("WillSave") ?? 0;
                tmpSavesTable.Add(tmpItem);
            }

            Standard.SavingThrowTables.Add(tmpSavesTable);
        }

        private void ImportBonusFeatsTable(string tablename, Guid guid)
        {
            var bfeatTable2da = Load2da(tablename.ToLower());

            var tmpBFeatTable = new BonusFeatsTable();
            tmpBFeatTable.ID = guid;
            tmpBFeatTable.Name = tablename;

            tmpBFeatTable.Clear();
            for (int i = 0; i < bfeatTable2da.Count; i++)
            {
                var tmpItem = new BonusFeatsTableItem();
                tmpItem.ParentTable = tmpBFeatTable;
                tmpItem.Level = i + 1;
                tmpItem.BonusFeatCount = bfeatTable2da[i].AsInteger("Bonus") ?? 0;
                tmpBFeatTable.Add(tmpItem);
            }

            Standard.BonusFeatTables.Add(tmpBFeatTable);
        }

        private void ImportSkillTable(string tablename, Guid guid)
        {
            var skillTable2da = Load2da(tablename.ToLower());

            var skillTable = new SkillsTable();
            skillTable.ID = guid;
            skillTable.Name = tablename;

            skillTable.Clear();
            for (int i = 0; i < skillTable2da.Count; i++)
            {
                var tmpItem = new SkillsTableItem();
                tmpItem.ParentTable = skillTable;
                tmpItem.Skill = CreateRef<Skill>(skillTable2da[i].AsInteger("SkillIndex"));
                tmpItem.IsClassSkill = skillTable2da[i].AsBoolean("ClassSkill");
                skillTable.Add(tmpItem);
            }

            Standard.SkillTables.Add(skillTable);
        }

        private void ImportSpellSlotTable(string tablename, Guid guid)
        {
            var spellSlotTable2da = Load2da(tablename.ToLower());

            var spellSlotTable = new SpellSlotTable();
            spellSlotTable.ID = guid;
            spellSlotTable.Name = tablename;

            spellSlotTable.Clear();
            for (int i = 0; i < spellSlotTable2da.Count; i++)
            {
                var tmpItem = new SpellSlotTableItem();
                tmpItem.ParentTable = spellSlotTable;
                tmpItem.Level = spellSlotTable2da[i].AsInteger("Level") ?? 0;
                tmpItem.SpellLevel0 = spellSlotTable2da[i].AsInteger("SpellLevel0", null);
                tmpItem.SpellLevel1 = spellSlotTable2da[i].AsInteger("SpellLevel1", null);
                tmpItem.SpellLevel2 = spellSlotTable2da[i].AsInteger("SpellLevel2", null);
                tmpItem.SpellLevel3 = spellSlotTable2da[i].AsInteger("SpellLevel3", null);
                tmpItem.SpellLevel4 = spellSlotTable2da[i].AsInteger("SpellLevel4", null);
                tmpItem.SpellLevel5 = spellSlotTable2da[i].AsInteger("SpellLevel5", null);
                tmpItem.SpellLevel6 = spellSlotTable2da[i].AsInteger("SpellLevel6", null);
                tmpItem.SpellLevel7 = spellSlotTable2da[i].AsInteger("SpellLevel7", null);
                tmpItem.SpellLevel8 = spellSlotTable2da[i].AsInteger("SpellLevel8", null);
                tmpItem.SpellLevel9 = spellSlotTable2da[i].AsInteger("SpellLevel9", null);
                spellSlotTable.Add(tmpItem);
            }

            Standard.SpellSlotTables.Add(spellSlotTable);
        }

        private void ImportKnownSpellsTable(string tablename, Guid guid)
        {
            var knownSpellsTable2da = Load2da(tablename.ToLower());

            var knownSpellsTable = new KnownSpellsTable();
            knownSpellsTable.ID = guid;
            knownSpellsTable.Name = tablename;

            knownSpellsTable.Clear();
            for (int i = 0; i < knownSpellsTable2da.Count; i++)
            {
                var tmpItem = new KnownSpellsTableItem();
                tmpItem.ParentTable = knownSpellsTable;
                tmpItem.Level = knownSpellsTable2da[i].AsInteger("Level") ?? 0;
                tmpItem.SpellLevel0 = knownSpellsTable2da[i].AsInteger("SpellLevel0", null);
                tmpItem.SpellLevel1 = knownSpellsTable2da[i].AsInteger("SpellLevel1", null);
                tmpItem.SpellLevel2 = knownSpellsTable2da[i].AsInteger("SpellLevel2", null);
                tmpItem.SpellLevel3 = knownSpellsTable2da[i].AsInteger("SpellLevel3", null);
                tmpItem.SpellLevel4 = knownSpellsTable2da[i].AsInteger("SpellLevel4", null);
                tmpItem.SpellLevel5 = knownSpellsTable2da[i].AsInteger("SpellLevel5", null);
                tmpItem.SpellLevel6 = knownSpellsTable2da[i].AsInteger("SpellLevel6", null);
                tmpItem.SpellLevel7 = knownSpellsTable2da[i].AsInteger("SpellLevel7", null);
                tmpItem.SpellLevel8 = knownSpellsTable2da[i].AsInteger("SpellLevel8", null);
                tmpItem.SpellLevel9 = knownSpellsTable2da[i].AsInteger("SpellLevel9", null);
                knownSpellsTable.Add(tmpItem);
            }

            Standard.KnownSpellsTables.Add(knownSpellsTable);
        }

        private void ImportPrerequisiteTable(string tablename, Guid guid)
        {
            var preRequTable2da = Load2da(tablename.ToLower());

            var preRequTable = new PrerequisiteTable();
            preRequTable.ID = guid;
            preRequTable.Name = tablename;

            preRequTable.Clear();
            for (int i = 0; i < preRequTable2da.Count; i++)
            {
                var tmpItem = new PrerequisiteTableItem();
                tmpItem.ParentTable = preRequTable;
                tmpItem.RequirementType = Enum.Parse<RequirementType>(preRequTable2da[i].AsString("ReqType") ?? "", true);

                switch (tmpItem.RequirementType)
                {
                    case RequirementType.CLASSOR:
                    case RequirementType.CLASSNOT:
                        tmpItem.RequirementParam1 = CreateRef<CharacterClass>(preRequTable2da[i].AsInteger("ReqParam1"));
                        break;

                    case RequirementType.FEAT:
                    case RequirementType.FEATOR:
                        tmpItem.RequirementParam1 = CreateRef<Feat>(preRequTable2da[i].AsInteger("ReqParam1"));
                        break;

                    case RequirementType.RACE:
                        tmpItem.RequirementParam1 = CreateRef<Race>(preRequTable2da[i].AsInteger("ReqParam1"));
                        break;

                    case RequirementType.SKILL:
                        tmpItem.RequirementParam1 = CreateRef<Skill>(preRequTable2da[i].AsInteger("ReqParam1"));
                        break;

                    default:
                        tmpItem.RequirementParam1 = preRequTable2da[i].AsObject("ReqParam1");
                        break;
                }

                tmpItem.RequirementParam2 = preRequTable2da[i].AsObject("ReqParam2");
                preRequTable.Add(tmpItem);
            }

            Standard.PrerequisiteTables.Add(preRequTable);
        }

        private void ImportStatGainTable(string tablename, Guid guid)
        {
            var statGainTable2da = Load2da(tablename.ToLower());

            var statGainTable = new StatGainTable();
            statGainTable.ID = guid;
            statGainTable.Name = tablename;

            statGainTable.Clear();
            for (int i = 0; i < statGainTable2da.Count; i++)
            {
                var tmpItem = new StatGainTableItem();
                tmpItem.ParentTable = statGainTable;
                tmpItem.Level = statGainTable2da[i].AsInteger("Level") ?? 0;
                tmpItem.Strength = statGainTable2da[i].AsInteger("Str", null);
                tmpItem.Dexterity = statGainTable2da[i].AsInteger("Dex", null);
                tmpItem.Constitution = statGainTable2da[i].AsInteger("Con", null);
                tmpItem.Wisdom = statGainTable2da[i].AsInteger("Wis", null);
                tmpItem.Intelligence = statGainTable2da[i].AsInteger("Int", null);
                tmpItem.Charisma = statGainTable2da[i].AsInteger("Cha", null);
                tmpItem.NaturalAC = statGainTable2da[i].AsInteger("NaturalAC", null);
                statGainTable.Add(tmpItem);
            }

            Standard.StatGainTables.Add(statGainTable);
        }

        private void ImportCutClass(TwoDimensionalArrayFile classes2da, CharacterClass tmpClass, int index)
        {
            switch (index)
            {
                case 39: // Eye of Grummsh
                    tmpClass.Playable = true;
                    tmpClass.Name.OriginalIndex = -1;
                    tmpClass.Name[TLKLanguage.English].Text = "Eye of Grummsh";
                    tmpClass.Name[TLKLanguage.English].TextF = tmpClass.Name[TLKLanguage.English].Text;
                    tmpClass.NamePlural.OriginalIndex = -1;
                    tmpClass.NamePlural[TLKLanguage.English].Text = "Eyes of Grummsh";
                    tmpClass.NamePlural[TLKLanguage.English].TextF = tmpClass.NamePlural[TLKLanguage.English].Text;
                    tmpClass.Description.OriginalIndex = -1;
                    tmpClass.Description[TLKLanguage.English].Text = "(PRESTIGE CLASS)\r\nWorshipers of the Orc deity Gruumsh that have put out their own right eye in a bloody and painful ritual. These living martyrs to Gruumsh are some of the toughest orcs and half-orcs in the world.\r\nThe eye of Gruumsh is a true prestige class in the sense that all orcs respect those who achieve it.\r\nBarbarians gain the most value from this prestige class, since it encourages raging as a fighting style\r\n\r\n- Hit Die: d12\r\n- Proficiencies: Simple and Martial Weapons, Light, and Medium Armor Proficiency, Shields\r\n- Skill Points: 2 + Int Modifier.\r\n\r\nREQUIREMENTS:\r\n\r\nAlignment: Chaotic evil, choatic neutral, neutral evil, or neutral.\r\nRace: Half-Orc or Orc\r\nFeats: Exotic Weapon Proficiency (Double Axe), Weapon Focus (Double Axe).\r\nBase Attack Bonus: +6\r\n\r\nABILITIES:\r\n\r\nLevel\r\n1: Blind-Fight.\r\n Command the Horde\r\n Rage 1/day.\r\n2. Swing Blindly - The character's rage's become more powerful granting a +4 strength bonus, while also suffering a -4 armor bonus while raging.\r\n3: Ritual Scarring - Through frequent disfiguration of his own skin, the eye of Gruumsh gains a +1 natural armor bonus.\r\n4: Blinding Spittle - The character can spit their stomach acid into a target's eyes 2/day.\r\n Rage 2/day.\r\n5: Blindsight - The character gains blindsight in a 5 foot radius.\r\n6: Ritual Scarring - The eye of Gruumsh's natural armor bonus increases to +2.\r\n7: Blinding Spittle 4/day.\r\n8: Blindsight 10 foot radius.\r\n Rage 3/day.\r\n9: Ritual Scarring - The eye of Gruumsh's natural armor bonus increases to +3.\r\n10: Sight of Gruumsh - The character gains a +2 morale bonus on all saving throws.";
                    tmpClass.Description[TLKLanguage.English].TextF = tmpClass.Description[TLKLanguage.English].Text;
                    tmpClass.Hint = "Cut";
                    tmpClass.Icon = "ir_wizard";
                    tmpClass.HitDie = 12;
                    tmpClass.SkillPointsPerLevel = 2;
                    tmpClass.IsSpellCaster = false;
                    tmpClass.PrimaryAbility = AbilityType.STR;
                    tmpClass.MulticlassXPPenalty = false;
                    tmpClass.PreEpicMaxLevel = 10;
                    tmpClass.MaxLevel = 40;
                    tmpClass.AllowedAlignments = Alignment.Neutral | Alignment.NeutralEvil | Alignment.ChaoticEvil | Alignment.ChaoticNeutral;

                    Standard.Classes.Add(tmpClass);
                    break;

                case 40: // Shou Disciple
                    tmpClass.Playable = true;
                    tmpClass.Name.OriginalIndex = -1;
                    tmpClass.Name[TLKLanguage.English].Text = "Shou Disciple";
                    tmpClass.Name[TLKLanguage.English].TextF = tmpClass.Name[TLKLanguage.English].Text;
                    tmpClass.NamePlural.OriginalIndex = -1;
                    tmpClass.NamePlural[TLKLanguage.English].Text = "Shou Disciples";
                    tmpClass.NamePlural[TLKLanguage.English].TextF = tmpClass.NamePlural[TLKLanguage.English].Text;
                    tmpClass.Description.OriginalIndex = -1;
                    tmpClass.Description[TLKLanguage.English].Text = "(PRESTIGE CLASS)\r\nShou disciples are martial artists who have studied or observed the monks of Kara-Tur and seek to emulate their style. Focusing more on the martial aspects of a monk's training, they sacrifice the enlightenment and supernatural abilities of the true ascetic. Shou disciples fight with martial weapons and often wear armor, instantly marking them as different from monks.\r\n\r\n- Hit Die: d10\r\n- Proficiencies: Martial and Monk Weapons, Light Armor Proficiency.\r\n- Skill Points: 2 + Int Modifier.\r\n\r\nREQUIREMENTS:\r\n\r\nFeats: Dodge, Improved Unarmed Strike, Weapon Focus (Unarmed Strike)\r\nBase Attack Bonus: +3\r\nSkills: Tumble 4 ranks\r\nBase Reflex Save: +2\r\n\r\nABILITIES:\r\n\r\nLevel\r\n1: Shou Disciple Dodge Bonus +1 (replaces the normal +1 dodge feat bonus).\r\n Unarmed Strike - The character deals 1d6 of unarmed damage. A Shou disciple with levels in the monk class will use the better of the two damage ranges, or the unarmed damage calculated by combining his Shou disciple and monk levels and using the unarmed damage of a monk of the resulting level if it produces a better result.\r\n Wearing light armor does not interfere with any of the Shou disciple's class abilities, but shields and medium or heavy armor do.\r\n2: Unarmed Strike - The character deals 1d8 of unarmed damage.\r\n Shou Disciple Dodge Bonus +2\r\n Bonus Feat - Chosen from the following list if the prerequisites are met: Deflect Arrows, Expertise, Improved Initiative, Improved Knockdown, Mobility, Power Attack, Spring Attack, Weapon Finesse and Weapon Specialization.\r\n3: Unarmed Strike - The character deals 1d10 of unarmed damage.\r\n Martial Flurry (light) - The character gains the ability to use any light melee weapon for his flurry of blows.\r\n4: Shou Disciple Dodge Bonus +3\r\n Bonus Feat - Chosen from the following list if the prerequisites are met: Deflect Arrows, Expertise, Improved Initiative, Improved Knockdown, Mobility, Power Attack, Spring Attack, Weapon Finesse and Weapon Specialization.\r\n5: Unarmed Strike - The character deals 2d6 of unarmed damage.\r\n Martial Flurry (any) - The character gains the ability to use any melee weapon for his flurry of blows.";
                    tmpClass.Description[TLKLanguage.English].TextF = tmpClass.Description[TLKLanguage.English].Text;
                    tmpClass.Hint = "Cut";
                    tmpClass.Icon = "ir_wizard";
                    tmpClass.HitDie = 10;
                    tmpClass.SkillPointsPerLevel = 2;
                    tmpClass.IsSpellCaster = false;
                    tmpClass.PrimaryAbility = AbilityType.WIS;
                    tmpClass.MulticlassXPPenalty = false;
                    tmpClass.PreEpicMaxLevel = 5;
                    tmpClass.MaxLevel = 40;

                    Standard.Classes.Add(tmpClass);
                    break;
            }
        }

        private void ImportClasses()
        {
            var classes2da = Load2da("classes");

            Standard.Classes.Clear();
            for (int i = 0; i < classes2da.Count; i++)
            {
                var tmpClass = new CharacterClass();
                tmpClass.ID = GenerateGuid("classes", i);
                tmpClass.Index = i;

                if (!SetText(tmpClass.Name, classes2da[i].AsInteger("Name")))
                {
                    ImportCutClass(classes2da, tmpClass, i);
                    continue;
                }
                SetText(tmpClass.NamePlural, classes2da[i].AsInteger("Plural"));
                SetText(tmpClass.Description, classes2da[i].AsInteger("Description"));

                tmpClass.Icon = AddIconResource(classes2da[i].AsString("Icon"));
                tmpClass.HitDie = classes2da[i].AsInteger("HitDie") ?? 0;
                tmpClass.SkillPointsPerLevel = classes2da[i].AsInteger("SkillPointBase") ?? 0;

                // AttackBonusTable
                var abTable = classes2da[i].AsString("AttackBonusTable");
                if (abTable != null)
                {
                    var abTableGuid = GenerateGuid(abTable.ToLower(), 0);
                    if (!Standard.AttackBonusTables.Contains(abTableGuid))
                        ImportAttackBonusTable(abTable, abTableGuid);
                    tmpClass.AttackBonusTable = Standard.AttackBonusTables.GetByID(abTableGuid);
                }

                // FeatsTable
                var featsTable = classes2da[i].AsString("FeatsTable");
                if (featsTable != null)
                {
                    var featsTableGuid = GenerateGuid(featsTable.ToLower(), 0);
                    if (!Standard.FeatTables.Contains(featsTableGuid))
                        ImportFeatsTable(featsTable, featsTableGuid);
                    tmpClass.Feats = Standard.FeatTables.GetByID(featsTableGuid);
                }

                // SavingThrowTable
                var savingThrowTable = classes2da[i].AsString("SavingThrowTable");
                if (savingThrowTable != null)
                {
                    var savingThrowTableGuid = GenerateGuid(savingThrowTable.ToLower(), 0);
                    if (!Standard.SavingThrowTables.Contains(savingThrowTableGuid))
                        ImportSavingThrowTable(savingThrowTable, savingThrowTableGuid);
                    tmpClass.SavingThrows = Standard.SavingThrowTables.GetByID(savingThrowTableGuid);
                }

                // SkillsTable
                var skillTable = classes2da[i].AsString("SkillsTable");
                if (skillTable != null)
                {
                    var skillTableGuid = GenerateGuid(skillTable.ToLower(), 0);
                    if (!Standard.SkillTables.Contains(skillTableGuid))
                        ImportSkillTable(skillTable, skillTableGuid);
                    tmpClass.Skills = Standard.SkillTables.GetByID(skillTableGuid);
                }

                // BonusFeatsTable
                var bonusFeatTable = classes2da[i].AsString("BonusFeatsTable");
                if (bonusFeatTable != null)
                {
                    var bonusFeatTableGuid = GenerateGuid(bonusFeatTable.ToLower(), 0);
                    if (!Standard.BonusFeatTables.Contains(bonusFeatTableGuid))
                        ImportBonusFeatsTable(bonusFeatTable, bonusFeatTableGuid);
                    tmpClass.BonusFeats = Standard.BonusFeatTables.GetByID(bonusFeatTableGuid);
                }

                // SpellGainTable
                var spellGainTable = classes2da[i].AsString("SpellGainTable");
                if (spellGainTable != null)
                {
                    var spellGainTableGuid = GenerateGuid(spellGainTable.ToLower(), 0);
                    if (!Standard.SpellSlotTables.Contains(spellGainTableGuid))
                        ImportSpellSlotTable(spellGainTable, spellGainTableGuid);
                    tmpClass.SpellSlots = Standard.SpellSlotTables.GetByID(spellGainTableGuid);
                }

                // SpellKnownTable
                var spellKnownTable = classes2da[i].AsString("SpellKnownTable");
                if (spellKnownTable != null)
                {
                    var spellKnownTableGuid = GenerateGuid(spellKnownTable.ToLower(), 0);
                    if (!Standard.KnownSpellsTables.Contains(spellKnownTableGuid))
                        ImportKnownSpellsTable(spellKnownTable, spellKnownTableGuid);
                    tmpClass.KnownSpells = Standard.KnownSpellsTables.GetByID(spellKnownTableGuid);
                }

                tmpClass.Playable = classes2da[i].AsBoolean("PlayerClass");
                tmpClass.IsSpellCaster = classes2da[i].AsBoolean("SpellCaster");
                tmpClass.RecommendedStr = classes2da[i].AsInteger("Str") ?? 0;
                tmpClass.RecommendedDex = classes2da[i].AsInteger("Dex") ?? 0;
                tmpClass.RecommendedCon = classes2da[i].AsInteger("Con") ?? 0;
                tmpClass.RecommendedWis = classes2da[i].AsInteger("Wis") ?? 0;
                tmpClass.RecommendedInt = classes2da[i].AsInteger("Int") ?? 0;
                tmpClass.RecommendedCha = classes2da[i].AsInteger("Cha") ?? 0;
                tmpClass.PrimaryAbility = Enum.Parse<AbilityType>(classes2da[i].AsString("PrimaryAbil") ?? "", true);

                // Alignment
                var alignFlags = classes2da[i].AsInteger("AlignRestrict") ?? 0;
                var alignAxis = classes2da[i].AsInteger("AlignRstrctType") ?? 0;
                var alignInvert = classes2da[i].AsBoolean("InvertRestrict");
                tmpClass.AllowedAlignments = Alignments.Create(alignFlags, alignAxis, alignInvert);

                // Prerequesites
                var preRequTable = classes2da[i].AsString("PreReqTable");
                if (preRequTable != null)
                {
                    var preRequTableGuid = GenerateGuid(preRequTable.ToLower(), 0);
                    if (!Standard.PrerequisiteTables.Contains(preRequTableGuid))
                        ImportPrerequisiteTable(preRequTable, preRequTableGuid);
                    tmpClass.Requirements = Standard.PrerequisiteTables.GetByID(preRequTableGuid);
                }

                tmpClass.MaxLevel = classes2da[i].AsInteger("MaxLevel") ?? 0;
                tmpClass.MulticlassXPPenalty = classes2da[i].AsBoolean("XPPenalty");
                tmpClass.ArcaneCasterLevelMod = classes2da[i].AsInteger("ArcSpellLvlMod") ?? 0;
                tmpClass.DivineCasterLevelMod = classes2da[i].AsInteger("DivSpellLvlMod") ?? 0;
                tmpClass.PreEpicMaxLevel = classes2da[i].AsInteger("EpicLevel") ?? 0;
                tmpClass.DefaultPackage = CreateRef<ClassPackage>(classes2da[i].AsInteger("Package"));

                // StatGainTable
                var statGainTable = classes2da[i].AsString("StatGainTable");
                if (statGainTable != null)
                {
                    var statGainTableGuid = GenerateGuid(statGainTable.ToLower(), 0);
                    if (!Standard.StatGainTables.Contains(statGainTableGuid))
                        ImportStatGainTable(statGainTable, statGainTableGuid);
                    tmpClass.StatGainTable = Standard.StatGainTables.GetByID(statGainTableGuid);
                }

                tmpClass.MemorizesSpells = classes2da[i].AsBoolean("MemorizesSpells");
                tmpClass.SpellbookRestricted = classes2da[i].AsBoolean("SpellbookRestricted");
                tmpClass.PicksDomain = classes2da[i].AsBoolean("PickDomains");
                tmpClass.PicksSchool = classes2da[i].AsBoolean("PickSchool");
                tmpClass.CanLearnFromScrolls = classes2da[i].AsBoolean("LearnScroll");
                tmpClass.IsArcaneCaster = classes2da[i].AsBoolean("Arcane");
                tmpClass.HasSpellFailure = classes2da[i].AsBoolean("ASF");

                var spellAbilityStr = classes2da[i].AsString("SpellcastingAbil") ?? "";
                if (spellAbilityStr != "")
                    tmpClass.SpellcastingAbility = Enum.Parse<AbilityType>(spellAbilityStr, true);

                // Spellbook
                var spellbookName = classes2da[i].AsString("SpellTableColumn");
                if (spellbookName != null)
                {
                    if (!((SpellbookRepository)Standard.Spellbooks).Contains(spellbookName))
                    {
                        Spellbook tmpSpellbook = new Spellbook();
                        tmpSpellbook.ID = GenerateGuid(spellbookName.ToLower(), 0);
                        tmpSpellbook.Name = spellbookName;
                        Standard.Spellbooks.Add(tmpSpellbook);
                    }

                    tmpClass.Spellbook = ((SpellbookRepository)Standard.Spellbooks).GetByName(spellbookName);
                }

                tmpClass.CasterLevelMultiplier = classes2da[i].AsFloat("CLMultiplier") ?? 1.0;
                tmpClass.MinCastingLevel = classes2da[i].AsInteger("MinCastingLevel") ?? 0;
                tmpClass.MinAssociateLevel = classes2da[i].AsInteger("MinAssociateLevel") ?? 0;
                tmpClass.CanCastSpontaneously = classes2da[i].AsBoolean("CanCastSpontaneously");
                tmpClass.SkipSpellSelection = classes2da[i].AsBoolean("SkipSpellSelection", null);

                Standard.Classes.Add(tmpClass);
            }
        }

        private void ImportDomains()
        {
            var domains2da = Load2da("domains");

            Standard.Domains.Clear();
            for (int i = 0; i < domains2da.Count; i++)
            {
                var tmpDomain = new Domain();
                tmpDomain.ID = GenerateGuid("domains", i);
                tmpDomain.Index = i;

                if (!SetText(tmpDomain.Name, domains2da[i].AsInteger("Name"))) continue;
                SetText(tmpDomain.Description, domains2da[i].AsInteger("Description"));

                tmpDomain.Icon = AddIconResource(domains2da[i].AsString("Icon"));
                tmpDomain.Level0Spell = CreateRef<Spell>(domains2da[i].AsInteger("Level_0", -1));
                tmpDomain.Level1Spell = CreateRef<Spell>(domains2da[i].AsInteger("Level_1"));
                tmpDomain.Level2Spell = CreateRef<Spell>(domains2da[i].AsInteger("Level_2"));
                tmpDomain.Level3Spell = CreateRef<Spell>(domains2da[i].AsInteger("Level_3"));
                tmpDomain.Level4Spell = CreateRef<Spell>(domains2da[i].AsInteger("Level_4"));
                tmpDomain.Level5Spell = CreateRef<Spell>(domains2da[i].AsInteger("Level_5"));
                tmpDomain.Level6Spell = CreateRef<Spell>(domains2da[i].AsInteger("Level_6"));
                tmpDomain.Level7Spell = CreateRef<Spell>(domains2da[i].AsInteger("Level_7"));
                tmpDomain.Level8Spell = CreateRef<Spell>(domains2da[i].AsInteger("Level_8"));
                tmpDomain.Level9Spell = CreateRef<Spell>(domains2da[i].AsInteger("Level_9"));

                tmpDomain.GrantedFeat = CreateRef<Feat>(domains2da[i].AsInteger("GrantedFeat"));
                tmpDomain.FeatIsActive = domains2da[i].AsBoolean("CastableFeat");

                Standard.Domains.Add(tmpDomain);
            }
        }

        private void ImportSkills()
        {
            var skills2da = Load2da("skills");

            Standard.Skills.Clear();
            for (int i = 0; i < skills2da.Count; i++)
            {
                var tmpSkill = new Skill();
                tmpSkill.ID = GenerateGuid("skills", i);
                tmpSkill.Index = i;

                if (!SetText(tmpSkill.Name, skills2da[i].AsInteger("Name"))) continue;
                SetText(tmpSkill.Description, skills2da[i].AsInteger("Description"));

                tmpSkill.Icon = AddIconResource(skills2da[i].AsString("Icon"));
                tmpSkill.CanUseUntrained = skills2da[i].AsBoolean("Untrained");
                tmpSkill.KeyAbility = Enum.Parse<AbilityType>(skills2da[i].AsString("KeyAbility") ?? "", true);
                tmpSkill.UseArmorPenalty = skills2da[i].AsBoolean("ArmorCheckPenalty");
                tmpSkill.AllClassesCanUse = skills2da[i].AsBoolean("AllClassesCanUse");
                tmpSkill.IsHostile = skills2da[i].AsBoolean("HostileSkill");
                tmpSkill.HideFromLevelUp = skills2da[i].AsBoolean("HideFromLevelUp", false);

                Standard.Skills.Add(tmpSkill);
            }
        }

        private void ImportCutFeat(TwoDimensionalArrayFile feat2da, Feat tmpFeat, int index)
        {
            switch (index)
            {
                case 480: // Blinding Spittle
                    SetText(tmpFeat.Name, 110691);
                    SetText(tmpFeat.Description, 110692);
                    tmpFeat.Hint = "Cut";
                    tmpFeat.MinLevelClass = CreateRef<CharacterClass>(39);
                    tmpFeat.MinLevel = 4;
                    tmpFeat.Icon = "ife_X1GFSPen";
                    tmpFeat.Category = AICategory.HarmfulRanged;
                    tmpFeat.UsesPerDay = 2;
                    tmpFeat.UseableByAllClasses = false;
                    tmpFeat.SuccessorFeat = CreateRef<Feat>(481);
                    tmpFeat.ToolsetCategory = FeatCategory.ClassOrRacial;
                    tmpFeat.IsHostile = true;
                    tmpFeat.UseActionQueue = true;
                    tmpFeat.CRModifier = 0.5;

                    Standard.Feats.Add(tmpFeat);
                    break;

                case 481: // Blinding Spittle 2
                    SetText(tmpFeat.Name, 110691);
                    SetText(tmpFeat.Description, 110692);
                    tmpFeat.Hint = "Cut";
                    tmpFeat.MinLevelClass = CreateRef<CharacterClass>(39);
                    tmpFeat.MinLevel = 7;
                    tmpFeat.Icon = "ife_X1GFSPen";
                    tmpFeat.Category = AICategory.HarmfulRanged;
                    tmpFeat.UsesPerDay = 4;
                    tmpFeat.UseableByAllClasses = false;
                    tmpFeat.ToolsetCategory = FeatCategory.ClassOrRacial;
                    tmpFeat.IsHostile = true;
                    tmpFeat.UseActionQueue = true;
                    tmpFeat.CRModifier = 0.5;

                    Standard.Feats.Add(tmpFeat);
                    break;

                case 482: // Command the Horde
                    SetText(tmpFeat.Name, 110693);
                    SetText(tmpFeat.Description, 110694);
                    tmpFeat.Hint = "Cut";
                    tmpFeat.MinLevelClass = CreateRef<CharacterClass>(39);
                    tmpFeat.MinLevel = 1;
                    tmpFeat.Icon = "ife_barbrage";
                    tmpFeat.Category = AICategory.FriendlyEnhancementAoe;
                    tmpFeat.UseableByAllClasses = false;
                    tmpFeat.ToolsetCategory = FeatCategory.ClassOrRacial;
                    tmpFeat.IsHostile = false;
                    tmpFeat.UseActionQueue = true;
                    tmpFeat.CRModifier = 0.5;

                    Standard.Feats.Add(tmpFeat);
                    break;

                case 483: // Swing Blindly
                    SetText(tmpFeat.Name, 110695);
                    SetText(tmpFeat.Description, 110696);
                    tmpFeat.Hint = "Cut";
                    tmpFeat.MinLevelClass = CreateRef<CharacterClass>(39);
                    tmpFeat.MinLevel = 2;
                    tmpFeat.Icon = "ife_X1BliFig";
                    tmpFeat.Category = AICategory.FriendlyEnhancementSelf;
                    tmpFeat.UseableByAllClasses = false;
                    tmpFeat.ToolsetCategory = FeatCategory.ClassOrRacial;
                    tmpFeat.IsHostile = false;
                    tmpFeat.UseActionQueue = false;
                    tmpFeat.CRModifier = 0.5;

                    Standard.Feats.Add(tmpFeat);
                    break;

                case 484: // Ritual Scarring
                    SetText(tmpFeat.Name, 110697);
                    SetText(tmpFeat.Description, 110698);
                    tmpFeat.Hint = "Cut";
                    tmpFeat.MinLevelClass = CreateRef<CharacterClass>(39);
                    tmpFeat.MinLevel = 3;
                    tmpFeat.Icon = "ife_X1Blood";
                    tmpFeat.Category = AICategory.FriendlyEnhancementSelf;
                    tmpFeat.UseableByAllClasses = false;
                    tmpFeat.ToolsetCategory = FeatCategory.ClassOrRacial;
                    tmpFeat.IsHostile = false;
                    tmpFeat.UseActionQueue = false;
                    tmpFeat.CRModifier = 0.5;

                    Standard.Feats.Add(tmpFeat);
                    break;

                case 485: // Blindsight 5 Feet
                    SetText(tmpFeat.Name, 110699);
                    SetText(tmpFeat.Description, 110700);
                    tmpFeat.Hint = "Cut";
                    tmpFeat.MinLevelClass = CreateRef<CharacterClass>(39);
                    tmpFeat.MinLevel = 5;
                    tmpFeat.Icon = "ife_x2blindsigh";
                    tmpFeat.Category = AICategory.FriendlyEnhancementSelf;
                    tmpFeat.UseableByAllClasses = false;
                    tmpFeat.ToolsetCategory = FeatCategory.ClassOrRacial;
                    tmpFeat.SuccessorFeat = CreateRef<Feat>(486);
                    tmpFeat.IsHostile = false;
                    tmpFeat.UseActionQueue = false;
                    tmpFeat.CRModifier = 0.5;

                    Standard.Feats.Add(tmpFeat);
                    break;

                case 486: // Blindsight 10 Feet
                    SetText(tmpFeat.Name, 110701);
                    SetText(tmpFeat.Description, 110702);
                    tmpFeat.Hint = "Cut";
                    tmpFeat.MinLevelClass = CreateRef<CharacterClass>(39);
                    tmpFeat.MinLevel = 8;
                    tmpFeat.Icon = "ife_x2blindsigh";
                    tmpFeat.Category = AICategory.FriendlyEnhancementSelf;
                    tmpFeat.UseableByAllClasses = false;
                    tmpFeat.ToolsetCategory = FeatCategory.ClassOrRacial;
                    tmpFeat.IsHostile = false;
                    tmpFeat.UseActionQueue = false;
                    tmpFeat.CRModifier = 0.5;

                    Standard.Feats.Add(tmpFeat);
                    break;

                case 487: // Sight of Gruumsh
                    SetText(tmpFeat.Name, 110703);
                    SetText(tmpFeat.Description, 110704);
                    tmpFeat.Hint = "Cut";
                    tmpFeat.MinLevelClass = CreateRef<CharacterClass>(39);
                    tmpFeat.MinLevel = 10;
                    tmpFeat.Icon = "ife_wizard";
                    tmpFeat.Category = AICategory.FriendlyEnhancementSelf;
                    tmpFeat.UseableByAllClasses = false;
                    tmpFeat.ToolsetCategory = FeatCategory.ClassOrRacial;
                    tmpFeat.IsHostile = false;
                    tmpFeat.UseActionQueue = false;
                    tmpFeat.CRModifier = 0.5;

                    Standard.Feats.Add(tmpFeat);
                    break;

                case 489: // Dodge Bonus + 2 (Shou Disciple)
                    SetText(tmpFeat.Name, 110711);
                    SetText(tmpFeat.Description, 110712);
                    tmpFeat.Hint = "Cut";
                    tmpFeat.MinLevelClass = CreateRef<CharacterClass>(40);
                    tmpFeat.MinLevel = 2;
                    tmpFeat.Icon = "ife_dodge";
                    tmpFeat.Category = AICategory.FriendlyEnhancementSelf;
                    tmpFeat.UseableByAllClasses = false;
                    tmpFeat.ToolsetCategory = FeatCategory.ClassOrRacial;
                    tmpFeat.SuccessorFeat = CreateRef<Feat>(1031);
                    tmpFeat.IsHostile = false;
                    tmpFeat.UseActionQueue = false;
                    tmpFeat.CRModifier = 0.5;

                    Standard.Feats.Add(tmpFeat);
                    break;

                case 866: // Martial Flurry (light)
                    SetText(tmpFeat.Name, 110715);
                    SetText(tmpFeat.Description, 110716);
                    tmpFeat.Hint = "Cut";
                    tmpFeat.MinLevelClass = CreateRef<CharacterClass>(40);
                    tmpFeat.MinLevel = 3;
                    tmpFeat.Icon = "ife_flurry";
                    tmpFeat.Category = AICategory.FriendlyEnhancementSelf;
                    tmpFeat.UseableByAllClasses = false;
                    tmpFeat.ToolsetCategory = FeatCategory.ClassOrRacial;
                    tmpFeat.SuccessorFeat = CreateRef<Feat>(899);
                    tmpFeat.IsHostile = false;
                    tmpFeat.UseActionQueue = false;
                    tmpFeat.CRModifier = 1.0;

                    Standard.Feats.Add(tmpFeat);
                    break;

                case 899: // Martial Flurry (light)
                    SetText(tmpFeat.Name, 110717);
                    SetText(tmpFeat.Description, 110718);
                    tmpFeat.Hint = "Cut";
                    tmpFeat.MinLevelClass = CreateRef<CharacterClass>(40);
                    tmpFeat.MinLevel = 5;
                    tmpFeat.Icon = "ife_flurry";
                    tmpFeat.Category = AICategory.FriendlyEnhancementSelf;
                    tmpFeat.UseableByAllClasses = false;
                    tmpFeat.ToolsetCategory = FeatCategory.ClassOrRacial;
                    tmpFeat.IsHostile = false;
                    tmpFeat.UseActionQueue = false;
                    tmpFeat.CRModifier = 1.0;

                    Standard.Feats.Add(tmpFeat);
                    break;

                case 1031: // Dodge Bonus + 3 (Shou Disciple)
                    SetText(tmpFeat.Name, 110713);
                    SetText(tmpFeat.Description, 110714);
                    tmpFeat.Hint = "Cut";
                    tmpFeat.MinLevelClass = CreateRef<CharacterClass>(40);
                    tmpFeat.MinLevel = 4;
                    tmpFeat.Icon = "ife_dodge";
                    tmpFeat.Category = AICategory.FriendlyEnhancementSelf;
                    tmpFeat.UseableByAllClasses = false;
                    tmpFeat.ToolsetCategory = FeatCategory.ClassOrRacial;
                    tmpFeat.IsHostile = false;
                    tmpFeat.UseActionQueue = false;
                    tmpFeat.CRModifier = 0.5;

                    Standard.Feats.Add(tmpFeat);
                    break;

                // General cut Feats

                case 385: // Smooth Talk
                    tmpFeat.Name.OriginalIndex = -1;
                    tmpFeat.Name[TLKLanguage.English].Text = "Smooth Talk";
                    tmpFeat.Name[TLKLanguage.English].TextF = tmpFeat.Name[TLKLanguage.English].Text;
                    tmpFeat.Hint = "Cut";
                    tmpFeat.Icon = "ife_x1smooth";
                    tmpFeat.Category = AICategory.FriendlyEnhancementSelf;
                    tmpFeat.UseableByAllClasses = true;
                    tmpFeat.ToolsetCategory = FeatCategory.Other;
                    tmpFeat.IsHostile = false;
                    tmpFeat.UseActionQueue = false;

                    Standard.Feats.Add(tmpFeat);
                    break;

                case 403: // Mercantile Background
                    tmpFeat.Name.OriginalIndex = -1;
                    tmpFeat.Name[TLKLanguage.English].Text = "Mercantile Background";
                    tmpFeat.Name[TLKLanguage.English].TextF = tmpFeat.Name[TLKLanguage.English].Text;
                    tmpFeat.Hint = "Cut";
                    tmpFeat.Icon = "ife_x1merch";
                    tmpFeat.Category = AICategory.FriendlyEnhancementSelf;
                    tmpFeat.UseableByAllClasses = true;
                    tmpFeat.ToolsetCategory = FeatCategory.Other;
                    tmpFeat.IsHostile = false;
                    tmpFeat.UseActionQueue = false;

                    Standard.Feats.Add(tmpFeat);
                    break;

                case 991: // Master Crafter
                    SetText(tmpFeat.Name, 83633);
                    SetText(tmpFeat.Description, 83634);
                    tmpFeat.Hint = "Cut";
                    tmpFeat.MinLevelClass = CreateRef<CharacterClass>(28); // Harper
                    tmpFeat.MinLevel = 10;
                    tmpFeat.Icon = "ife_X2MstCraft";
                    tmpFeat.UseableByAllClasses = false;
                    tmpFeat.ToolsetCategory = FeatCategory.ClassOrRacial;
                    tmpFeat.IsHostile = false;
                    tmpFeat.UseActionQueue = false;

                    Standard.Feats.Add(tmpFeat);
                    break;

                case 992: // Scrounger
                    SetText(tmpFeat.Name, 83635);
                    SetText(tmpFeat.Description, 83636);
                    tmpFeat.Hint = "Cut";
                    tmpFeat.MinLevelClass = CreateRef<CharacterClass>(28); // Harper
                    tmpFeat.MinLevel = 6;
                    tmpFeat.Icon = "ife_X2MstCraft";
                    tmpFeat.UseableByAllClasses = false;
                    tmpFeat.ToolsetCategory = FeatCategory.ClassOrRacial;
                    tmpFeat.IsHostile = false;
                    tmpFeat.UseActionQueue = false;

                    Standard.Feats.Add(tmpFeat);
                    break;
            }
        }

        private void ImportFeats()
        {
            var feat2da = Load2da("feat");

            Standard.Feats.Clear();
            for (int i = 0; i < feat2da.Count; i++)
            {
                var tmpFeat = new Feat();
                tmpFeat.ID = GenerateGuid("feat", i);
                tmpFeat.Index = i;

                if (!SetText(tmpFeat.Name, feat2da[i].AsInteger("FEAT")))
                {
                    ImportCutFeat(feat2da, tmpFeat, i);
                    continue;
                }
                SetText(tmpFeat.Description, feat2da[i].AsInteger("DESCRIPTION"));

                tmpFeat.Icon = AddIconResource(feat2da[i].AsString("ICON"));
                tmpFeat.MinAttackBonus = feat2da[i].AsInteger("MINATTACKBONUS");
                tmpFeat.MinStr = feat2da[i].AsInteger("MINSTR");
                tmpFeat.MinDex = feat2da[i].AsInteger("MINDEX");
                tmpFeat.MinInt = feat2da[i].AsInteger("MININT");
                tmpFeat.MinWis = feat2da[i].AsInteger("MINWIS");
                tmpFeat.MinCon = feat2da[i].AsInteger("MINCON");
                tmpFeat.MinCha = feat2da[i].AsInteger("MINCHA");
                tmpFeat.MinSpellLevel = feat2da[i].AsInteger("MINSPELLLVL");
                tmpFeat.RequiredFeat1 = CreateRef<Feat>(feat2da[i].AsInteger("PREREQFEAT1"));
                tmpFeat.RequiredFeat2 = CreateRef<Feat>(feat2da[i].AsInteger("PREREQFEAT2"));
                tmpFeat.UseableByAllClasses = feat2da[i].AsBoolean("ALLCLASSESCANUSE");
                tmpFeat.Category = !feat2da[i].IsNull("CATEGORY") ? (AICategory)Enum.ToObject(typeof(AICategory), feat2da[i].AsInteger("CATEGORY") ?? 0) : null;
                tmpFeat.OnUseEffect = CreateRef<Spell>(feat2da[i].AsInteger("SPELLID"));
                tmpFeat.SuccessorFeat = CreateRef<Feat>(feat2da[i].AsInteger("SUCCESSOR"));
                tmpFeat.CRModifier = feat2da[i].AsFloat("CRValue");
                tmpFeat.UsesPerDay = feat2da[i].AsInteger("USESPERDAY");
                tmpFeat.MasterFeat = CreateRef<Feat>(feat2da[i].AsInteger("MASTERFEAT"));
                tmpFeat.TargetSelf = feat2da[i].AsBoolean("TARGETSELF");
                tmpFeat.RequiredFeatSelection1 = CreateRef<Feat>(feat2da[i].AsInteger("OrReqFeat0"));
                tmpFeat.RequiredFeatSelection2 = CreateRef<Feat>(feat2da[i].AsInteger("OrReqFeat1"));
                tmpFeat.RequiredFeatSelection3 = CreateRef<Feat>(feat2da[i].AsInteger("OrReqFeat2"));
                tmpFeat.RequiredFeatSelection4 = CreateRef<Feat>(feat2da[i].AsInteger("OrReqFeat3"));
                tmpFeat.RequiredFeatSelection5 = CreateRef<Feat>(feat2da[i].AsInteger("OrReqFeat4"));
                tmpFeat.RequiredSkill1 = CreateRef<Skill>(feat2da[i].AsInteger("REQSKILL"));
                tmpFeat.RequiredSkill1Minimum = feat2da[i].AsInteger("ReqSkillMinRanks");
                tmpFeat.RequiredSkill2 = CreateRef<Skill>(feat2da[i].AsInteger("REQSKILL2"));
                tmpFeat.RequiredSkill2Minimum = feat2da[i].AsInteger("ReqSkillMinRanks2");
                tmpFeat.ToolsetCategory = (FeatCategory)Enum.ToObject(typeof(FeatCategory), feat2da[i].AsInteger("TOOLSCATEGORIES") ?? 0);
                tmpFeat.IsHostile = feat2da[i].AsBoolean("HostileFeat");
                tmpFeat.MinLevel = feat2da[i].AsInteger("MinLevel");
                tmpFeat.MinLevelClass = CreateRef<CharacterClass>(feat2da[i].AsInteger("MinLevelClass"));
                tmpFeat.MaxLevel = feat2da[i].AsInteger("MaxLevel");
                tmpFeat.MinFortitudeSave = feat2da[i].AsInteger("MinFortSave");
                tmpFeat.RequiresEpic = feat2da[i].AsBoolean("PreReqEpic");
                tmpFeat.UseActionQueue = feat2da[i].AsBoolean("ReqAction");

                Standard.Feats.Add(tmpFeat);
            }
        }

        private void ImportSpells()
        {
            var spells2da = Load2da("spells");

            Standard.Spells.Clear();
            Standard.Spells.BeginUpdate();
            for (int i = 0; i < spells2da.Count; i++)
            {
                var tmpSpell = new Spell();
                tmpSpell.ID = GenerateGuid("spells", i);
                tmpSpell.Index = i;

                if (!SetText(tmpSpell.Name, spells2da[i].AsInteger("Name"))) continue;
                SetText(tmpSpell.Description, spells2da[i].AsInteger("SpellDesc"));
                SetText(tmpSpell.AlternativeCastMessage, spells2da[i].AsInteger("AltMessage"));

                tmpSpell.Icon = AddIconResource(spells2da[i].AsString("IconResRef"));
                tmpSpell.School = Enum.Parse<SpellSchool>(spells2da[i].AsString("School") ?? "", true);
                tmpSpell.Range = Enum.Parse<SpellRange>(spells2da[i].AsString("Range") ?? "", true);

                var componentStr = spells2da[i].AsString("VS") ?? "";
                SpellComponent components = 0;
                if (componentStr.Contains('v'))
                    components |= SpellComponent.V;
                if (componentStr.Contains('s'))
                    components |= SpellComponent.S;
                tmpSpell.Components = components;
                tmpSpell.AvailableMetaMagic = (MetaMagicType)(spells2da[i].AsInteger("MetaMagic") ?? 0);
                tmpSpell.TargetTypes = (SpellTarget)(spells2da[i].AsInteger("TargetType") ?? 0);
                tmpSpell.ImpactScript = spells2da[i].AsString("ImpactScript");
                tmpSpell.InnateLevel = spells2da[i].AsInteger("Innate") ?? 0;

                tmpSpell.ConjurationTime = spells2da[i].AsInteger("ConjTime") ?? 1500;
                tmpSpell.ConjuringAnimation = !spells2da[i].IsNull("ConjAnim") ? Enum.Parse<SpellConjureAnimation>(spells2da[i].AsString("ConjAnim") ?? "", true) : null;
                tmpSpell.ConjurationHeadEffect = spells2da[i].AsString("ConjHeadVisual");
                tmpSpell.ConjurationHandEffect = spells2da[i].AsString("ConjHandVisual");
                tmpSpell.ConjurationGroundEffect = spells2da[i].AsString("ConjGrndVisual");
                tmpSpell.ConjurationSound = spells2da[i].AsString("ConjSoundVFX");
                tmpSpell.ConjurationMaleSound = spells2da[i].AsString("ConjSoundMale");
                tmpSpell.ConjurationFemaleSound = spells2da[i].AsString("ConjSoundFemale");
                tmpSpell.CastingAnimation = !spells2da[i].IsNull("CastAnim") ? Enum.Parse<SpellCastAnimation>(spells2da[i].AsString("CastAnim") ?? "", true) : null;
                tmpSpell.CastTime = spells2da[i].AsInteger("CastTime") ?? 1000;
                tmpSpell.CastingHeadEffect = spells2da[i].AsString("CastHeadVisual");
                tmpSpell.CastingHandEffect = spells2da[i].AsString("CastHandVisual");
                tmpSpell.CastingGroundEffect = spells2da[i].AsString("CastGrndVisual");
                tmpSpell.CastingSound = spells2da[i].AsString("CastSound");
                tmpSpell.HasProjectile = spells2da[i].AsBoolean("Proj");
                tmpSpell.ProjectileModel = spells2da[i].AsString("ProjModel");
                tmpSpell.ProjectileType = !spells2da[i].IsNull("ProjType") ? Enum.Parse<ProjectileType>(spells2da[i].AsString("ProjType") ?? "", true) : null;
                tmpSpell.ProjectileSpawnPoint = !spells2da[i].IsNull("ProjSpwnPoint") ? Enum.Parse<ProjectileSource>(spells2da[i].AsString("ProjSpwnPoint") ?? "", true) : null;
                tmpSpell.ProjectileSound = spells2da[i].AsString("ProjSound");
                tmpSpell.ProjectileOrientation = !spells2da[i].IsNull("ProjOrientation") ? Enum.Parse<ProjectileOrientation>(spells2da[i].AsString("ProjOrientation") ?? "", true) : null;

                tmpSpell.SubSpell1 = CreateRef<Spell>(spells2da[i].AsInteger("SubRadSpell1"));
                tmpSpell.SubSpell2 = CreateRef<Spell>(spells2da[i].AsInteger("SubRadSpell2"));
                tmpSpell.SubSpell3 = CreateRef<Spell>(spells2da[i].AsInteger("SubRadSpell3"));
                tmpSpell.SubSpell4 = CreateRef<Spell>(spells2da[i].AsInteger("SubRadSpell4"));
                tmpSpell.SubSpell5 = CreateRef<Spell>(spells2da[i].AsInteger("SubRadSpell5"));
                tmpSpell.SubSpell6 = CreateRef<Spell>(spells2da[i].AsInteger("SubRadSpell6", -1));
                tmpSpell.SubSpell7 = CreateRef<Spell>(spells2da[i].AsInteger("SubRadSpell7", -1));
                tmpSpell.SubSpell8 = CreateRef<Spell>(spells2da[i].AsInteger("SubRadSpell8", -1));

                tmpSpell.Category = !spells2da[i].IsNull("Category") ? (AICategory)Enum.ToObject(typeof(AICategory), spells2da[i].AsInteger("Category") ?? 0) : null;
                tmpSpell.ParentSpell = CreateRef<Spell>(spells2da[i].AsInteger("Master"));
                tmpSpell.Type = !spells2da[i].IsNull("UserType") ? (SpellType)Enum.ToObject(typeof(SpellType), spells2da[i].AsInteger("UserType") ?? 0) : SpellType.Other;
                tmpSpell.UseConcentration = spells2da[i].AsBoolean("UseConcentration");
                tmpSpell.IsCastSpontaneously = spells2da[i].AsBoolean("SpontaneouslyCast");
                tmpSpell.IsHostile = spells2da[i].AsBoolean("HostileSetting");
                tmpSpell.CounterSpell1 = CreateRef<Spell>(spells2da[i].AsInteger("Counter1"));
                tmpSpell.CounterSpell2 = CreateRef<Spell>(spells2da[i].AsInteger("Counter2"));
                tmpSpell.HasProjectileVisuals = tmpSpell.IsHostile = spells2da[i].AsBoolean("HasProjectile");
                tmpSpell.TargetShape = !spells2da[i].IsNull("TargetShape", false) ? Enum.Parse<TargetShape>(spells2da[i].AsString("TargetShape", null) ?? "", true) : null;
                tmpSpell.TargetSizeX = spells2da[i].AsFloat("TargetSizeX", null);
                tmpSpell.TargetSizeY = spells2da[i].AsFloat("TargetSizeY", null);
                tmpSpell.TargetingFlags = (TargetFlag?)spells2da[i].AsInteger("TargetFlags", null);

                // Spellbook entries:
                for (int j = 0; j < spells2da.Columns.Count; j++)
                {
                    var spellbook = ((SpellbookRepository)Standard.Spellbooks).GetByName(spells2da.Columns[j]);
                    if (spellbook != null)
                    {
                        var level = spells2da[i].AsInteger(j);
                        if (level != null)
                            spellbook.AddSpell(level ?? 99, tmpSpell);
                    }
                }


                Standard.Spells.Add(tmpSpell);
            }
            Standard.Spells.EndUpdate();
        }

        private void ImportDiseases()
        {
            var disease2da = Load2da("disease");

            Standard.Diseases.Clear();
            for (int i = 0; i < disease2da.Count; i++)
            {
                var tmpDisease = new Disease();
                tmpDisease.ID = GenerateGuid("disease", i);
                tmpDisease.Index = i;

                if (!SetText(tmpDisease.Name, disease2da[i].AsInteger("Name"))) continue;

                tmpDisease.FirstSaveDC = disease2da[i].AsInteger("First_Save") ?? 0;
                tmpDisease.SecondSaveDC = disease2da[i].AsInteger("Subs_Save") ?? 0;
                tmpDisease.IncubationHours = disease2da[i].AsInteger("Incu_Hours") ?? 1;
                tmpDisease.AbilityDamage1Dice = disease2da[i].AsInteger("Dice_1") ?? 1;
                tmpDisease.AbilityDamage1DiceCount = disease2da[i].AsInteger("Dam_1") ?? 1;
                tmpDisease.AbilityDamage1Type = !disease2da[i].IsNull("Type_1") ? (AbilityType)Enum.ToObject(typeof(SpellType), disease2da[i].AsInteger("Type_1") ?? 0) : null;
                tmpDisease.AbilityDamage2Dice = disease2da[i].AsInteger("Dice_2") ?? 1;
                tmpDisease.AbilityDamage2DiceCount = disease2da[i].AsInteger("Dam_2") ?? 1;
                tmpDisease.AbilityDamage2Type = !disease2da[i].IsNull("Type_2") ? (AbilityType)Enum.ToObject(typeof(SpellType), disease2da[i].AsInteger("Type_2") ?? 0) : null;
                tmpDisease.AbilityDamage3Dice = disease2da[i].AsInteger("Dice_3") ?? 1;
                tmpDisease.AbilityDamage3DiceCount = disease2da[i].AsInteger("Dam_3") ?? 1;
                tmpDisease.AbilityDamage3Type = !disease2da[i].IsNull("Type_3") ? (AbilityType)Enum.ToObject(typeof(SpellType), disease2da[i].AsInteger("Type_3") ?? 0) : null;
                tmpDisease.IncubationEndScript = disease2da[i].AsString("End_Incu_Script");
                tmpDisease.DailyEffectScript = disease2da[i].AsString("24_Hour_Script");

                Standard.Diseases.Add(tmpDisease);
            }
        }

        private void ImportPoisons()
        {
            var poison2da = Load2da("poison");

            Standard.Poisons.Clear();
            for (int i = 0; i < poison2da.Count; i++)
            {
                var tmpPoison = new Poison();
                tmpPoison.ID = GenerateGuid("poison", i);
                tmpPoison.Index = i;

                if (!SetText(tmpPoison.Name, poison2da[i].AsInteger("Name"))) continue;

                tmpPoison.SaveDC = poison2da[i].AsInteger("Save_DC") ?? 10;
                tmpPoison.HandleDC = poison2da[i].AsInteger("Handle_DC") ?? 10;
                tmpPoison.InitialAbilityDamageDice = poison2da[i].AsInteger("Dice_1") ?? 0;
                tmpPoison.InitialAbilityDamageDiceCount = poison2da[i].AsInteger("Dam_1") ?? 0;
                tmpPoison.InitialAbilityDamageType = !poison2da[i].IsNull("Default_1") ? Enum.Parse<AbilityType>(poison2da[i].AsString("Default_1") ?? "", true) : null;
                tmpPoison.InitialEffectScript = poison2da[i].AsString("Script_1");
                tmpPoison.SecondaryAbilityDamageDice = poison2da[i].AsInteger("Dice_2") ?? 0;
                tmpPoison.SecondaryAbilityDamageDiceCount = poison2da[i].AsInteger("Dam_2") ?? 0;
                tmpPoison.SecondaryAbilityDamageType = !poison2da[i].IsNull("Default_2") ? Enum.Parse<AbilityType>(poison2da[i].AsString("Default_2") ?? "", true) : null;
                tmpPoison.SecondaryEffectScript = poison2da[i].AsString("Script_2");
                tmpPoison.Cost = poison2da[i].AsFloat("Cost") ?? 0.0;
                tmpPoison.OnHitApplied = poison2da[i].AsBoolean("OnHitApplied");
                tmpPoison.ImpactVFX = poison2da[i].AsString("VFX_Impact");

                Standard.Poisons.Add(tmpPoison);
            }
        }

        private void ImportClassPackages()
        {
            var packages2da = Load2da("packages");

            Standard.ClassPackages.Clear();
            for (int i = 0; i < packages2da.Count; i++)
            {
                var tmpPackage = new ClassPackage();
                tmpPackage.ID = GenerateGuid("packages", i);
                tmpPackage.Index = i;

                if (!SetText(tmpPackage.Name, packages2da[i].AsInteger("Name"))) continue;
                SetText(tmpPackage.Description, packages2da[i].AsInteger("Description"));

                tmpPackage.ForClass = CreateRef<CharacterClass>(packages2da[i].AsInteger("ClassID", -1));
                tmpPackage.PreferredAbility = Enum.Parse<AbilityType>(packages2da[i].AsString("Attribute") ?? "", true);
                tmpPackage.Gold = packages2da[i].AsInteger("Gold") ?? 0;
                tmpPackage.SpellSchool = !packages2da[i].IsNull("School") ? (SpellSchool)Enum.ToObject(typeof(SpellSchool), packages2da[i].AsInteger("School") ?? 0) : null;
                tmpPackage.Domain1 = CreateRef<Domain>(packages2da[i].AsInteger("Domain1", -1));
                tmpPackage.Domain2 = CreateRef<Domain>(packages2da[i].AsInteger("Domain2", -1));
                tmpPackage.Associate = IntPtr.Zero; // ! (Associate)
                tmpPackage.SpellPreferences = IntPtr.Zero; // ! (SpellPref2DA)
                tmpPackage.FeatPreferences = IntPtr.Zero; // ! (FeatPref2DA)
                tmpPackage.SkillPreferences = IntPtr.Zero; // ! (SkillPref2DA)
                tmpPackage.StartingEquipment = IntPtr.Zero; // ! (Equip2DA)
                tmpPackage.Playable = packages2da[i].AsBoolean("PlayerClass");

                Standard.ClassPackages.Add(tmpPackage);
            }
        }

        private void ImportAreaOfEffects()
        {
            var aoe2da = Load2da("vfx_persistent");

            Standard.AreaEffects.Clear();
            for (int i = 0; i < aoe2da.Count; i++)
            {
                var tmpAreaEffect = new AreaEffect();
                tmpAreaEffect.ID = GenerateGuid("vfx_persistent", i);
                tmpAreaEffect.Index = i;

                tmpAreaEffect.Name = aoe2da[i].AsString("LABEL") ?? "";
                if (aoe2da[i].IsNull("SHAPE")) continue;
                tmpAreaEffect.Shape = Enum.Parse<AreaEffectShape>(aoe2da[i].AsString("SHAPE") ?? "C", true);
                tmpAreaEffect.Radius = aoe2da[i].AsFloat("RADIUS");
                tmpAreaEffect.Width = aoe2da[i].AsFloat("WIDTH");
                tmpAreaEffect.Length = aoe2da[i].AsFloat("LENGTH");
                tmpAreaEffect.OnEnterScript = aoe2da[i].AsString("ONENTER");
                tmpAreaEffect.OnExitScript = aoe2da[i].AsString("ONEXIT");
                tmpAreaEffect.OnHeartbeatScript = aoe2da[i].AsString("HEARTBEAT");
                tmpAreaEffect.OrientWithGround = aoe2da[i].AsBoolean("OrientWithGround");
                tmpAreaEffect.VisualEffect = IntPtr.Zero; // !
                tmpAreaEffect.Model1 = aoe2da[i].AsString("MODEL01");
                tmpAreaEffect.Model2 = aoe2da[i].AsString("MODEL02");
                tmpAreaEffect.Model3 = aoe2da[i].AsString("MODEL03");
                tmpAreaEffect.Model1Amount = aoe2da[i].AsInteger("NUMACT01");
                tmpAreaEffect.Model2Amount = aoe2da[i].AsInteger("NUMACT02");
                tmpAreaEffect.Model3Amount = aoe2da[i].AsInteger("NUMACT03");
                tmpAreaEffect.Model1Duration = aoe2da[i].AsInteger("DURATION01");
                tmpAreaEffect.Model2Duration = aoe2da[i].AsInteger("DURATION02");
                tmpAreaEffect.Model3Duration = aoe2da[i].AsInteger("DURATION03");
                tmpAreaEffect.Model1EdgeWeight = aoe2da[i].AsFloat("EDGEWGHT01");
                tmpAreaEffect.Model2EdgeWeight = aoe2da[i].AsFloat("EDGEWGHT02");
                tmpAreaEffect.Model3EdgeWeight = aoe2da[i].AsFloat("EDGEWGHT03");
                tmpAreaEffect.ImpactSound = aoe2da[i].AsString("SoundImpact");
                tmpAreaEffect.LoopSound = aoe2da[i].AsString("SoundDuration");
                tmpAreaEffect.CessationSound = aoe2da[i].AsString("SoundCessation");
                tmpAreaEffect.RandomSound = aoe2da[i].AsString("SoundOneShot");
                tmpAreaEffect.RandomSoundChance = aoe2da[i].AsFloat("SoundOneShotPercentage");
                tmpAreaEffect.LowQualityModel1 = aoe2da[i].AsString("MODELMIN01");
                tmpAreaEffect.LowQualityModel2 = aoe2da[i].AsString("MODELMIN02");
                tmpAreaEffect.LowQualityModel3 = aoe2da[i].AsString("MODELMIN03");

                Standard.AreaEffects.Add(tmpAreaEffect);
            }
        }

        private void ImportSoundsets()
        {
            var soundset2da = Load2da("soundset");

            Standard.Soundsets.Clear();
            for (int i = 0; i < soundset2da.Count; i++)
            {
                var tmpSoundset = new Soundset();
                tmpSoundset.ID = GenerateGuid("soundset", i);
                tmpSoundset.Index = i;

                if (!SetText(tmpSoundset.Name, soundset2da[i].AsInteger("STRREF"))) continue;

                tmpSoundset.Gender = !soundset2da[i].IsNull("GENDER") ? (Gender)Enum.ToObject(typeof(Gender), soundset2da[i].AsInteger("GENDER") ?? 0) : Gender.Male;
                tmpSoundset.Type = !soundset2da[i].IsNull("TYPE") ? (SoundsetType)Enum.ToObject(typeof(SoundsetType), soundset2da[i].AsInteger("TYPE") ?? 0) : SoundsetType.Player;
                tmpSoundset.SoundsetResource = soundset2da[i].AsString("RESREF") ?? "";

                var ssfResource = bif.ReadResource(tmpSoundset.SoundsetResource, NWNResourceType.SSF);
                var ssf = new SsfFile(ssfResource.RawData);
                for (int j = 0; j < ssf.Data.Count; j++)
                {
                    var soundsetEntry = tmpSoundset.Entries.GetByType((SoundsetEntryType)j);
                    if (soundsetEntry != null)
                    {
                        SetText(soundsetEntry.Text, ssf.Data[j].StringRef >= 0x01000000 ? null : (int)ssf.Data[j].StringRef);
                        soundsetEntry.SoundFile = new string(ssf.Data[j].ResRef).Trim('\0');
                    }
                }

                Standard.Soundsets.Add(tmpSoundset);
            }
        }

        private void ImportPolymorphs()
        {
            var polymorph2da = Load2da("polymorph");

            Standard.Polymorphs.Clear();
            for (int i = 0; i < polymorph2da.Count; i++)
            {
                var tmpPolymorph = new Polymorph();
                tmpPolymorph.ID = GenerateGuid("polymorph", i);
                tmpPolymorph.Index = i;

                if (polymorph2da[i].IsNull("Name")) continue;
                tmpPolymorph.Name = polymorph2da[i].AsString("Name") ?? "";
                tmpPolymorph.Appearance = CreateRef<Appearance>(polymorph2da[i].AsInteger("AppearanceType"));
                tmpPolymorph.RacialType = CreateRef<Race>(polymorph2da[i].AsInteger("RacialType"));
                //tmpPolymorph.Portrait = CreateRef<Portrait>(polymorph2da[i].AsInteger("PortraitId"));
                tmpPolymorph.PortraitResRef = polymorph2da[i].AsString("Portrait") ?? "";
                tmpPolymorph.CreatureWeapon1 = polymorph2da[i].AsString("CreatureWeapon1");
                tmpPolymorph.CreatureWeapon2 = polymorph2da[i].AsString("CreatureWeapon2");
                tmpPolymorph.CreatureWeapon3 = polymorph2da[i].AsString("CreatureWeapon3");
                tmpPolymorph.HideItem = polymorph2da[i].AsString("HideItem");
                tmpPolymorph.MainHandItem = polymorph2da[i].AsString("EQUIPPED");
                tmpPolymorph.Strength = polymorph2da[i].AsInteger("STR");
                tmpPolymorph.Constitution = polymorph2da[i].AsInteger("CON");
                tmpPolymorph.Dexterity = polymorph2da[i].AsInteger("DEX");
                tmpPolymorph.NaturalACBonus = polymorph2da[i].AsInteger("NATURALACBONUS");
                tmpPolymorph.HPBonus = polymorph2da[i].AsInteger("HPBONUS");
                //tmpPolymorph.Soundset = CreateRef<Soundset>(polymorph2da[i].AsInteger("SoundSet"));
                tmpPolymorph.Spell1 = CreateRef<Spell>(polymorph2da[i].AsInteger("SPELL1"));
                tmpPolymorph.Spell2 = CreateRef<Spell>(polymorph2da[i].AsInteger("SPELL2"));
                tmpPolymorph.Spell3 = CreateRef<Spell>(polymorph2da[i].AsInteger("SPELL3"));
                tmpPolymorph.MergeWeapon = polymorph2da[i].AsBoolean("MergeW");
                tmpPolymorph.MergeAccessories = polymorph2da[i].AsBoolean("MergeI");
                tmpPolymorph.MergeArmor = polymorph2da[i].AsBoolean("MergeA");

                Standard.Polymorphs.Add(tmpPolymorph);
            }
        }

        private void ImportAppearances()
        {
            var appearance2da = Load2da("appearance");

            Standard.Appearances.Clear();
            for (int i = 0; i < appearance2da.Count; i++)
            {
                var tmpAppearance = new Appearance();
                tmpAppearance.ID = GenerateGuid("appearance", i);
                tmpAppearance.Index = i;

                if (!SetText(tmpAppearance.Name, appearance2da[i].AsInteger("STRING_REF"))) continue;

                Standard.Appearances.Add(tmpAppearance);
            }
        }

        private T? SolveInstance<T>(T? instance, ModelRepository<T> repository) where T : BaseModel, new()
        {
            if (instance?.Index == null)
                return null;
            else
                return repository.GetByIndex(instance.Index ?? -1);
        }

        private void ResolveDependencies()
        {
            // Classes
            foreach (var cls in Standard.Classes)
            {
                if (cls == null) continue;
                cls.DefaultPackage = SolveInstance(cls.DefaultPackage, Standard.ClassPackages);
            }

            // Races
            foreach (var race in Standard.Races)
            {
                if (race == null) continue;
                race.Appearance = SolveInstance(race.Appearance, Standard.Appearances);
                race.FavoredClass = SolveInstance(race.FavoredClass, Standard.Classes);
                race.ToolsetDefaultClass = SolveInstance(race.ToolsetDefaultClass, Standard.Classes);
                race.FavoredEnemyFeat = SolveInstance(race.FavoredEnemyFeat, Standard.Feats);
            }

            // Domains
            foreach (var domain in Standard.Domains)
            {
                if (domain == null) continue;
                domain.Level0Spell = SolveInstance(domain.Level0Spell, Standard.Spells);
                domain.Level1Spell = SolveInstance(domain.Level1Spell, Standard.Spells);
                domain.Level2Spell = SolveInstance(domain.Level2Spell, Standard.Spells);
                domain.Level3Spell = SolveInstance(domain.Level3Spell, Standard.Spells);
                domain.Level4Spell = SolveInstance(domain.Level4Spell, Standard.Spells);
                domain.Level5Spell = SolveInstance(domain.Level5Spell, Standard.Spells);
                domain.Level6Spell = SolveInstance(domain.Level6Spell, Standard.Spells);
                domain.Level7Spell = SolveInstance(domain.Level7Spell, Standard.Spells);
                domain.Level8Spell = SolveInstance(domain.Level8Spell, Standard.Spells);
                domain.Level9Spell = SolveInstance(domain.Level9Spell, Standard.Spells);
                domain.GrantedFeat = SolveInstance(domain.GrantedFeat, Standard.Feats);
            }

            // Spells
            foreach (var spell in Standard.Spells)
            {
                if (spell == null) continue;
                spell.CounterSpell1 = SolveInstance(spell.CounterSpell1, Standard.Spells);
                spell.CounterSpell2 = SolveInstance(spell.CounterSpell2, Standard.Spells);
                spell.ParentSpell = SolveInstance(spell.ParentSpell, Standard.Spells);
                spell.SubSpell1 = SolveInstance(spell.SubSpell1, Standard.Spells);
                spell.SubSpell2 = SolveInstance(spell.SubSpell2, Standard.Spells);
                spell.SubSpell3 = SolveInstance(spell.SubSpell3, Standard.Spells);
                spell.SubSpell4 = SolveInstance(spell.SubSpell4, Standard.Spells);
                spell.SubSpell5 = SolveInstance(spell.SubSpell5, Standard.Spells);
                spell.SubSpell6 = SolveInstance(spell.SubSpell6, Standard.Spells);
                spell.SubSpell7 = SolveInstance(spell.SubSpell7, Standard.Spells);
                spell.SubSpell8 = SolveInstance(spell.SubSpell8, Standard.Spells);
            }

            // Feats
            foreach (var feat in Standard.Feats)
            {
                if (feat == null) continue;
                feat.OnUseEffect = SolveInstance(feat.OnUseEffect, Standard.Spells);
                feat.MasterFeat = SolveInstance(feat.MasterFeat, Standard.Feats);
                feat.RequiredFeat1 = SolveInstance(feat.RequiredFeat1, Standard.Feats);
                feat.RequiredFeat2 = SolveInstance(feat.RequiredFeat2, Standard.Feats);
                feat.RequiredFeatSelection1 = SolveInstance(feat.RequiredFeatSelection1, Standard.Feats);
                feat.RequiredFeatSelection2 = SolveInstance(feat.RequiredFeatSelection2, Standard.Feats);
                feat.RequiredFeatSelection3 = SolveInstance(feat.RequiredFeatSelection3, Standard.Feats);
                feat.RequiredFeatSelection4 = SolveInstance(feat.RequiredFeatSelection4, Standard.Feats);
                feat.RequiredFeatSelection5 = SolveInstance(feat.RequiredFeatSelection5, Standard.Feats);
                feat.RequiredSkill1 = SolveInstance(feat.RequiredSkill1, Standard.Skills);
                feat.RequiredSkill2 = SolveInstance(feat.RequiredSkill2, Standard.Skills);
                feat.SuccessorFeat = SolveInstance(feat.SuccessorFeat, Standard.Feats);
                feat.MinLevelClass = SolveInstance(feat.MinLevelClass, Standard.Classes);
            }

            // Polymorphs
            foreach (var polymorph in Standard.Polymorphs)
            {
                if (polymorph == null) continue;
                polymorph.Appearance = SolveInstance(polymorph.Appearance, Standard.Appearances);
                polymorph.RacialType = SolveInstance(polymorph.RacialType, Standard.Races);
                //polymorph.Portrait = SolveInstance(polymorph.Portrait, Standard.Portraits);
                //polymorph.Soundset = SolveInstance(polymorph.Soundset, Standard.Soundsets);
                polymorph.Spell1 = SolveInstance(polymorph.Spell1, Standard.Spells);
                polymorph.Spell2 = SolveInstance(polymorph.Spell2, Standard.Spells);
                polymorph.Spell3 = SolveInstance(polymorph.Spell3, Standard.Spells);
            }

            // FeatsTable
            foreach (var featTable in Standard.FeatTables)
            {
                if (featTable == null) continue;

                featTable.Items.Sort(p => p?.GrantedOnLevel == -1 ? int.MaxValue : p?.GrantedOnLevel);
                for (int i = 0; i < featTable.Count; i++)
                {
                    var item = featTable[i];
                    if (item == null) continue;
                    item.Feat = SolveInstance(item.Feat, Standard.Feats);
                }
            }

            // SkillsTable
            foreach (var skillTable in Standard.SkillTables)
            {
                if (skillTable == null) continue;
                for (int i = 0; i < skillTable.Count; i++)
                {
                    var item = skillTable[i];
                    if (item == null) continue;
                    item.Skill = SolveInstance(item.Skill, Standard.Skills);
                }
            }

            // Prerequisites Table
            foreach (var preRequTable in Standard.PrerequisiteTables)
            {
                if (preRequTable == null) continue;
                for (int i = 0; i < preRequTable.Count; i++)
                {
                    var item = preRequTable[i];
                    if (item == null) continue;

                    switch (item.RequirementType)
                    {
                        case RequirementType.CLASSNOT:
                        case RequirementType.CLASSOR:
                            item.RequirementParam1 = SolveInstance((CharacterClass?)item.RequirementParam1, Standard.Classes);
                            break;

                        case RequirementType.FEAT:
                        case RequirementType.FEATOR:
                            item.RequirementParam1 = SolveInstance((Feat?)item.RequirementParam1, Standard.Feats);
                            break;

                        case RequirementType.RACE:
                            item.RequirementParam1 = SolveInstance((Race?)item.RequirementParam1, Standard.Races);
                            break;

                        case RequirementType.SKILL:
                            item.RequirementParam1 = SolveInstance((Skill?)item.RequirementParam1, Standard.Skills);
                            break;
                    }
                }
            }

            // Racial Feats Table
            foreach (var racialFeatsTable in Standard.RacialFeatsTables)
            {
                if (racialFeatsTable == null) continue;
                for (int i = 0; i < racialFeatsTable.Count; i++)
                {
                    var item = racialFeatsTable[i];
                    if (item == null) continue;
                    item.Feat = SolveInstance(item.Feat, Standard.Feats);
                }
            }

            // Class Packages
            foreach (var package in Standard.ClassPackages)
            {
                if (package == null) continue;
                package.ForClass = SolveInstance(package.ForClass, Standard.Classes);
                package.Domain1 = SolveInstance(package.Domain1, Standard.Domains);
                package.Domain2 = SolveInstance(package.Domain2, Standard.Domains);
            }
        }

        private void SaveToJson()
        {
            if (!Directory.Exists(Constants.BaseDataPath))
                Directory.CreateDirectory(Constants.BaseDataPath);

            Standard.Races.SaveToFile(Constants.RacesFilePath);
            Standard.Classes.SaveToFile(Constants.ClassesFilePath);
            Standard.Domains.SaveToFile(Constants.DomainsFilePath);
            Standard.Skills.SaveToFile(Constants.SkillsFilePath);
            Standard.Feats.SaveToFile(Constants.FeatsFilePath);
            Standard.Spells.SaveToFile(Constants.SpellsFilePath);
            Standard.Diseases.SaveToFile(Constants.DiseasesFilePath);
            Standard.Poisons.SaveToFile(Constants.PoisonsFilePath);
            Standard.Spellbooks.SaveToFile(Constants.SpellbooksFilePath);
            Standard.AreaEffects.SaveToFile(Constants.AreaEffectsFilePath);

            Standard.Appearances.SaveToFile(Constants.AppearancesFilePath);
            Standard.ClassPackages.SaveToFile(Constants.ClassPackagesFilePath);
            Standard.Soundsets.SaveToFile(Constants.SoundsetsFilePath);
            Standard.Polymorphs.SaveToFile(Constants.PolymorphsFilePath);

            Standard.AttackBonusTables.SaveToFile(Constants.AttackBonusTablesFilePath);
            Standard.BonusFeatTables.SaveToFile(Constants.BonusFeatTablesFilePath);
            Standard.FeatTables.SaveToFile(Constants.FeatTablesFilePath);
            Standard.SavingThrowTables.SaveToFile(Constants.SavingThrowTablesFilePath);
            Standard.PrerequisiteTables.SaveToFile(Constants.PrerequisiteTablesFilePath);
            Standard.SkillTables.SaveToFile(Constants.SkillTablesFilePath);
            Standard.SpellSlotTables.SaveToFile(Constants.SpellSlotTablesFilePath);
            Standard.KnownSpellsTables.SaveToFile(Constants.KnownSpellsTablesFilePath);
            Standard.StatGainTables.SaveToFile(Constants.StatGainTablesFilePath);
            Standard.RacialFeatsTables.SaveToFile(Constants.RacialFeatsTablesFilePath);
        }

        private void ImportIcons()
        {
            if (!Directory.Exists(Constants.IconResourcesFilePath))
                Directory.CreateDirectory(Constants.IconResourcesFilePath);

            foreach (var iconResRef in iconResourceBuffer)
            {
                var iconTGAData = Resources.GetRaw(iconResRef, NWNResourceType.TGA);
                if (iconTGAData != null)
                {
                    var fs = new FileStream(Constants.IconResourcesFilePath + iconResRef + ".tga", FileMode.Create, FileAccess.ReadWrite);
                    try
                    {
                        iconTGAData.CopyTo(fs);
                    }
                    finally
                    {
                        fs.Close();
                    }
                }
            }
        }

        private void ClearTables()
        {
            Standard.Spellbooks.Clear();

            Standard.AttackBonusTables.Clear();
            Standard.BonusFeatTables.Clear();
            Standard.FeatTables.Clear();
            Standard.SavingThrowTables.Clear();
            Standard.PrerequisiteTables.Clear();
            Standard.SkillTables.Clear();
            Standard.SpellSlotTables.Clear();
            Standard.KnownSpellsTables.Clear();
            Standard.StatGainTables.Clear();
            Standard.RacialFeatsTables.Clear();
        }

        public void Import(string nwnBasePath)
        {
            this.nwnBasePath = nwnBasePath;
            tlk.Load(nwnBasePath);
            bif.Load(nwnBasePath);

            ClearTables();

            ImportRaces();
            ImportClasses();
            ImportDomains();
            ImportSkills();
            ImportFeats();
            ImportSpells();
            ImportDiseases();
            ImportPoisons();
            ImportAreaOfEffects();

            ImportAppearances();
            ImportClassPackages();
            ImportSoundsets();
            ImportPolymorphs();

            ImportText();

            ResolveDependencies();

            Standard.Domains.Sort(d => d?.Name[TLKLanguage.English].Text);
            Standard.Skills.Sort(s => s?.Name[TLKLanguage.English].Text);
            Standard.Feats.Sort(f => f?.Name[TLKLanguage.English].Text);
            Standard.Spells.Sort(s => s?.Name[TLKLanguage.English].Text);
            Standard.Diseases.Sort(d => d?.Name[TLKLanguage.English].Text);
            Standard.Poisons.Sort(p => p?.Name[TLKLanguage.English].Text);
            Standard.Spellbooks.Sort(p => p?.Name);

            Standard.AreaEffects.Sort(p => p?.Name);
            Standard.Polymorphs.Sort(p => p?.Name);
            Standard.Appearances.Sort(p => p?.Name[TLKLanguage.English].Text);
            Standard.ClassPackages.Sort(p => p?.Name[TLKLanguage.English].Text);
            Standard.Soundsets.Sort(p => p?.Name[TLKLanguage.English].Text);

            SaveToJson();
            ImportIcons();
        }
    }
}
