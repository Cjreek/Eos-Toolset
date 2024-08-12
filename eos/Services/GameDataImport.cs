using Eos.Config;
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
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Eos.Services
{
    public class GameDataImport
    {
        private string nwnBasePath = "";
        private TlkCollection tlk = new TlkCollection();
        private BifCollection bif = new BifCollection();
        private BifCollection ovrBif = new BifCollection();
        private RepositoryCollection Standard = MasterRepository.Standard;
        private ResourceRepository Resources = MasterRepository.Resources;

        private List<KeyValuePair<TLKStringSet, int?>> tlkBuffer = new List<KeyValuePair<TLKStringSet, int?>>();
        private HashSet<String> iconResourceBuffer = new HashSet<String>();

        private TwoDimensionalArrayFile Load2da(String name)
        {
            try
            {
                var filename = Path.Combine(nwnBasePath, "ovr", name + ".2da");
                if (ovrBif.ContainsResource(name, NWNResourceType.TWODA))
                {
                    Log.Info("Loading 2DA: {0}.2da", name);
                    var resource = ovrBif.ReadResource(name, NWNResourceType.TWODA);
                    return new TwoDimensionalArrayFile(resource.RawData);
                }
                else if (File.Exists(filename))
                {
                    Log.Info("Loading 2DA: {0}", filename);
                    using (var fs = File.OpenRead(filename))
                    {
                        return new TwoDimensionalArrayFile(fs);
                    }
                }
                else
                {
                    Log.Info("Loading 2DA: {0}.2da", name);
                    var resource = bif.ReadResource(name, NWNResourceType.TWODA);
                    return new TwoDimensionalArrayFile(resource.RawData);
                }
            }
            catch(Exception e)
            {
                Log.Error(e.Message);
                throw;
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
            var result = (strRef != null) && (strRef != 0);
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
                tmpItem.SourceLabel = racialFeatTable2da[i].AsString("FeatLabel") ?? "";
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
                tmpRace.SourceLabel = races2da[i].AsString("Label");

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
                tmpItem.SourceLabel = featTable2da[i].AsString("FeatLabel") ?? "";
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
                tmpItem.SourceLabel = skillTable2da[i].AsString("SkillLabel") ?? "";
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
                tmpItem.SourceLabel = preRequTable2da[i].AsString("LABEL") ?? "";
                tmpItem.RequirementType = Enum.Parse<RequirementType>(preRequTable2da[i].AsString("ReqType") ?? "", true);

                switch (tmpItem.RequirementType)
                {
                    case RequirementType.CLASSOR:
                    case RequirementType.CLASSNOT:
                        tmpItem.Param1Class = CreateRef<CharacterClass>(preRequTable2da[i].AsInteger("ReqParam1"));
                        break;

                    case RequirementType.FEAT:
                    case RequirementType.FEATOR:
                        tmpItem.Param1Feat = CreateRef<Feat>(preRequTable2da[i].AsInteger("ReqParam1"));
                        break;

                    case RequirementType.RACE:
                        tmpItem.Param1Race = CreateRef<Race>(preRequTable2da[i].AsInteger("ReqParam1"));
                        break;

                    case RequirementType.SKILL:
#if SPACEPOPE
                    case RequirementType.SKILLOR:
#endif
                        tmpItem.Param1Skill = CreateRef<Skill>(preRequTable2da[i].AsInteger("ReqParam1"));
                        break;

                    case RequirementType.VAR:
                        tmpItem.Param1String = preRequTable2da[i].AsString("ReqParam1");
                        break;

                    case RequirementType.ARCSPELL:
                    case RequirementType.SPELL:
                    case RequirementType.BAB:
#if SPACEPOPE
                    case RequirementType.ARCCAST:
                    case RequirementType.DIVCAST:
                    case RequirementType.DIVSPELL:
                    case RequirementType.PANTHEONOR:
                    case RequirementType.DEITYOR:
#endif
                        tmpItem.Param1Int = preRequTable2da[i].AsInteger("ReqParam1");
                        break;

                    case RequirementType.SAVE:
                        tmpItem.Param1Save = preRequTable2da[i].AsInteger("ReqParam1");
                        break;
                }

                tmpItem.RequirementParam2 = preRequTable2da[i].AsInteger("ReqParam2");
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
                    tmpClass.Abbreviation.OriginalIndex = -1;
                    tmpClass.Abbreviation[TLKLanguage.English].Text = "Gru";
                    tmpClass.Abbreviation[TLKLanguage.English].TextF = tmpClass.Abbreviation[TLKLanguage.English].Text;
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
                    tmpClass.Abbreviation.OriginalIndex = -1;
                    tmpClass.Abbreviation[TLKLanguage.English].Text = "Sho";
                    tmpClass.Abbreviation[TLKLanguage.English].TextF = tmpClass.Abbreviation[TLKLanguage.English].Text;
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
                tmpClass.SourceLabel = classes2da[i].AsString("Label");

                if (!SetText(tmpClass.Name, classes2da[i].AsInteger("Name")))
                {
                    ImportCutClass(classes2da, tmpClass, i);
                    continue;
                }
                SetText(tmpClass.NamePlural, classes2da[i].AsInteger("Plural"));
                SetText(tmpClass.Abbreviation, classes2da[i].AsInteger("Short", null));
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
                tmpDomain.SourceLabel = domains2da[i].AsString("Label");

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
                tmpSkill.SourceLabel = skills2da[i].AsString("Label");

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

                case 981: // Epic Harper Scout
                    tmpFeat.Name.OriginalIndex = -1;
                    tmpFeat.Name[TLKLanguage.English].Text = "Epic Harper Scout";
                    tmpFeat.Name[TLKLanguage.English].TextF = tmpFeat.Name[TLKLanguage.English].Text;
                    tmpFeat.Hint = "Cut";
                    tmpFeat.Icon = "IR_X1_HARPER";
                    tmpFeat.Category = null;
                    tmpFeat.UseableByAllClasses = false;
                    tmpFeat.ToolsetCategory = FeatCategory.Other;
                    tmpFeat.IsHostile = false;
                    tmpFeat.UseActionQueue = false;
                    tmpFeat.CRModifier = 0;
                    tmpFeat.UsesPerDay = null;
                    tmpFeat.RequiresEpic = true;

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

        private void ImportMasterFeats()
        {
            var masterFeats2da = Load2da("masterfeats");

            Standard.MasterFeats.Clear();
            for (int i = 0; i < masterFeats2da.Count; i++)
            {
                var tmpMasterFeat = new MasterFeat();
                tmpMasterFeat.ID = GenerateGuid("masterfeats", i);
                tmpMasterFeat.Index = i;
                tmpMasterFeat.SourceLabel = masterFeats2da[i].AsString("LABEL");

                if (!SetText(tmpMasterFeat.Name, masterFeats2da[i].AsInteger("STRREF"))) continue;
                SetText(tmpMasterFeat.Description, masterFeats2da[i].AsInteger("DESCRIPTION"));
                tmpMasterFeat.Icon = AddIconResource(masterFeats2da[i].AsString("ICON"));

                Standard.MasterFeats.Add(tmpMasterFeat);
            }
        }

        private void ImportBaseItems()
        {
            var baseitems2da = Load2da("baseitems");

            Standard.BaseItems.Clear();
            for (int i = 0; i < baseitems2da.Count; i++)
            {
                var tmpBaseItem = new BaseItem();
                tmpBaseItem.ID = GenerateGuid("baseitems", i);
                tmpBaseItem.Index = i;
                tmpBaseItem.SourceLabel = baseitems2da[i].AsString("label");

                if (!SetText(tmpBaseItem.Name, baseitems2da[i].AsInteger("Name"))) continue;
                SetText(tmpBaseItem.Description, baseitems2da[i].AsInteger("Description"));
                SetText(tmpBaseItem.StatsText, baseitems2da[i].AsInteger("BaseItemStatRef"));
                tmpBaseItem.Icon = AddIconResource(baseitems2da[i].AsString("DefaultIcon"));
                tmpBaseItem.InventorySlotWidth = baseitems2da[i].AsInteger("InvSlotWidth") ?? 1;
                tmpBaseItem.InventorySlotHeight = baseitems2da[i].AsInteger("InvSlotHeight") ?? 1;
                tmpBaseItem.EquipableSlots = (InventorySlots)(baseitems2da[i].AsInteger("EquipableSlots") ?? 0);
                tmpBaseItem.CanRotateIcon = baseitems2da[i].AsBoolean("CanRotateIcon");
                tmpBaseItem.ModelType = (ItemModelType)Enum.ToObject(typeof(ItemModelType), baseitems2da[i].AsInteger("ModelType") ?? 0);
                tmpBaseItem.ItemModel = baseitems2da[i].AsString("ItemClass") ?? "";
                tmpBaseItem.GenderSpecific = baseitems2da[i].AsBoolean("GenderSpecific");
                tmpBaseItem.Part1Alpha = baseitems2da[i].IsNull("Part1EnvMap") ? null : (AlphaChannelUsageType)Enum.ToObject(typeof(AlphaChannelUsageType), baseitems2da[i].AsInteger("Part1EnvMap") ?? 0);
                tmpBaseItem.Part2Alpha = baseitems2da[i].IsNull("Part2EnvMap") ? null : (AlphaChannelUsageType)Enum.ToObject(typeof(AlphaChannelUsageType), baseitems2da[i].AsInteger("Part2EnvMap") ?? 0);
                tmpBaseItem.Part3Alpha = baseitems2da[i].IsNull("Part3EnvMap") ? null : (AlphaChannelUsageType)Enum.ToObject(typeof(AlphaChannelUsageType), baseitems2da[i].AsInteger("Part3EnvMap") ?? 0);
                tmpBaseItem.DefaultModel = baseitems2da[i].AsString("DefaultModel") ?? "";
                tmpBaseItem.IsContainer = baseitems2da[i].AsBoolean("Container");
                tmpBaseItem.WeaponWieldType = (WeaponWieldType)Enum.ToObject(typeof(WeaponWieldType), baseitems2da[i].AsInteger("WeaponWield") ?? 0);
                tmpBaseItem.WeaponDamageType = (WeaponDamageType)Enum.ToObject(typeof(WeaponDamageType), baseitems2da[i].AsInteger("WeaponType") ?? 0);
                tmpBaseItem.WeaponSize = baseitems2da[i].IsNull("WeaponSize") ? null : (WeaponSize)Enum.ToObject(typeof(WeaponSize), baseitems2da[i].AsInteger("WeaponSize") ?? 0);
                tmpBaseItem.AmmunitionBaseItem = CreateRef<BaseItem>(baseitems2da[i].AsInteger("RangedWeapon"));
                tmpBaseItem.PreferredAttackDistance = baseitems2da[i].AsFloat("PrefAttackDist");
                tmpBaseItem.MinimumModelCount = baseitems2da[i].AsInteger("MinRange") ?? 10;
                tmpBaseItem.MaximumModelCount = baseitems2da[i].AsInteger("MaxRange") ?? 100;
                tmpBaseItem.DamageDiceCount = baseitems2da[i].AsInteger("NumDice");
                tmpBaseItem.DamageDice = baseitems2da[i].AsInteger("DieToRoll");
                tmpBaseItem.CriticalThreatRange = baseitems2da[i].AsInteger("CritThreat");
                tmpBaseItem.CriticalMultiplier = baseitems2da[i].AsInteger("CritHitMult");
                tmpBaseItem.Category = (ItemCategory)Enum.ToObject(typeof(ItemCategory), baseitems2da[i].AsInteger("Category") ?? 0);
                tmpBaseItem.BaseCost = baseitems2da[i].AsFloat("BaseCost") ?? 0.0;
                tmpBaseItem.MaxStackSize = baseitems2da[i].AsInteger("Stacking") ?? 1;
                tmpBaseItem.ItemCostMultiplier = baseitems2da[i].AsFloat("ItemMultiplier") ?? 1.0;
                tmpBaseItem.InventorySound = CreateRef<InventorySound>(baseitems2da[i].AsInteger("InvSoundType"));
                tmpBaseItem.MaxSpellProperties = baseitems2da[i].AsInteger("MaxProps") ?? 8;
                tmpBaseItem.MinSpellProperties = baseitems2da[i].AsInteger("MinProps") ?? 0;
                tmpBaseItem.ItemPropertySet = CreateRef<ItemPropertySet>(baseitems2da[i].AsInteger("PropColumn"));
                tmpBaseItem.StorePanel = baseitems2da[i].IsNull("StorePanel") ? null : (StorePanelType)Enum.ToObject(typeof(StorePanelType), baseitems2da[i].AsInteger("StorePanel") ?? 0);
                tmpBaseItem.RequiredFeat1 = CreateRef<Feat>(baseitems2da[i].AsInteger("ReqFeat0"));
                tmpBaseItem.RequiredFeat2 = CreateRef<Feat>(baseitems2da[i].AsInteger("ReqFeat1"));
                tmpBaseItem.RequiredFeat3 = CreateRef<Feat>(baseitems2da[i].AsInteger("ReqFeat2"));
                tmpBaseItem.RequiredFeat4 = CreateRef<Feat>(baseitems2da[i].AsInteger("ReqFeat3"));
                tmpBaseItem.RequiredFeat5 = CreateRef<Feat>(baseitems2da[i].AsInteger("ReqFeat4"));
                tmpBaseItem.ArmorClassType = baseitems2da[i].IsNull("AC_Enchant") ? null : (ArmorClassType)Enum.ToObject(typeof(ArmorClassType), baseitems2da[i].AsInteger("AC_Enchant") ?? 0);
                tmpBaseItem.BaseShieldAC = baseitems2da[i].AsInteger("BaseAC") ?? 0;
                tmpBaseItem.ArmorCheckPenalty = baseitems2da[i].AsInteger("ArmorCheckPen") ?? 0;
                tmpBaseItem.DefaultChargeCount = baseitems2da[i].AsInteger("ChargesStarting") ?? 0;
                tmpBaseItem.GroundModelRotation = (ItemModelRotation)Enum.ToObject(typeof(ItemModelRotation), baseitems2da[i].AsInteger("RotateOnGround") ?? 0);
                tmpBaseItem.Weight = (baseitems2da[i].AsInteger("TenthLBS") ?? 0) / 10.0;
                tmpBaseItem.WeaponSound = CreateRef<WeaponSound>(baseitems2da[i].AsInteger("WeaponMatType"));
                tmpBaseItem.AmmunitionType = baseitems2da[i].IsNull("AmmunitionType") ? null : (AmmunitionType)Enum.ToObject(typeof(AmmunitionType), baseitems2da[i].AsInteger("AmmunitionType") ?? 0);
                tmpBaseItem.QuickbarBehaviour = (QuickbarBehaviour)Enum.ToObject(typeof(QuickbarBehaviour), baseitems2da[i].AsInteger("QBBehaviour") ?? 0);
                tmpBaseItem.ArcaneSpellFailure = baseitems2da[i].AsInteger("ArcaneSpellFailure");
                tmpBaseItem.LeftSlashAnimationPercent = baseitems2da[i].AsInteger("%AnimSlashL");
                tmpBaseItem.RightSlashAnimationPercent = baseitems2da[i].AsInteger("%AnimSlashR");
                tmpBaseItem.StraightSlashAnimationPercent = baseitems2da[i].AsInteger("%AnimSlashS");
                tmpBaseItem.StorePanelOrder = baseitems2da[i].AsInteger("StorePanelSort");
                tmpBaseItem.ItemLevelRestrictionStackSize = baseitems2da[i].AsInteger("ILRStackSize") ?? 1;
                tmpBaseItem.WeaponFocusFeat = CreateRef<Feat>(baseitems2da[i].AsInteger("WeaponFocusFeat"));
                tmpBaseItem.EpicWeaponFocusFeat = CreateRef<Feat>(baseitems2da[i].AsInteger("EpicWeaponFocusFeat"));
                tmpBaseItem.WeaponSpecializationFeat = CreateRef<Feat>(baseitems2da[i].AsInteger("WeaponSpecializationFeat"));
                tmpBaseItem.EpicWeaponSpecializationFeat = CreateRef<Feat>(baseitems2da[i].AsInteger("EpicWeaponSpecializationFeat"));
                tmpBaseItem.ImprovedCriticalFeat = CreateRef<Feat>(baseitems2da[i].AsInteger("WeaponImprovedCriticalFeat"));
                tmpBaseItem.OverwhelmingCriticalFeat = CreateRef<Feat>(baseitems2da[i].AsInteger("EpicWeaponOverwhelmingCriticalFeat"));
                tmpBaseItem.DevastatingCriticalFeat = CreateRef<Feat>(baseitems2da[i].AsInteger("EpicWeaponDevastatingCriticalFeat"));
                tmpBaseItem.WeaponOfChoiceFeat = CreateRef<Feat>(baseitems2da[i].AsInteger("WeaponOfChoiceFeat"));
                tmpBaseItem.IsMonkWeapon = baseitems2da[i].AsBoolean("IsMonkWeapon");
                tmpBaseItem.WeaponFinesseMinimumCreatureSize = baseitems2da[i].IsNull("WeaponFinesseMinimumCreatureSize") ? null : (SizeCategory)Enum.ToObject(typeof(SizeCategory), baseitems2da[i].AsInteger("WeaponFinesseMinimumCreatureSize") ?? 0);

                Standard.BaseItems.Add(tmpBaseItem);
            }
        }

        private void ImportFeatsPTColumns(ItemPropertyTable table)
        { 
            table.CustomColumn01.Label = "Cost";
            table.CustomColumn01.Column = "Cost";
            table.CustomColumn01.DataType = MasterRepository.GetDataTypeByType(typeof(double));

            table.CustomColumn02.Label = "Feat";
            table.CustomColumn02.Column = "FeatIndex";
            table.CustomColumn02.DataType = MasterRepository.GetDataTypeByType(typeof(Feat));
        }

        private void ImportFeatsPTValues(LineRecord record, ItemPropertyTableItem item)
        {
            item.CustomColumnValue01.Value = record.AsFloat("Cost");
            item.CustomColumnValue02.Value = CreateRef<Feat>(record.AsInteger("FeatIndex"));
        }

        private void ImportSpellsPTColumns(ItemPropertyTable table)
        {
            table.CustomColumn01.Label = "Caster Level";
            table.CustomColumn01.Column = "CasterLvl";
            table.CustomColumn01.DataType = MasterRepository.GetDataTypeByType(typeof(int));

            table.CustomColumn02.Label = "Innate Level";
            table.CustomColumn02.Column = "InnateLvl";
            table.CustomColumn02.DataType = MasterRepository.GetDataTypeByType(typeof(double));

            table.CustomColumn03.Label = "Cost";
            table.CustomColumn03.Column = "Cost";
            table.CustomColumn03.DataType = MasterRepository.GetDataTypeByType(typeof(int));

            table.CustomColumn04.Label = "Spell";
            table.CustomColumn04.Column = "SpellIndex";
            table.CustomColumn04.DataType = MasterRepository.GetDataTypeByType(typeof(Spell));

            table.CustomColumn05.Label = "Potion";
            table.CustomColumn05.Column = "PotionUse";
            table.CustomColumn05.DataType = MasterRepository.GetDataTypeByType(typeof(bool));

            table.CustomColumn06.Label = "Wand";
            table.CustomColumn06.Column = "WandUse";
            table.CustomColumn06.DataType = MasterRepository.GetDataTypeByType(typeof(bool));

            table.CustomColumn07.Label = "General";
            table.CustomColumn07.Column = "GeneralUse";
            table.CustomColumn07.DataType = MasterRepository.GetDataTypeByType(typeof(bool));

            table.CustomColumn08.Label = "Icon";
            table.CustomColumn08.Column = "Icon";
            table.CustomColumn08.DataType = MasterRepository.GetDataTypeById(new Guid("e4897c44-4117-45d4-b3fc-37b82fd88247")); // Resource Reference
        }

        private void ImportSpellsPTValues(LineRecord record, ItemPropertyTableItem item)
        {
            item.CustomColumnValue01.Value = record.AsInteger("CasterLvl");
            item.CustomColumnValue02.Value = record.AsFloat("InnateLvl");
            item.CustomColumnValue03.Value = record.AsInteger("Cost");
            item.CustomColumnValue04.Value = CreateRef<Spell>(record.AsInteger("SpellIndex"));
            item.CustomColumnValue05.Value = record.AsBoolean("PotionUse");
            item.CustomColumnValue06.Value = record.AsBoolean("WandUse");
            item.CustomColumnValue07.Value = record.AsBoolean("GeneralUse");
            item.CustomColumnValue08.Value = record.AsString("Icon");
        }

        private void ImportDamageTypesPTColumns(ItemPropertyTable table)
        {
            table.CustomColumn01.Label = "Cost";
            table.CustomColumn01.Column = "Cost";
            table.CustomColumn01.DataType = MasterRepository.GetDataTypeByType(typeof(double));

            table.CustomColumn02.Label = "VisualFX";
            table.CustomColumn02.Column = "VisualFX";
            table.CustomColumn02.DataType = MasterRepository.GetDataTypeByType(typeof(int)); // ! iprp_visualfx --> PAIN
        }

        private void ImportDamageTypesPTValues(LineRecord record, ItemPropertyTableItem item)
        {
            item.CustomColumnValue01.Value = record.AsFloat("Cost");
            item.CustomColumnValue02.Value = record.AsInteger("VisualFX"); // ! iprp_visualfx --> PAIN
        }

        private void ImportCostOnlyPTColumns(ItemPropertyTable table)
        {
            table.CustomColumn01.Label = "Cost";
            table.CustomColumn01.Column = "Cost";
            table.CustomColumn01.DataType = MasterRepository.GetDataTypeByType(typeof(double));
        }

        private void ImportCostOnlyPTValues(LineRecord record, ItemPropertyTableItem item)
        {
            item.CustomColumnValue01.Value = record.AsFloat("Cost");
        }

        private void ImportOnHitPTColumns(ItemPropertyTable table)
        {
            table.CustomColumn01.Label = "Cost";
            table.CustomColumn01.Column = "Cost";
            table.CustomColumn01.DataType = MasterRepository.GetDataTypeByType(typeof(double));

            table.CustomColumn02.Label = "Param Table";
            table.CustomColumn02.Column = "Param1ResRef";
            table.CustomColumn02.DataType = MasterRepository.GetDataTypeByType(typeof(ItemPropertyParam));
        }

        private void ImportOnHitPTValues(LineRecord record, ItemPropertyTableItem item)
        {
            item.CustomColumnValue01.Value = record.AsFloat("Cost");
            item.CustomColumnValue02.Value = CreateRef<ItemPropertyParam>(record.AsInteger("Param1ResRef"));
        }

        private void ImportSpellSchoolPTColumns(ItemPropertyTable table)
        {
            table.CustomColumn01.Label = "Cost";
            table.CustomColumn01.Column = "Cost";
            table.CustomColumn01.DataType = MasterRepository.GetDataTypeByType(typeof(double));

            table.CustomColumn02.Label = "Letter";
            table.CustomColumn02.Column = "Letter";
            table.CustomColumn02.DataType = MasterRepository.GetDataTypeByType(typeof(string));
        }

        private void ImportSpellSchoolPTValues(LineRecord record, ItemPropertyTableItem item)
        {
            item.CustomColumnValue01.Value = record.AsFloat("Cost");
            item.CustomColumnValue02.Value = record.AsString("Letter");
        }

        private void ImportAmmoTypePTColumns(ItemPropertyTable table)
        {
            table.CustomColumn01.Label = "AmmoType";
            table.CustomColumn01.Column = "AmmoType";
            table.CustomColumn01.DataType = MasterRepository.GetDataTypeByType(typeof(BaseItem));
        }

        private void ImportAmmoTypePTValues(LineRecord record, ItemPropertyTableItem item)
        {
            item.CustomColumnValue01.Value = CreateRef<BaseItem>(record.AsInteger("AmmoType"));
        }

        private void ImportMonsterHitPTColumns(ItemPropertyTable table)
        {
            table.CustomColumn01.Label = "Cost";
            table.CustomColumn01.Column = "Cost";
            table.CustomColumn01.DataType = MasterRepository.GetDataTypeByType(typeof(double));

            table.CustomColumn02.Label = "Param1 Table";
            table.CustomColumn02.Column = "Param1ResRef";
            table.CustomColumn02.DataType = MasterRepository.GetDataTypeByType(typeof(ItemPropertyParam));

            table.CustomColumn03.Label = "Param2 Table";
            table.CustomColumn03.Column = "Param2ResRef";
            table.CustomColumn03.DataType = MasterRepository.GetDataTypeByType(typeof(ItemPropertyParam));
        }

        private void ImportMonsterHitPTValues(LineRecord record, ItemPropertyTableItem item)
        {
            item.CustomColumnValue01.Value = record.AsFloat("Cost");
            item.CustomColumnValue02.Value = CreateRef<ItemPropertyParam>(record.AsInteger("Param1ResRef"));
            item.CustomColumnValue03.Value = CreateRef<ItemPropertyParam>(record.AsInteger("Param2ResRef"));
        }

        private void ImportOnHitSpellPTColumns(ItemPropertyTable table)
        {
            table.CustomColumn01.Label = "SpellIndex";
            table.CustomColumn01.Column = "SpellIndex";
            table.CustomColumn01.DataType = MasterRepository.GetDataTypeByType(typeof(Spell));

            table.CustomColumn02.Label = "Cost";
            table.CustomColumn02.Column = "Cost";
            table.CustomColumn02.DataType = MasterRepository.GetDataTypeByType(typeof(double));
        }

        private void ImportOnHitSpellPTValues(LineRecord record, ItemPropertyTableItem item)
        {
            item.CustomColumnValue01.Value = CreateRef<Spell>(record.AsInteger("SpellIndex"));
            item.CustomColumnValue02.Value = record.AsFloat("Cost");
        }

        private void ImportVisualFXPTColumns(ItemPropertyTable table)
        {
            table.CustomColumn01.Label = "Model Suffix";
            table.CustomColumn01.Column = "ModelSuffix";
            table.CustomColumn01.DataType = MasterRepository.GetDataTypeById(new Guid("e4897c44-4117-45d4-b3fc-37b82fd88247")); // Resource Reference
        }

        private void ImportVisualFXPTValues(LineRecord record, ItemPropertyTableItem item)
        {
            item.CustomColumnValue01.Value = record.AsString("ModelSuffix");
        }

        private void ImportOnHitDurPTColumns(ItemPropertyTable table)
        {
            table.CustomColumn01.Label = "Cost";
            table.CustomColumn01.Column = "Cost";
            table.CustomColumn01.DataType = MasterRepository.GetDataTypeByType(typeof(double));

            table.CustomColumn02.Label = "Effect Chance";
            table.CustomColumn02.Column = "EffectChance";
            table.CustomColumn02.DataType = MasterRepository.GetDataTypeByType(typeof(int));

            table.CustomColumn03.Label = "Duration (Rounds)";
            table.CustomColumn03.Column = "DurationRounds";
            table.CustomColumn03.DataType = MasterRepository.GetDataTypeByType(typeof(int));
        }

        private void ImportOnHitDurPTValues(LineRecord record, ItemPropertyTableItem item)
        {
            item.CustomColumnValue01.Value = record.AsFloat("Cost");
            item.CustomColumnValue02.Value = record.AsInteger("EffectChance");
            item.CustomColumnValue03.Value = record.AsInteger("DurationRounds");
        }

        private void ImportWeightIncPTColumns(ItemPropertyTable table)
        {
            table.CustomColumn01.Label = "Value";
            table.CustomColumn01.Column = "Value";
            table.CustomColumn01.DataType = MasterRepository.GetDataTypeByType(typeof(int));
        }

        private void ImportWeightIncPTValues(LineRecord record, ItemPropertyTableItem item)
        {
            item.CustomColumnValue01.Value = record.AsInteger("Value");
        }

        private void ImportItemPropertyTable(string tablename, Guid guid)
        {
            if (!tablename.StartsWith("iprp_", StringComparison.OrdinalIgnoreCase)) return;
            if (((ItemPropertyTableRepository)Standard.ItemPropertyTables).Contains(tablename)) return;

            var iprpTable2da = Load2da(tablename.ToLower());

            var iprpTable = new ItemPropertyTable();
            iprpTable.ID = guid;
            iprpTable.Name = tablename;

            switch (tablename.ToLower())
            {
                case "iprp_feats":
                    ImportFeatsPTColumns(iprpTable);
                    break;
                case "iprp_spells":
                    ImportSpellsPTColumns(iprpTable);
                    break;
                case "iprp_damagetype":
                    ImportDamageTypesPTColumns(iprpTable);
                    break;
                case "iprp_protection":
                case "iprp_immunity":
                case "iprp_saveelement":
                case "iprp_traps":
                    ImportCostOnlyPTColumns(iprpTable);
                    break;
                case "iprp_onhit":
                    ImportOnHitPTColumns(iprpTable);
                    break;
                case "iprp_spellshl":
                    ImportSpellSchoolPTColumns(iprpTable);
                    break;
                case "iprp_ammotype":
                    ImportAmmoTypePTColumns(iprpTable);
                    break;
                case "iprp_monsterhit":
                    ImportMonsterHitPTColumns(iprpTable);
                    break;
                case "iprp_onhitspell":
                    ImportOnHitSpellPTColumns(iprpTable);
                    break;
                case "iprp_visualfx":
                    ImportVisualFXPTColumns(iprpTable);
                    break;
                case "iprp_onhitdur":
                    ImportOnHitDurPTColumns(iprpTable);
                    break;
                case "iprp_weightinc":
                    ImportWeightIncPTColumns(iprpTable);
                    break;
            }

            iprpTable.Clear();
            for (int i = 0; i < iprpTable2da.Count; i++)
            {
                var tmpItem = new ItemPropertyTableItem(iprpTable);
                if (iprpTable2da.Columns.IndexOf("Label") != -1)
                    tmpItem.SourceLabel = iprpTable2da[i].AsString("Label") ?? "";

                SetText(tmpItem.Name, iprpTable2da[i].AsInteger("Name"));
                switch (tablename.ToLower())
                {
                    case "iprp_feats":
                        ImportFeatsPTValues(iprpTable2da[i], tmpItem);
                        break;
                    case "iprp_spells":
                        ImportSpellsPTValues(iprpTable2da[i], tmpItem);
                        break;
                    case "iprp_damagetype":
                        ImportDamageTypesPTValues(iprpTable2da[i], tmpItem);
                        break;
                    case "iprp_protection":
                    case "iprp_immunity":
                    case "iprp_saveelement":
                    case "iprp_traps":
                        ImportCostOnlyPTValues(iprpTable2da[i], tmpItem);
                        break;
                    case "iprp_onhit":
                        ImportOnHitPTValues(iprpTable2da[i], tmpItem);
                        break;
                    case "iprp_spellshl":
                        ImportSpellSchoolPTValues(iprpTable2da[i], tmpItem);
                        break;
                    case "iprp_ammotype":
                        ImportAmmoTypePTValues(iprpTable2da[i], tmpItem);
                        break;
                    case "iprp_monsterhit":
                        ImportMonsterHitPTValues(iprpTable2da[i], tmpItem);
                        break;
                    case "iprp_onhitspell":
                        ImportOnHitSpellPTValues(iprpTable2da[i], tmpItem);
                        break;
                    case "iprp_visualfx":
                        ImportVisualFXPTValues(iprpTable2da[i], tmpItem);
                        break;
                    case "iprp_onhitdur":
                        ImportOnHitDurPTValues(iprpTable2da[i], tmpItem);
                        break;
                    case "iprp_weightinc":
                        ImportWeightIncPTValues(iprpTable2da[i], tmpItem);
                        break;
                }

                iprpTable.Add(tmpItem);
            }

            Standard.ItemPropertyTables.Add(iprpTable);
        }

        private void ImportItemProperties()
        {
            var itemprops2da = Load2da("itemprops");

            Standard.ItemPropertySets.Clear();
            for (int i = 0; i < itemprops2da.Columns.Count; i++)
            {
                var match = Regex.Match(itemprops2da.Columns[i], "^([0-9]+)_(\\w+)$");
                if (match.Success)
                {
                    int number = int.Parse(match.Groups[1].Value);

                    var tmpItemPropertySet = new ItemPropertySet();
                    tmpItemPropertySet.ID = GenerateGuid("propertysets", number);
                    tmpItemPropertySet.Index = number;
                    tmpItemPropertySet.Name = match.Groups[2].Value;

                    for (int j = 0; j < itemprops2da.Count; j++)
                    {
                        if (itemprops2da[j].AsBoolean(i))
                        {
                            var item = new ItemPropertySetEntry(tmpItemPropertySet);
                            item.ItemProperty = CreateRef<ItemProperty>(j);

                            tmpItemPropertySet.ItemProperties.Add(item);
                        }
                    }

                    Standard.ItemPropertySets.Add(tmpItemPropertySet);
                }
            }

            var itempropdef2da = Load2da("itempropdef");
            Standard.ItemProperties.Clear();
            for (int i = 0; i < itempropdef2da.Count; i++)
            {
                var tmpItemProperty = new ItemProperty();
                tmpItemProperty.ID = GenerateGuid("itempropdef", i);
                tmpItemProperty.Index = i;
                tmpItemProperty.SourceLabel = itempropdef2da[i].AsString("Label");

                if (!SetText(tmpItemProperty.Name, itempropdef2da[i].AsInteger("Name"))) continue;
                SetText(tmpItemProperty.PropertyText, itempropdef2da[i].AsInteger("GameStrRef"));
                SetText(tmpItemProperty.Description, itempropdef2da[i].AsInteger("Description"));
                tmpItemProperty.Cost = itempropdef2da[i].AsFloat("Cost") ?? 1.0;

                // SubTypes
                tmpItemProperty.SubTypeResRef = itempropdef2da[i].AsString("SubTypeResRef") ?? "";
                if (tmpItemProperty.SubTypeResRef != null)
                {
                    var subTypeTableGuid = GenerateGuid(tmpItemProperty.SubTypeResRef.ToLower(), 0);
                    if (!Standard.ItemPropertyTables.Contains(subTypeTableGuid))
                        ImportItemPropertyTable(tmpItemProperty.SubTypeResRef, subTypeTableGuid);
                    tmpItemProperty.SubType = Standard.ItemPropertyTables.GetByID(subTypeTableGuid);
                }

                tmpItemProperty.CostTable = CreateRef<ItemPropertyCostTable>(itempropdef2da[i].AsInteger("CostTableResRef"));
                tmpItemProperty.Param = CreateRef<ItemPropertyParam>(itempropdef2da[i].AsInteger("Param1ResRef"));

                Standard.ItemProperties.Add(tmpItemProperty);
            }
        }

        private void ImportValueOnlyCTColumns(ItemPropertyCostTable table)
        {
            table.CustomColumn01.Label = "Value";
            table.CustomColumn01.Column = "Value";
            table.CustomColumn01.DataType = MasterRepository.GetDataTypeByType(typeof(int));
        }

        private void ImportValueOnlyCTValues(LineRecord record, ItemPropertyCostTableItem item)
        {
            item.CustomColumnValue01.Value = record.AsInteger("Value");
        }

        private void ImportMeleeCostCTColumns(ItemPropertyCostTable table)
        {
            table.CustomColumn01.Label = "Value";
            table.CustomColumn01.Column = "Value";
            table.CustomColumn01.DataType = MasterRepository.GetDataTypeByType(typeof(int));

            table.CustomColumn02.Label = "VFX";
            table.CustomColumn02.Column = "VFX";
            table.CustomColumn02.DataType = MasterRepository.GetDataTypeByType(typeof(bool));
        }

        private void ImportMeleeCostCTValues(LineRecord record, ItemPropertyCostTableItem item)
        {
            item.CustomColumnValue01.Value = record.AsInteger("Value");
            item.CustomColumnValue02.Value = record.AsBoolean("VFX");
        }

        private void ImportChargeCostCTColumns(ItemPropertyCostTable table)
        {
            table.CustomColumn01.Label = "Potion";
            table.CustomColumn01.Column = "PotionCost";
            table.CustomColumn01.DataType = MasterRepository.GetDataTypeByType(typeof(bool));

            table.CustomColumn02.Label = "Wand";
            table.CustomColumn02.Column = "WandCost";
            table.CustomColumn02.DataType = MasterRepository.GetDataTypeByType(typeof(bool));
        }

        private void ImportChargeCostCTValues(LineRecord record, ItemPropertyCostTableItem item)
        {
            item.CustomColumnValue01.Value = record.AsBoolean("PotionCost");
            item.CustomColumnValue02.Value = record.AsBoolean("WandCost");
        }

        private void ImportDamageCostCTColumns(ItemPropertyCostTable table)
        {
            table.CustomColumn01.Label = "#Dice";
            table.CustomColumn01.Column = "NumDice";
            table.CustomColumn01.DataType = MasterRepository.GetDataTypeByType(typeof(int));

            table.CustomColumn02.Label = "Die";
            table.CustomColumn02.Column = "Die";
            table.CustomColumn02.DataType = MasterRepository.GetDataTypeByType(typeof(int));

            table.CustomColumn03.Label = "Rank";
            table.CustomColumn03.Column = "Rank";
            table.CustomColumn03.DataType = MasterRepository.GetDataTypeByType(typeof(int));

            table.CustomColumn04.Label = "Ingame Text";
            table.CustomColumn04.Column = "GameString";
            table.CustomColumn04.DataType = MasterRepository.GetDataTypeByType(typeof(TLKStringSet));

            table.CustomColumn05.Label = "VFX";
            table.CustomColumn05.Column = "VFX";
            table.CustomColumn05.DataType = MasterRepository.GetDataTypeByType(typeof(bool));
        }

        private void ImportDamageCostCTValues(LineRecord record, ItemPropertyCostTableItem item)
        {
            
            item.CustomColumnValue01.Value = record.AsInteger("NumDice");
            item.CustomColumnValue02.Value = record.AsInteger("Die");
            item.CustomColumnValue03.Value = record.AsInteger("Rank");

            var gameStr = record.AsInteger("GameString");
            if (gameStr != null)
            {
                var tlkGameStr = new TLKStringSet();
                SetText(tlkGameStr, gameStr);
                item.CustomColumnValue04.Value = tlkGameStr;
            }
             
            item.CustomColumnValue05.Value = record.AsBoolean("VFX");
        }

        private void ImportAmountOnlyCTColumns(ItemPropertyCostTable table)
        {
            table.CustomColumn01.Label = "Amount";
            table.CustomColumn01.Column = "Amount";
            table.CustomColumn01.DataType = MasterRepository.GetDataTypeByType(typeof(int));
        }

        private void ImportAmountOnlyCTValues(LineRecord record, ItemPropertyCostTableItem item)
        {
            item.CustomColumnValue01.Value = record.AsInteger("Amount");
        }

        private void ImportFloatValueOnlyCTColumns(ItemPropertyCostTable table)
        {
            table.CustomColumn01.Label = "Value";
            table.CustomColumn01.Column = "Value";
            table.CustomColumn01.DataType = MasterRepository.GetDataTypeByType(typeof(double));
        }

        private void ImportFloatValueOnlyCTValues(LineRecord record, ItemPropertyCostTableItem item)
        {
            item.CustomColumnValue01.Value = record.AsFloat("Value");
        }

        private void ImportAmmoCostCTColumns(ItemPropertyCostTable table)
        {
            table.CustomColumn01.Label = "Arrow Blueprint";
            table.CustomColumn01.Column = "Arrow";
            table.CustomColumn01.DataType = MasterRepository.GetDataTypeById(new Guid("e4897c44-4117-45d4-b3fc-37b82fd88247")); // Resource Reference

            table.CustomColumn02.Label = "Bolt Blueprint";
            table.CustomColumn02.Column = "Bolt";
            table.CustomColumn02.DataType = MasterRepository.GetDataTypeById(new Guid("e4897c44-4117-45d4-b3fc-37b82fd88247")); // Resource Reference

            table.CustomColumn03.Label = "Bullet Blueprint";
            table.CustomColumn03.Column = "Bullet";
            table.CustomColumn03.DataType = MasterRepository.GetDataTypeById(new Guid("e4897c44-4117-45d4-b3fc-37b82fd88247")); // Resource Reference
        }

        private void ImportAmmoCostCTValues(LineRecord record, ItemPropertyCostTableItem item)
        {
            item.CustomColumnValue01.Value = record.AsString("Arrow");
            item.CustomColumnValue02.Value = record.AsString("Bolt");
            item.CustomColumnValue03.Value = record.AsString("Bullet");
        }

        private void ImportSpellCostCTColumns(ItemPropertyCostTable table)
        {
            table.CustomColumn01.Label = "Spell";
            table.CustomColumn01.Column = "SpellIndex";
            table.CustomColumn01.DataType = MasterRepository.GetDataTypeByType(typeof(Spell));
        }

        private void ImportSpellCostCTValues(LineRecord record, ItemPropertyCostTableItem item)
        {
            item.CustomColumnValue01.Value = CreateRef<Spell>(record.AsInteger("SpellIndex"));
        }

        private void ImportTrapCostCTColumns(ItemPropertyCostTable table)
        {
            table.CustomColumn01.Label = "Minor";
            table.CustomColumn01.Column = "Minor";
            table.CustomColumn01.DataType = MasterRepository.GetDataTypeByType(typeof(Trap));

            table.CustomColumn02.Label = "Average";
            table.CustomColumn02.Column = "Average";
            table.CustomColumn02.DataType = MasterRepository.GetDataTypeByType(typeof(Trap));

            table.CustomColumn03.Label = "Strong";
            table.CustomColumn03.Column = "Strong";
            table.CustomColumn03.DataType = MasterRepository.GetDataTypeByType(typeof(Trap));

            table.CustomColumn04.Label = "Deadly";
            table.CustomColumn04.Column = "Deadly";
            table.CustomColumn04.DataType = MasterRepository.GetDataTypeByType(typeof(Trap));

            table.CustomColumn05.Label = "Epic";
            table.CustomColumn05.Column = "Epic";
            table.CustomColumn05.DataType = MasterRepository.GetDataTypeByType(typeof(Trap));
        }

        private void ImportTrapCostCTValues(LineRecord record, ItemPropertyCostTableItem item)
        {
            item.CustomColumnValue01.Value = CreateRef<Trap>(record.AsInteger("Minor"));
            item.CustomColumnValue02.Value = CreateRef<Trap>(record.AsInteger("Average"));
            item.CustomColumnValue03.Value = CreateRef<Trap>(record.AsInteger("Strong"));
            item.CustomColumnValue04.Value = CreateRef<Trap>(record.AsInteger("Deadly"));
            item.CustomColumnValue05.Value = CreateRef<Trap>(record.AsInteger("Epic"));
        }

        private void ImportMonstCostCTColumns(ItemPropertyCostTable table)
        {
            table.CustomColumn01.Label = "#Dice";
            table.CustomColumn01.Column = "NumDice";
            table.CustomColumn01.DataType = MasterRepository.GetDataTypeByType(typeof(int));

            table.CustomColumn02.Label = "Die";
            table.CustomColumn02.Column = "Die";
            table.CustomColumn02.DataType = MasterRepository.GetDataTypeByType(typeof(int));
        }

        private void ImportMonstCostCTValues(LineRecord record, ItemPropertyCostTableItem item)
        {

            item.CustomColumnValue01.Value = record.AsInteger("NumDice");
            item.CustomColumnValue02.Value = record.AsInteger("Die");
        }

        private void ImportItemPropertyCostTables()
        {
            var costTable2da = Load2da("iprp_costtable");
            Standard.ItemPropertyCostTables.Clear();
            for (int i = 0; i < costTable2da.Count; i++)
            {
                var tmpCostTable = new ItemPropertyCostTable();
                tmpCostTable.ID = GenerateGuid("iprp_costtable", i);
                tmpCostTable.Index = i;
                tmpCostTable.SourceLabel = costTable2da[i].AsString("Label");

                if (costTable2da[i].IsNull("Name")) continue;
                tmpCostTable.Name = costTable2da[i].AsString("Name") ?? "";
                tmpCostTable.ClientLoad = costTable2da[i].AsBoolean("ClientLoad");

                var costPropTable2da = Load2da(tmpCostTable.Name);

                switch (tmpCostTable.Name.ToLower())
                {
                    case "iprp_bonuscost":
                    case "iprp_immuncost":
                    case "iprp_srcost":
                    case "iprp_neg5cost":
                    case "iprp_neg10cost":
                    case "iprp_damvulcost":
                    case "iprp_onhitcost":
                    case "iprp_skillcost":
                    case "iprp_arcspell":
                        ImportValueOnlyCTColumns(tmpCostTable);
                        break;
                    case "iprp_meleecost":
                        ImportMeleeCostCTColumns(tmpCostTable);
                        break;
                    case "iprp_chargecost":
                        ImportChargeCostCTColumns(tmpCostTable);
                        break;
                    case "iprp_damagecost":
                        ImportDamageCostCTColumns(tmpCostTable);
                        break;
                    case "iprp_soakcost":
                    case "iprp_resistcost":
                        ImportAmountOnlyCTColumns(tmpCostTable);
                        break;
                    case "iprp_weightcost":
                    case "iprp_redcost":
                        ImportFloatValueOnlyCTColumns(tmpCostTable);
                        break;
                    case "iprp_ammocost":
                        ImportAmmoCostCTColumns(tmpCostTable);
                        break;
                    case "iprp_spellcost":
                        ImportSpellCostCTColumns(tmpCostTable);
                        break;
                    case "iprp_trapcost":
                        ImportTrapCostCTColumns(tmpCostTable);
                        break;
                    case "iprp_monstcost":
                        ImportMonstCostCTColumns(tmpCostTable);
                        break;
                }

                tmpCostTable.Clear();
                for (int j = 0; j < costPropTable2da.Count; j++)
                {
                    var tmpItem = new ItemPropertyCostTableItem(tmpCostTable);
                    if (costPropTable2da.Columns.IndexOf("Label") != -1)
                        tmpItem.SourceLabel = costPropTable2da[j].AsString("Label") ?? "";

                    SetText(tmpItem.Name, costPropTable2da[j].AsInteger("Name"));
                    tmpItem.Cost = costPropTable2da[j].AsFloat("Cost") ?? 1.0;
                    switch (tmpCostTable.Name.ToLower())
                    {
                        case "iprp_bonuscost":
                        case "iprp_immuncost":
                        case "iprp_srcost":
                        case "iprp_neg5cost":
                        case "iprp_neg10cost":
                        case "iprp_damvulcost":
                        case "iprp_onhitcost":
                        case "iprp_skillcost":
                        case "iprp_arcspell":
                            ImportValueOnlyCTValues(costPropTable2da[j], tmpItem);
                            break;
                        case "iprp_meleecost":
                            ImportMeleeCostCTValues(costPropTable2da[j], tmpItem);
                            break;
                        case "iprp_chargecost":
                            ImportChargeCostCTValues(costPropTable2da[j], tmpItem);
                            break;
                        case "iprp_damagecost":
                            ImportDamageCostCTValues(costPropTable2da[j], tmpItem);
                            break;
                        case "iprp_soakcost":
                        case "iprp_resistcost":
                            ImportAmountOnlyCTValues(costPropTable2da[j], tmpItem);
                            break;
                        case "iprp_weightcost":
                        case "iprp_redcost":
                            ImportFloatValueOnlyCTValues(costPropTable2da[j], tmpItem);
                            break;
                        case "iprp_ammocost":
                            ImportAmmoCostCTValues(costPropTable2da[j], tmpItem);
                            break;
                        case "iprp_spellcost":
                            ImportSpellCostCTValues(costPropTable2da[j], tmpItem);
                            break;
                        case "iprp_trapcost":
                            ImportTrapCostCTValues(costPropTable2da[j], tmpItem);
                            break;
                        case "iprp_monstcost":
                            ImportMonstCostCTValues(costPropTable2da[j], tmpItem);
                            break;
                    }

                    tmpCostTable.Add(tmpItem);
                }

                Standard.ItemPropertyCostTables.Add(tmpCostTable);
            }
        }

        private void ImportItemPropertyParams()
        {
            var paramTable2da = Load2da("iprp_paramtable");
            Standard.ItemPropertyParams.Clear();
            for (int i = 0; i < paramTable2da.Count; i++)
            {
                var tmpParam = new ItemPropertyParam();
                tmpParam.ID = GenerateGuid("iprp_paramtable", i);
                tmpParam.Index = i;
                tmpParam.SourceLabel = paramTable2da[i].AsString("Lable");

                if (!SetText(tmpParam.Name, paramTable2da[i].AsInteger("Name"))) continue;

                tmpParam.TableResRef = paramTable2da[i].AsString("TableResRef") ?? "";
                if (tmpParam.TableResRef != "")
                {
                    var paramTableGuid = GenerateGuid(tmpParam.TableResRef.ToLower(), 0);
                    if (!Standard.ItemPropertyTables.Contains(paramTableGuid))
                        ImportItemPropertyTable(tmpParam.TableResRef, paramTableGuid);
                    tmpParam.ItemPropertyTable = Standard.ItemPropertyTables.GetByID(paramTableGuid);
                }

                Standard.ItemPropertyParams.Add(tmpParam);
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
                tmpFeat.SourceLabel = feat2da[i].AsString("LABEL");

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
                tmpFeat.MasterFeat = CreateRef<MasterFeat>(feat2da[i].AsInteger("MASTERFEAT"));
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
                tmpSpell.SourceLabel = spells2da[i].AsString("Label");

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
                tmpSpell.TargetingFlags = (TargetFlag?)spells2da[i].AsInteger("TargetFlags", null) ?? (TargetFlag)0;

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
                tmpDisease.SourceLabel = disease2da[i].AsString("Label");

                if (!SetText(tmpDisease.Name, disease2da[i].AsInteger("Name"))) continue;

                tmpDisease.FirstSaveDC = disease2da[i].AsInteger("First_Save") ?? 0;
                tmpDisease.SecondSaveDC = disease2da[i].AsInteger("Subs_Save") ?? 0;
                tmpDisease.IncubationHours = disease2da[i].AsInteger("Incu_Hours") ?? 1;
                tmpDisease.AbilityDamage1Dice = disease2da[i].AsInteger("Dice_1");
                tmpDisease.AbilityDamage1DiceCount = disease2da[i].AsInteger("Dam_1");
                tmpDisease.AbilityDamage1Type = !disease2da[i].IsNull("Type_1") ? (AbilityType)Enum.ToObject(typeof(AbilityType), disease2da[i].AsInteger("Type_1") ?? 0) : null;
                tmpDisease.AbilityDamage2Dice = disease2da[i].AsInteger("Dice_2");
                tmpDisease.AbilityDamage2DiceCount = disease2da[i].AsInteger("Dam_2");
                tmpDisease.AbilityDamage2Type = !disease2da[i].IsNull("Type_2") ? (AbilityType)Enum.ToObject(typeof(AbilityType), disease2da[i].AsInteger("Type_2") ?? 0) : null;
                tmpDisease.AbilityDamage3Dice = disease2da[i].AsInteger("Dice_3");
                tmpDisease.AbilityDamage3DiceCount = disease2da[i].AsInteger("Dam_3");
                tmpDisease.AbilityDamage3Type = !disease2da[i].IsNull("Type_3") ? (AbilityType)Enum.ToObject(typeof(AbilityType), disease2da[i].AsInteger("Type_3") ?? 0) : null;
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
                tmpPoison.SourceLabel = poison2da[i].AsString("Label");

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

        private void ImportPackageSpellPreferences(string tablename, Guid guid)
        {
            var spellPreference2da = Load2da(tablename.ToLower());

            var packageSpellPrefs = new PackageSpellPreferencesTable();
            packageSpellPrefs.ID = guid;
            packageSpellPrefs.Name = tablename;

            packageSpellPrefs.Items.Clear();
            for (int i = 0; i < spellPreference2da.Count; i++)
            {
                var tmpItem = new PackageSpellPreferencesTableItem();
                tmpItem.ParentTable = packageSpellPrefs;
                tmpItem.SourceLabel = spellPreference2da[i].AsString("Label") ?? "";
                tmpItem.Spell = CreateRef<Spell>(spellPreference2da[i].AsInteger("SpellIndex"));
                packageSpellPrefs.Items.Add(tmpItem);
            }

            Standard.SpellPreferencesTables.Add(packageSpellPrefs);
        }

        private void ImportPackageFeatPreferences(string tablename, Guid guid)
        {
            var featPreference2da = Load2da(tablename.ToLower());

            var packageFeatPrefs = new PackageFeatPreferencesTable();
            packageFeatPrefs.ID = guid;
            packageFeatPrefs.Name = tablename;

            packageFeatPrefs.Items.Clear();
            for (int i = 0; i < featPreference2da.Count; i++)
            {
                var tmpItem = new PackageFeatPreferencesTableItem();
                tmpItem.ParentTable = packageFeatPrefs;
                tmpItem.SourceLabel = featPreference2da[i].AsString("Label") ?? "";
                tmpItem.Feat = CreateRef<Feat>(featPreference2da[i].AsInteger("FeatIndex"));
                packageFeatPrefs.Items.Add(tmpItem);
            }

            Standard.FeatPreferencesTables.Add(packageFeatPrefs);
        }

        private void ImportPackageSkillPreferences(string tablename, Guid guid)
        {
            var skillPreference2da = Load2da(tablename.ToLower());

            var packageSkillPrefs = new PackageSkillPreferencesTable();
            packageSkillPrefs.ID = guid;
            packageSkillPrefs.Name = tablename;

            packageSkillPrefs.Items.Clear();
            for (int i = 0; i < skillPreference2da.Count; i++)
            {
                var tmpItem = new PackageSkillPreferencesTableItem();
                tmpItem.ParentTable = packageSkillPrefs;
                tmpItem.Skill = CreateRef<Skill>(skillPreference2da[i].AsInteger("SkillIndex"));
                packageSkillPrefs.Items.Add(tmpItem);
            }

            Standard.SkillPreferencesTables.Add(packageSkillPrefs);
        }

        private void ImportPackageEquipmentTable(string tablename, Guid guid)
        {
            var packageEquipment2da = Load2da(tablename.ToLower());

            var packageEquipment = new PackageEquipmentTable();
            packageEquipment.ID = guid;
            packageEquipment.Name = tablename;

            packageEquipment.Items.Clear();
            for (int i = 0; i < packageEquipment2da.Count; i++)
            {
                var tmpItem = new PackageEquipmentTableItem();
                tmpItem.ParentTable = packageEquipment;
                tmpItem.BlueprintResRef = packageEquipment2da[i].AsString("Label") ?? "";
                packageEquipment.Items.Add(tmpItem);
            }

            Standard.PackageEquipmentTables.Add(packageEquipment);
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
                tmpPackage.SourceLabel = packages2da[i].AsString("Label");

                if (!SetText(tmpPackage.Name, packages2da[i].AsInteger("Name"))) continue;
                SetText(tmpPackage.Description, packages2da[i].AsInteger("Description"));

                tmpPackage.ForClass = CreateRef<CharacterClass>(packages2da[i].AsInteger("ClassID", -1));
                tmpPackage.PreferredAbility = Enum.Parse<AbilityType>(packages2da[i].AsString("Attribute") ?? "", true);
                tmpPackage.Gold = packages2da[i].AsInteger("Gold") ?? 0;
                tmpPackage.SpellSchool = !packages2da[i].IsNull("School") ? (SpellSchool)Enum.ToObject(typeof(SpellSchool), packages2da[i].AsInteger("School") ?? 0) : null;
                tmpPackage.Domain1 = CreateRef<Domain>(packages2da[i].AsInteger("Domain1", -1));
                tmpPackage.Domain2 = CreateRef<Domain>(packages2da[i].AsInteger("Domain2", -1));
                tmpPackage.AssociateCompanion = CreateRef<Companion>(packages2da[i].AsInteger("Associate", -1));
                tmpPackage.AssociateFamiliar = CreateRef<Familiar>(packages2da[i].AsInteger("Associate", -1));

                // Spell Preferences
                var spellPreferences = packages2da[i].AsString("SpellPref2DA");
                if (spellPreferences != null)
                {
                    var spellPreferencesGuid = GenerateGuid(spellPreferences.ToLower(), 0);
                    if (!Standard.SpellPreferencesTables.Contains(spellPreferencesGuid))
                        ImportPackageSpellPreferences(spellPreferences, spellPreferencesGuid);
                    tmpPackage.SpellPreferences = Standard.SpellPreferencesTables.GetByID(spellPreferencesGuid);
                }

                // Feat Preferences
                var featPreferences = packages2da[i].AsString("FeatPref2DA");
                if (featPreferences != null)
                {
                    var featPreferencesGuid = GenerateGuid(featPreferences.ToLower(), 0);
                    if (!Standard.FeatPreferencesTables.Contains(featPreferencesGuid))
                        ImportPackageFeatPreferences(featPreferences, featPreferencesGuid);
                    tmpPackage.FeatPreferences = Standard.FeatPreferencesTables.GetByID(featPreferencesGuid);
                }

                // Skill Preferences
                var skillPreferences = packages2da[i].AsString("SkillPref2DA");
                if (skillPreferences != null)
                {
                    var skillPreferencesGuid = GenerateGuid(skillPreferences.ToLower(), 0);
                    if (!Standard.SkillPreferencesTables.Contains(skillPreferencesGuid))
                        ImportPackageSkillPreferences(skillPreferences, skillPreferencesGuid);
                    tmpPackage.SkillPreferences = Standard.SkillPreferencesTables.GetByID(skillPreferencesGuid);
                }

                // Equipment
                var packageEquipment = packages2da[i].AsString("Equip2DA");
                if (packageEquipment != null)
                {
                    var packageEquipmentGuid = GenerateGuid(packageEquipment.ToLower(), 0);
                    if (!Standard.SkillPreferencesTables.Contains(packageEquipmentGuid))
                        ImportPackageEquipmentTable(packageEquipment, packageEquipmentGuid);
                    tmpPackage.StartingEquipment = Standard.PackageEquipmentTables.GetByID(packageEquipmentGuid);
                }

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
                tmpAreaEffect.SourceLabel = aoe2da[i].AsString("LABEL");

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
                tmpAreaEffect.VisualEffect = CreateRef<VisualEffect>(aoe2da[i].AsInteger("DurationVFX", -1));
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
                tmpSoundset.SourceLabel = soundset2da[i].AsString("LABEL");

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
                tmpPolymorph.SourceLabel = polymorph2da[i].AsString("Name");

                if (polymorph2da[i].IsNull("Name")) continue;
                tmpPolymorph.Name = polymorph2da[i].AsString("Name") ?? "";
                tmpPolymorph.Appearance = CreateRef<Appearance>(polymorph2da[i].AsInteger("AppearanceType"));
                tmpPolymorph.RacialType = CreateRef<Race>(polymorph2da[i].AsInteger("RacialType"));
                tmpPolymorph.Portrait = CreateRef<Portrait>(polymorph2da[i].AsInteger("PortraitId"));
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
                tmpAppearance.SourceLabel = appearance2da[i].AsString("LABEL");

                if (!SetText(tmpAppearance.Name, appearance2da[i].AsInteger("STRING_REF")))
                {
                    if ((tmpAppearance.SourceLabel != "") && (tmpAppearance.SourceLabel != null))
                    {
                        foreach (TLKLanguage lang in Enum.GetValues(typeof(TLKLanguage)))
                        {
                            tmpAppearance.Name[lang].Text = tmpAppearance.SourceLabel;
                            tmpAppearance.Name[lang].TextF = tmpAppearance.SourceLabel;
                        }
                    }
                    else
                        continue;
                }
                tmpAppearance.RaceModel = appearance2da[i].AsString("RACE") ?? "";
                tmpAppearance.EnvironmentMap = appearance2da[i].AsString("ENVMAP") ?? "";
                tmpAppearance.BloodColor = Enum.Parse<BloodColor>(appearance2da[i].AsString("BLOODCOLR") ?? "R", true);
                var modelType = (appearance2da[i].AsString("MODELTYPE") ?? "").ToUpper();
                for (int j = 0; j < modelType.Length; j++)
                {
                    if (modelType[j] == 'W')
                        tmpAppearance.CanHaveWings = true;
                    else if (modelType[j] == 'T')
                        tmpAppearance.CanHaveTails = true;
                    else
                        tmpAppearance.ModelType = Enum.Parse<ModelType>(modelType[j].ToString(), true);
                }
                tmpAppearance.WeaponScale = appearance2da[i].AsFloat("WEAPONSCALE");
                tmpAppearance.WingTailScale = appearance2da[i].AsFloat("WING_TAIL_SCALE");
                tmpAppearance.HelmetScaleMale = appearance2da[i].AsFloat("HELMET_SCALE_M");
                tmpAppearance.HelmetScaleFemale = appearance2da[i].AsFloat("HELMET_SCALE_F");
                tmpAppearance.MovementRate = Enum.Parse<MovementRate>(appearance2da[i].AsString("MOVERATE") ?? "NORM", true);
                tmpAppearance.WalkAnimationDistance = appearance2da[i].AsFloat("WALKDIST") ?? tmpAppearance.WalkAnimationDistance;
                tmpAppearance.RunAnimationDistance = appearance2da[i].AsFloat("RUNDIST") ?? tmpAppearance.RunAnimationDistance;
                tmpAppearance.PersonalSpaceRadius = appearance2da[i].AsFloat("PERSPACE") ?? tmpAppearance.PersonalSpaceRadius;
                tmpAppearance.CreaturePersonalSpaceRadius = appearance2da[i].AsFloat("CREPERSPACE") ?? tmpAppearance.CreaturePersonalSpaceRadius;
                tmpAppearance.CameraHeight = appearance2da[i].AsFloat("HEIGHT") ?? tmpAppearance.CameraHeight;
                tmpAppearance.HitDistance = appearance2da[i].AsFloat("HITDIST") ?? tmpAppearance.HitDistance;
                tmpAppearance.PreferredAttackDistance = appearance2da[i].AsFloat("PREFATCKDIST") ?? tmpAppearance.PreferredAttackDistance;
                tmpAppearance.TargetHeight = Enum.Parse<TargetHeight>(appearance2da[i].AsString("TARGETHEIGHT") ?? "H", true);
                tmpAppearance.AbortAttackAnimationOnParry = appearance2da[i].AsBoolean("ABORTONPARRY");
                tmpAppearance.HasLegs = appearance2da[i].AsBoolean("HASLEGS");
                tmpAppearance.HasArms = appearance2da[i].AsBoolean("HASARMS");
                tmpAppearance.Portrait = appearance2da[i].AsString("PORTRAIT");
                tmpAppearance.SizeCategory = !appearance2da[i].IsNull("SIZECATEGORY") ? (SizeCategory)Enum.ToObject(typeof(SoundsetType), appearance2da[i].AsInteger("SIZECATEGORY") ?? 0) : SizeCategory.Medium;
                tmpAppearance.PerceptionRange = !appearance2da[i].IsNull("PERCEPTIONDIST") ? (PerceptionDistance)Enum.ToObject(typeof(PerceptionDistance), appearance2da[i].AsInteger("PERCEPTIONDIST") ?? 0) : PerceptionDistance.Medium;
                tmpAppearance.FootstepSound = !appearance2da[i].IsNull("FOOTSTEPTYPE") ? (FootstepSound)Enum.ToObject(typeof(FootstepSound), appearance2da[i].AsInteger("FOOTSTEPTYPE") ?? 0) : FootstepSound.Normal;
                tmpAppearance.AppearanceSoundset = CreateRef<AppearanceSoundset>(appearance2da[i].AsInteger("SOUNDAPPTYPE"));
                tmpAppearance.HeadTracking = appearance2da[i].AsBoolean("HEADTRACK");
                tmpAppearance.HorizontalHeadTrackingRange = appearance2da[i].AsInteger("HEAD_ARC_H") ?? tmpAppearance.HorizontalHeadTrackingRange;
                tmpAppearance.VerticalHeadTrackingRange = appearance2da[i].AsInteger("HEAD_ARC_V") ?? tmpAppearance.VerticalHeadTrackingRange;
                tmpAppearance.ModelHeadNodeName = appearance2da[i].AsString("HEAD_NAME") ?? tmpAppearance.ModelHeadNodeName;
                tmpAppearance.BodyBag = !appearance2da[i].IsNull("SIZECATEGORY") ? (BodyBag)Enum.ToObject(typeof(BodyBag), appearance2da[i].AsInteger("BODY_BAG") ?? 0) : BodyBag.Default;
                tmpAppearance.Targetable = appearance2da[i].AsBoolean("TARGETABLE");

                Standard.Appearances.Add(tmpAppearance);
            }
        }

        private void ImportAppearanceSoundsets()
        {
            var appearancesndset2da = Load2da("appearancesndset");

            Standard.AppearanceSoundsets.Clear();
            for (int i = 0; i < appearancesndset2da.Count; i++)
            {
                var tmpAppearanceSoundset = new AppearanceSoundset();
                tmpAppearanceSoundset.ID = GenerateGuid("appearancesndset", i);
                tmpAppearanceSoundset.Index = i;
                tmpAppearanceSoundset.SourceLabel = appearancesndset2da[i].AsString("Label");

                if (appearancesndset2da[i].IsNull("Label")) continue;
                tmpAppearanceSoundset.Name = appearancesndset2da[i].AsString("Label") ?? "";
                tmpAppearanceSoundset.ArmorType = appearancesndset2da[i].IsNull("ArmorType") ? null : Enum.Parse<ArmorType>(appearancesndset2da[i].AsString("ArmorType") ?? "", true);
                tmpAppearanceSoundset.LeftAttack = CreateRef<WeaponSound>(appearancesndset2da[i].AsInteger("WeapTypeL"));
                tmpAppearanceSoundset.RightAttack = CreateRef<WeaponSound>(appearancesndset2da[i].AsInteger("WeapTypeR"));
                tmpAppearanceSoundset.StraightAttack = CreateRef<WeaponSound>(appearancesndset2da[i].AsInteger("WeapTypeS"));
                tmpAppearanceSoundset.LowCloseAttack = CreateRef<WeaponSound>(appearancesndset2da[i].AsInteger("WeapTypeClsLw"));
                tmpAppearanceSoundset.HighCloseAttack = CreateRef<WeaponSound>(appearancesndset2da[i].AsInteger("WeapTypeClsH"));
                tmpAppearanceSoundset.ReachAttack = CreateRef<WeaponSound>(appearancesndset2da[i].AsInteger("WeapTypeRch"));
                tmpAppearanceSoundset.Miss = CreateRef<WeaponSound>(appearancesndset2da[i].AsInteger("MissIndex"));
                tmpAppearanceSoundset.Looping = appearancesndset2da[i].AsString("Looping");
                tmpAppearanceSoundset.FallForward = appearancesndset2da[i].AsString("FallFwd");
                tmpAppearanceSoundset.FallBackward = appearancesndset2da[i].AsString("FallBck");

                Standard.AppearanceSoundsets.Add(tmpAppearanceSoundset);
            }
        }

        private void ImportWeaponSounds()
        {
            var weaponsounds2da = Load2da("weaponsounds");

            Standard.WeaponSounds.Clear();
            for (int i = 0; i < weaponsounds2da.Count; i++)
            {
                var tmpWeaponSound = new WeaponSound();
                tmpWeaponSound.ID = GenerateGuid("weaponsounds", i);
                tmpWeaponSound.Index = i;
                tmpWeaponSound.SourceLabel = weaponsounds2da[i].AsString("Label");

                if (weaponsounds2da[i].IsNull("Label")) continue;
                tmpWeaponSound.Name = weaponsounds2da[i].AsString("Label") ?? "";
                tmpWeaponSound.Leather0 = weaponsounds2da[i].AsString("Leather0");
                tmpWeaponSound.Leather1 = weaponsounds2da[i].AsString("Leather1");
                tmpWeaponSound.Chain0 = weaponsounds2da[i].AsString("Chain0");
                tmpWeaponSound.Chain1 = weaponsounds2da[i].AsString("Chain1");
                tmpWeaponSound.Plate0 = weaponsounds2da[i].AsString("Plate0");
                tmpWeaponSound.Plate1 = weaponsounds2da[i].AsString("Plate1");
                tmpWeaponSound.Stone0 = weaponsounds2da[i].AsString("Stone0");
                tmpWeaponSound.Stone1 = weaponsounds2da[i].AsString("Stone1");
                tmpWeaponSound.Wood0 = weaponsounds2da[i].AsString("Wood0");
                tmpWeaponSound.Wood1 = weaponsounds2da[i].AsString("Wood1");
                tmpWeaponSound.Chitin0 = weaponsounds2da[i].AsString("Chitin0");
                tmpWeaponSound.Chitin1 = weaponsounds2da[i].AsString("Chitin1");
                tmpWeaponSound.Scale0 = weaponsounds2da[i].AsString("Scale0");
                tmpWeaponSound.Scale1 = weaponsounds2da[i].AsString("Scale1");
                tmpWeaponSound.Ethereal0 = weaponsounds2da[i].AsString("Ethereal0");
                tmpWeaponSound.Ethereal1 = weaponsounds2da[i].AsString("Ethereal1");
                tmpWeaponSound.Crystal0 = weaponsounds2da[i].AsString("Crystal0");
                tmpWeaponSound.Crystal1 = weaponsounds2da[i].AsString("Crystal1");
                tmpWeaponSound.Miss0 = weaponsounds2da[i].AsString("Miss0");
                tmpWeaponSound.Miss1 = weaponsounds2da[i].AsString("Miss1");
                tmpWeaponSound.Parry = weaponsounds2da[i].AsString("Parry0");
                tmpWeaponSound.Critical = weaponsounds2da[i].AsString("Critical0");

                Standard.WeaponSounds.Add(tmpWeaponSound);
            }
        }

        public void ImportInventorySounds()
        {
            var inventorysnds2da = Load2da("inventorysnds");

            Standard.InventorySounds.Clear();
            for (int i = 0; i < inventorysnds2da.Count; i++)
            {
                var tmpInventorySound = new InventorySound();
                tmpInventorySound.ID = GenerateGuid("inventorysnds", i);
                tmpInventorySound.Index = i;
                tmpInventorySound.SourceLabel = inventorysnds2da[i].AsString("Label");

                if (inventorysnds2da[i].IsNull("Label")) continue;
                tmpInventorySound.Name = inventorysnds2da[i].AsString("Label") ?? "";
                tmpInventorySound.Sound = inventorysnds2da[i].AsString("InventorySound") ?? "";

                Standard.InventorySounds.Add(tmpInventorySound);
            }
        }

        private void ImportVisualEffects()
        {
            var vfx2da = Load2da("visualeffects");

            Standard.VisualEffects.Clear();
            for (int i = 0; i < vfx2da.Count; i++)
            {
                var tmpVfx = new VisualEffect();
                tmpVfx.ID = GenerateGuid("visualeffects", i);
                tmpVfx.Index = i;
                tmpVfx.SourceLabel = vfx2da[i].AsString("Label");

                if (vfx2da[i].IsNull("Type_FD")) continue;
                tmpVfx.Name = vfx2da[i].AsString("Label") ?? "";
                tmpVfx.Type = Enum.Parse<VisualEffectType>(vfx2da[i].AsString("Type_FD") ?? "F", true);
                tmpVfx.OrientWithGround = vfx2da[i].AsBoolean("OrientWithGround");
                tmpVfx.ImpactHeadEffect = vfx2da[i].AsString("Imp_HeadCon_Node");
                tmpVfx.ImpactImpactEffect = vfx2da[i].AsString("Imp_Impact_Node");
                tmpVfx.ImpactRootSmallEffect = vfx2da[i].AsString("Imp_Root_S_Node");
                tmpVfx.ImpactRootMediumEffect = vfx2da[i].AsString("Imp_Root_M_Node");
                tmpVfx.ImpactRootLargeEffect = vfx2da[i].AsString("Imp_Root_L_Node");
                tmpVfx.ImpactRootHugeEffect = vfx2da[i].AsString("Imp_Root_H_Node");
                tmpVfx.ImpactProgFX = CreateRef<ProgrammedEffect>(vfx2da[i].AsInteger("ProgFX_Impact"));
                tmpVfx.ImpactSound = vfx2da[i].AsString("SoundImpact");
                tmpVfx.DurationProgFX = CreateRef<ProgrammedEffect>(vfx2da[i].AsInteger("ProgFX_Duration"));
                tmpVfx.DurationSound = vfx2da[i].AsString("SoundDuration");
                tmpVfx.CessationProgFX = CreateRef<ProgrammedEffect>(vfx2da[i].AsInteger("ProgFX_Cessation"));
                tmpVfx.CessationSound = vfx2da[i].AsString("SoundCessastion");
                tmpVfx.CessationHeadEffect = vfx2da[i].AsString("Ces_HeadCon_Node");
                tmpVfx.CessationImpactEffect = vfx2da[i].AsString("Ces_Impact_Node");
                tmpVfx.CessationRootSmallEffect = vfx2da[i].AsString("Ces_Root_S_Node");
                tmpVfx.CessationRootMediumEffect = vfx2da[i].AsString("Ces_Root_M_Node");
                tmpVfx.CessationRootLargeEffect = vfx2da[i].AsString("Ces_Root_L_Node");
                tmpVfx.CessationRootHugeEffect = vfx2da[i].AsString("Ces_Root_H_Node");
                tmpVfx.ShakeType = (VFXShakeType)Enum.ToObject(typeof(VFXShakeType), vfx2da[i].AsInteger("ShakeType") ?? (int)VFXShakeType.None);
                tmpVfx.ShakeDelay = vfx2da[i].AsFloat("ShakeDelay");
                tmpVfx.ShakeDuration = vfx2da[i].AsFloat("ShakeDuration");
                tmpVfx.LowViolenceModel = vfx2da[i].AsString("LowViolence");
                tmpVfx.LowQualityModel = vfx2da[i].AsString("LowQuality");
                tmpVfx.OrientWithObject = vfx2da[i].AsBoolean("OrientWithObject");

                Standard.VisualEffects.Add(tmpVfx);
            }
        }

        private void ImportPortraits()
        {
            var portraits2da = Load2da("portraits");

            Standard.Portraits.Clear();
            for (int i = 0; i < portraits2da.Count; i++)
            {
                var tmpPortrait = new Portrait();
                tmpPortrait.ID = GenerateGuid("portraits", i);
                tmpPortrait.Index = i;
                tmpPortrait.SourceLabel = portraits2da[i].AsString("BaseResRef");

                if (portraits2da[i].IsNull("BaseResRef")) continue;
                tmpPortrait.ResRef = portraits2da[i].AsString("BaseResRef") ?? "";
                tmpPortrait.LowGoreResRef = portraits2da[i].AsString("LowGore");
                tmpPortrait.Gender = (PortraitGender?)portraits2da[i].AsInteger("Sex") ?? PortraitGender.Male;
                tmpPortrait.PlaceableType = (PlaceableType?)portraits2da[i].AsInteger("InanimateType");
                tmpPortrait.Race = CreateRef<Race>(portraits2da[i].AsInteger("Race"));
                tmpPortrait.IsPlayerPortrait = !portraits2da[i].AsBoolean("Plot");

                Standard.Portraits.Add(tmpPortrait);
            }
        }

        private void ImportCompanions()
        {
            var companions2da = Load2da("hen_companion");

            Standard.Companions.Clear();
            for (int i = 0; i < companions2da.Count; i++)
            {
                var tmpCompanion = new Companion();
                tmpCompanion.ID = GenerateGuid("hen_companion", i);
                tmpCompanion.Index = i;
                tmpCompanion.SourceLabel = companions2da[i].AsString("NAME");

                if (!SetText(tmpCompanion.Name, companions2da[i].AsInteger("STRREF"))) continue;
                SetText(tmpCompanion.Description, companions2da[i].AsInteger("DESCRIPTION"));
                tmpCompanion.Template = companions2da[i].AsString("BASERESREF") ?? "";

                Standard.Companions.Add(tmpCompanion);
            }
        }

        private void ImportFamiliars()
        {
            var familiars2da = Load2da("hen_familiar");

            Standard.Familiars.Clear();
            for (int i = 0; i < familiars2da.Count; i++)
            {
                var tmpFamiliar = new Familiar();
                tmpFamiliar.ID = GenerateGuid("hen_familiar", i);
                tmpFamiliar.Index = i;
                tmpFamiliar.SourceLabel = familiars2da[i].AsString("NAME");

                if (!SetText(tmpFamiliar.Name, familiars2da[i].AsInteger("STRREF"))) continue;
                SetText(tmpFamiliar.Description, familiars2da[i].AsInteger("DESCRIPTION"));
                tmpFamiliar.Template = familiars2da[i].AsString("BASERESREF") ?? "";

                Standard.Familiars.Add(tmpFamiliar);
            }
        }

        private void ImportTraps()
        {
            var traps2da = Load2da("traps");

            Standard.Traps.Clear();
            for (int i = 0; i < traps2da.Count; i++)
            {
                var tmpTrap = new Trap();
                tmpTrap.ID = GenerateGuid("traps", i);
                tmpTrap.Index = i;
                tmpTrap.SourceLabel = traps2da[i].AsString("Label");

                if (!SetText(tmpTrap.Name, traps2da[i].AsInteger("TrapName"))) continue;
                tmpTrap.TrapScript = traps2da[i].AsString("TrapScript") ?? "";
                tmpTrap.SetDC = traps2da[i].AsInteger("SetDC") ?? 10;
                tmpTrap.DetectDC = traps2da[i].AsInteger("DetectDCMod") ?? 10;
                tmpTrap.DisarmDC = traps2da[i].AsInteger("DisarmDCMod") ?? 10;
                tmpTrap.BlueprintResRef = traps2da[i].AsString("ResRef") ?? "";
                tmpTrap.Icon = traps2da[i].AsString("IconResRef") ?? "";

                Standard.Traps.Add(tmpTrap);
            }
        }

        private void ImportProgrammedEffects()
        {
            var progFX2da = Load2da("progfx");

            Standard.ProgrammedEffects.Clear();
            for (int i = 0; i < progFX2da.Count; i++)
            {
                var tmpProgFX = new ProgrammedEffect();
                tmpProgFX.ID = GenerateGuid("progfx", i);
                tmpProgFX.Index = i;
                tmpProgFX.SourceLabel = progFX2da[i].AsString("Label");

                if (progFX2da[i].IsNull("Label")) continue;
                tmpProgFX.Name = progFX2da[i].AsString("Label") ?? "";
                tmpProgFX.Type = (ProgrammedEffectType?)progFX2da[i].AsInteger("Type") ?? ProgrammedEffectType.Invalid;
                
                switch (tmpProgFX.Type)
                {
                    case ProgrammedEffectType.SkinOverlay:
                        tmpProgFX.T1ModelName = progFX2da[i].AsString("Param1") ?? "";
                        tmpProgFX.T1ArmorType = Enum.Parse<ArmorType>(progFX2da[i].AsString("Param2") ?? "leather", true);
                        tmpProgFX.T1OnHitVFX = CreateRef<VisualEffect>(progFX2da[i].AsInteger("Param3"));
                        tmpProgFX.T1OnHitVFXSmall = CreateRef<VisualEffect>(progFX2da[i].AsInteger("Param4"));
                        break;

                    case ProgrammedEffectType.EnvironmentMapping:
                        tmpProgFX.T2EnvironmentMap = progFX2da[i].AsString("Param1") ?? "";
                        break;

                    case ProgrammedEffectType.GlowEffect:
                        var colorR = Convert.ToUInt32((progFX2da[i].AsFloat("Param1") ?? 0.0) * 255.0);
                        var colorG = Convert.ToUInt32((progFX2da[i].AsFloat("Param2") ?? 0.0) * 255.0);
                        var colorB = Convert.ToUInt32((progFX2da[i].AsFloat("Param3") ?? 0.0) * 255.0);
                        tmpProgFX.T3GlowColor = colorB | (colorG << 8) | (colorR << 16);
                        break;

                    case ProgrammedEffectType.Lighting:
                        tmpProgFX.T4LightModelAnimation = progFX2da[i].AsString("Param1") ?? "";
                        tmpProgFX.T4AnimationSpeed = progFX2da[i].AsFloat("Param2") ?? 1.0;
                        tmpProgFX.T4CastShadows = progFX2da[i].AsBoolean("Param3");
                        tmpProgFX.T4Priority = progFX2da[i].AsInteger("Param4") ?? 20;
                        tmpProgFX.T4RemoveCloseToOtherLights = progFX2da[i].AsBoolean("Param5");
                        tmpProgFX.T4RemoveAllOtherLights = progFX2da[i].AsBoolean("Param6");
                        tmpProgFX.T4LightModel = progFX2da[i].AsString("Param7") ?? "";
                        break;

                    case ProgrammedEffectType.AlphaTransparency:
                        tmpProgFX.T5OpacityFrom = progFX2da[i].AsFloat("Param1") ?? 0.0;

                        var tcRed = progFX2da[i].AsFloat("Param2") ?? 0;
                        var tcGreen = progFX2da[i].AsFloat("Param3") ?? 0;
                        var tcBlue = progFX2da[i].AsFloat("Param4") ?? 0;
                        tmpProgFX.T5TransparencyColorKeepRed = (tcRed == -1);
                        tmpProgFX.T5TransparencyColorKeepGreen = (tcGreen == -1);
                        tmpProgFX.T5TransparencyColorKeepBlue = (tcBlue == -1);

                        uint transColorR = 0;
                        uint transColorG = 0;
                        uint transColorB = 0;
                        if (!tmpProgFX.T5TransparencyColorKeepRed)
                            transColorR = Convert.ToUInt32(tcRed * 255.0);
                        if (!tmpProgFX.T5TransparencyColorKeepRed)
                            transColorG = Convert.ToUInt32(tcGreen * 255.0);
                        if (!tmpProgFX.T5TransparencyColorKeepRed)
                            transColorB = Convert.ToUInt32(tcBlue * 255.0);
                        tmpProgFX.T5TransparencyColor = transColorB | (transColorG << 8) | (transColorR << 16);

                        tmpProgFX.T5FadeInterval = progFX2da[i].AsInteger("Param5") ?? 1000;
                        tmpProgFX.T5OpacityTo = progFX2da[i].AsFloat("Param6") ?? 1.0;
                        break;

                    case ProgrammedEffectType.PulsingAura:
                        var color1R = Convert.ToUInt32((progFX2da[i].AsFloat("Param1") ?? 0.0) * 255.0);
                        var color1G = Convert.ToUInt32((progFX2da[i].AsFloat("Param2") ?? 0.0) * 255.0);
                        var color1B = Convert.ToUInt32((progFX2da[i].AsFloat("Param3") ?? 0.0) * 255.0);
                        tmpProgFX.T6Color1 = color1B | (color1G << 8) | (color1R << 16);
                        var color2R = Convert.ToUInt32((progFX2da[i].AsFloat("Param4") ?? 0.0) * 255.0);
                        var color2G = Convert.ToUInt32((progFX2da[i].AsFloat("Param5") ?? 0.0) * 255.0);
                        var color2B = Convert.ToUInt32((progFX2da[i].AsFloat("Param6") ?? 0.0) * 255.0);
                        tmpProgFX.T6Color2 = color2B | (color2G << 8) | (color2R << 16);
                        tmpProgFX.T6FadeDuration = progFX2da[i].AsInteger("Param7") ?? 1000;
                        break;

                    case ProgrammedEffectType.Beam:
                        tmpProgFX.T7BeamModel = progFX2da[i].AsString("Param1") ?? "";
                        tmpProgFX.T7BeamAnimation = progFX2da[i].AsString("Param2") ?? "";
                        break;

                    case ProgrammedEffectType.MIRV:
                        tmpProgFX.T10ProjectileModel = progFX2da[i].AsString("Param1") ?? "";
                        tmpProgFX.T10Spell = CreateRef<Spell>(progFX2da[i].AsInteger("Param2"));
                        tmpProgFX.T10Orientation = (ProjectileOrientation?)progFX2da[i].AsInteger("Param3") ?? ProjectileOrientation.None;
                        tmpProgFX.T10ProjectilePath = (ProjectilePath?)progFX2da[i].AsInteger("Param4") ?? ProjectilePath.Default;
                        tmpProgFX.T10TravelTime = Enum.Parse<ProjectileTravelTime>(progFX2da[i].AsString("Param5") ?? "log", true);
                        break;

                    case ProgrammedEffectType.VariantMIRV:
                        tmpProgFX.T11ProjectileModel = progFX2da[i].AsString("Param1") ?? "";
                        tmpProgFX.T11FireSound = progFX2da[i].AsString("Param2") ?? "";
                        tmpProgFX.T11ImpactSound = progFX2da[i].AsString("Param3") ?? "";
                        tmpProgFX.T11ProjectilePath = (ProjectilePath?)progFX2da[i].AsInteger("Param4") ?? ProjectilePath.Default;
                        break;

                    case ProgrammedEffectType.SpellCastFailure:
                        tmpProgFX.T12ModelNode = progFX2da[i].AsString("Param1") ?? "";
                        tmpProgFX.T12EffectModel = progFX2da[i].AsString("Param2") ?? "";
                        break;
                }

                Standard.ProgrammedEffects.Add(tmpProgFX);
            }
        }

        private void ImportDamageTypes()
        {
            var damagetypes2da = Load2da("damagetypes");

            Standard.DamageTypes.Clear();
            for (int i = 0; i < damagetypes2da.Count; i++)
            {
                var tmpDamageType = new DamageType();
                tmpDamageType.ID = GenerateGuid("damagetypes", i);
                tmpDamageType.Index = i;
                tmpDamageType.SourceLabel = damagetypes2da[i].AsString("Label");

                if (!SetText(tmpDamageType.Name, damagetypes2da[i].AsInteger("CharsheetStrref"))) continue;
                tmpDamageType.Group = CreateRef<DamageTypeGroup>(damagetypes2da[i].AsInteger("DamageTypeGroup"));
               
                Standard.DamageTypes.Add(tmpDamageType);
            }
        }

        private void ImportDamageTypeGroups()
        {
            var damagetypegroups2da = Load2da("damagetypegroups");

            Standard.DamageTypeGroups.Clear();
            for (int i = 0; i < damagetypegroups2da.Count; i++)
            {
                var tmpDamageTypeGroup = new DamageTypeGroup();
                tmpDamageTypeGroup.ID = GenerateGuid("damagetypegroups", i);
                tmpDamageTypeGroup.Index = i;
                tmpDamageTypeGroup.SourceLabel = damagetypegroups2da[i].AsString("Label");

                if (!SetText(tmpDamageTypeGroup.FeedbackText, damagetypegroups2da[i].AsInteger("FeedbackStrref"))) continue;
                tmpDamageTypeGroup.Name = damagetypegroups2da[i].AsString("Label") ?? "";
                if (!damagetypegroups2da[i].IsNull("ColorR") && !damagetypegroups2da[i].IsNull("ColorG") && !damagetypegroups2da[i].IsNull("ColorB"))
                {
                    var colorR = Convert.ToUInt32(damagetypegroups2da[i].AsInteger("ColorR")) & 0xFF;
                    var colorG = Convert.ToUInt32(damagetypegroups2da[i].AsInteger("ColorG")) & 0xFF;
                    var colorB = Convert.ToUInt32(damagetypegroups2da[i].AsInteger("ColorB")) & 0xFF;
                    tmpDamageTypeGroup.Color = colorB | (colorG << 8) | (colorR << 16);
                }

                Standard.DamageTypeGroups.Add(tmpDamageTypeGroup);
            }
        }

        private T? SolveInstance<T>(T? instance, ModelRepository<T> repository) where T : BaseModel, new()
        {
            if (instance?.Index == null)
                return null;
            else
                return repository.GetByIndex(instance.Index ?? -1);
        }

        private object? SolveCustomValue(object? value, CustomObjectProperty prop)
        {
            if ((value is VariantValue varValue) && (varValue.Value is BaseModel varModel))
            {
                varValue.Value = MasterRepository.Standard.GetByIndex(varModel.GetType(), varModel.Index ?? -1);
            }
            else if (value is BaseModel model)
            {
                return MasterRepository.Standard.GetByIndex(model.GetType(), model.Index ?? -1);
            }

            return value;
        }

        private void ResolveDependencies()
        {
            Log.Info("Resolving object dependencies...");

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
                feat.MasterFeat = SolveInstance(feat.MasterFeat, Standard.MasterFeats);
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
                polymorph.Portrait = SolveInstance(polymorph.Portrait, Standard.Portraits);
                //polymorph.Soundset = SolveInstance(polymorph.Soundset, Standard.Soundsets); // Unused will not be imported!
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
                            item.Param1Class = SolveInstance(item.Param1Class, Standard.Classes);
                            break;

                        case RequirementType.FEAT:
                        case RequirementType.FEATOR:
                            item.Param1Feat = SolveInstance(item.Param1Feat, Standard.Feats);
                            break;

                        case RequirementType.RACE:
                            item.Param1Race = SolveInstance(item.Param1Race, Standard.Races);
                            break;

                        case RequirementType.SKILL:
#if SPACEPOPE
                        case RequirementType.SKILLOR:
#endif
                            item.Param1Skill = SolveInstance(item.Param1Skill, Standard.Skills);
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

                if (package.ForClass != null && package.ForClass.IsSpellCaster)
                {
                    if (package.ForClass.IsArcaneCaster)
                    {
                        package.AssociateCompanion = null;
                        package.AssociateFamiliar = SolveInstance(package.AssociateFamiliar, Standard.Familiars); ;
                    }
                    else
                    {
                        package.AssociateCompanion = SolveInstance(package.AssociateCompanion, Standard.Companions);
                        package.AssociateFamiliar = null;
                    }
                }
                else
                {
                    package.AssociateCompanion = null;
                    package.AssociateFamiliar = null;
                }
            }

            // Package Spell Preferences
            foreach (var spellPrefs in Standard.SpellPreferencesTables)
            {
                if (spellPrefs == null) continue;
                for (int i = 0; i < spellPrefs.Items.Count; i++)
                {
                    var item = spellPrefs[i];
                    if (item == null) continue;
                    item.Spell = SolveInstance(item.Spell, Standard.Spells);
                }
            }

            // Package Feat Preferences
            foreach (var featPrefs in Standard.FeatPreferencesTables)
            {
                if (featPrefs == null) continue;
                for (int i = 0; i < featPrefs.Items.Count; i++)
                {
                    var item = featPrefs[i];
                    if (item == null) continue;
                    item.Feat = SolveInstance(item.Feat, Standard.Feats);
                }
            }

            // Package Skill Preferences
            foreach (var skillPrefs in Standard.SkillPreferencesTables)
            {
                if (skillPrefs == null) continue;
                for (int i = 0; i < skillPrefs.Items.Count; i++)
                {
                    var item = skillPrefs[i];
                    if (item == null) continue;
                    item.Skill = SolveInstance(item.Skill, Standard.Skills);
                }
            }

            // Area Effects
            foreach (var aoe in Standard.AreaEffects)
            {
                if (aoe == null) continue;
                aoe.VisualEffect = SolveInstance(aoe.VisualEffect, Standard.VisualEffects);
            }

            // Appearances
            foreach (var appearance in Standard.Appearances)
            {
                if (appearance == null) continue;
                appearance.AppearanceSoundset = SolveInstance(appearance.AppearanceSoundset, Standard.AppearanceSoundsets);
            }

            // Appearance Soundsets
            foreach (var appearanceSoundset in Standard.AppearanceSoundsets)
            {
                if (appearanceSoundset == null) continue;
                appearanceSoundset.LeftAttack = SolveInstance(appearanceSoundset.LeftAttack, Standard.WeaponSounds);
                appearanceSoundset.RightAttack = SolveInstance(appearanceSoundset.RightAttack, Standard.WeaponSounds);
                appearanceSoundset.StraightAttack = SolveInstance(appearanceSoundset.StraightAttack, Standard.WeaponSounds);
                appearanceSoundset.HighCloseAttack = SolveInstance(appearanceSoundset.HighCloseAttack, Standard.WeaponSounds);
                appearanceSoundset.LowCloseAttack = SolveInstance(appearanceSoundset.LowCloseAttack, Standard.WeaponSounds);
                appearanceSoundset.ReachAttack = SolveInstance(appearanceSoundset.ReachAttack, Standard.WeaponSounds);
                appearanceSoundset.Miss = SolveInstance(appearanceSoundset.Miss, Standard.WeaponSounds);
            }

            // Base Items
            foreach (var baseItem in Standard.BaseItems)
            {
                if (baseItem == null) continue;
                baseItem.AmmunitionBaseItem = SolveInstance(baseItem.AmmunitionBaseItem, Standard.BaseItems);
                baseItem.InventorySound = SolveInstance(baseItem.InventorySound, Standard.InventorySounds);
                baseItem.ItemPropertySet = SolveInstance(baseItem.ItemPropertySet, Standard.ItemPropertySets);
                baseItem.RequiredFeat1 = SolveInstance(baseItem.RequiredFeat1, Standard.Feats);
                baseItem.RequiredFeat2 = SolveInstance(baseItem.RequiredFeat2, Standard.Feats);
                baseItem.RequiredFeat3 = SolveInstance(baseItem.RequiredFeat3, Standard.Feats);
                baseItem.RequiredFeat4 = SolveInstance(baseItem.RequiredFeat4, Standard.Feats);
                baseItem.RequiredFeat5 = SolveInstance(baseItem.RequiredFeat5, Standard.Feats);
                baseItem.WeaponSound = SolveInstance(baseItem.WeaponSound, Standard.WeaponSounds);
                baseItem.WeaponFocusFeat = SolveInstance(baseItem.WeaponFocusFeat, Standard.Feats);
                baseItem.EpicWeaponFocusFeat = SolveInstance(baseItem.EpicWeaponFocusFeat, Standard.Feats);
                baseItem.WeaponSpecializationFeat = SolveInstance(baseItem.WeaponSpecializationFeat, Standard.Feats);
                baseItem.EpicWeaponSpecializationFeat = SolveInstance(baseItem.EpicWeaponSpecializationFeat, Standard.Feats);
                baseItem.ImprovedCriticalFeat = SolveInstance(baseItem.ImprovedCriticalFeat, Standard.Feats);
                baseItem.OverwhelmingCriticalFeat = SolveInstance(baseItem.OverwhelmingCriticalFeat, Standard.Feats);
                baseItem.DevastatingCriticalFeat = SolveInstance(baseItem.DevastatingCriticalFeat, Standard.Feats);
                baseItem.WeaponOfChoiceFeat = SolveInstance(baseItem.WeaponOfChoiceFeat, Standard.Feats);
            }

            // Portraits
            foreach (var portrait in Standard.Portraits)
            {
                if (portrait == null) continue;
                portrait.Race = SolveInstance(portrait.Race, Standard.Races);
            }

            // Visual Effects
            foreach (var vfx in Standard.VisualEffects)
            {
                if (vfx == null) continue;
                vfx.ImpactProgFX = SolveInstance(vfx.ImpactProgFX, Standard.ProgrammedEffects);
                vfx.DurationProgFX = SolveInstance(vfx.DurationProgFX, Standard.ProgrammedEffects);
                vfx.CessationProgFX = SolveInstance(vfx.CessationProgFX, Standard.ProgrammedEffects);
            }

            // ProgFX
            foreach (var progfx in Standard.ProgrammedEffects)
            {
                if (progfx == null) continue;
                progfx.T1OnHitVFX = SolveInstance(progfx.T1OnHitVFX, Standard.VisualEffects);
                progfx.T1OnHitVFXSmall = SolveInstance(progfx.T1OnHitVFXSmall, Standard.VisualEffects);
                progfx.T10Spell = SolveInstance(progfx.T10Spell, Standard.Spells);
            }

            // Damage Types
            foreach (var damageType in Standard.DamageTypes)
            {
                if (damageType == null) continue;
                damageType.Group = SolveInstance(damageType.Group, Standard.DamageTypeGroups);
            }

            // Item Properties
            foreach (var itemProp in Standard.ItemProperties)
            {
                if (itemProp == null) continue;
                itemProp.CostTable = SolveInstance(itemProp.CostTable, Standard.ItemPropertyCostTables);
                itemProp.Param = SolveInstance(itemProp.Param, Standard.ItemPropertyParams);
            }

            // Item Property Sets
            foreach (var itemPropSet in Standard.ItemPropertySets)
            {
                if (itemPropSet == null) continue;

                for (int i = 0; i < itemPropSet.ItemProperties.Count; i++)
                {
                    var item = itemPropSet.ItemProperties[i];
                    if (item == null) continue;
                    item.ItemProperty = SolveInstance(item.ItemProperty, Standard.ItemProperties);
                }
            }

            // Item Property Tables
            foreach (var propTable in Standard.ItemPropertyTables)
            {
                if (propTable == null) continue;

                foreach (var tableItem in propTable.Items)
                {
                    if (tableItem == null) continue;

                    tableItem.CustomColumnValue01.Value = SolveCustomValue(tableItem.CustomColumnValue01.Value, propTable.CustomColumn01);
                    tableItem.CustomColumnValue02.Value = SolveCustomValue(tableItem.CustomColumnValue02.Value, propTable.CustomColumn02);
                    tableItem.CustomColumnValue03.Value = SolveCustomValue(tableItem.CustomColumnValue03.Value, propTable.CustomColumn03);
                    tableItem.CustomColumnValue04.Value = SolveCustomValue(tableItem.CustomColumnValue04.Value, propTable.CustomColumn04);
                    tableItem.CustomColumnValue05.Value = SolveCustomValue(tableItem.CustomColumnValue05.Value, propTable.CustomColumn05);
                    tableItem.CustomColumnValue06.Value = SolveCustomValue(tableItem.CustomColumnValue06.Value, propTable.CustomColumn06);
                    tableItem.CustomColumnValue07.Value = SolveCustomValue(tableItem.CustomColumnValue07.Value, propTable.CustomColumn07);
                    tableItem.CustomColumnValue08.Value = SolveCustomValue(tableItem.CustomColumnValue08.Value, propTable.CustomColumn08);
                    tableItem.CustomColumnValue09.Value = SolveCustomValue(tableItem.CustomColumnValue09.Value, propTable.CustomColumn09);
                    tableItem.CustomColumnValue10.Value = SolveCustomValue(tableItem.CustomColumnValue10.Value, propTable.CustomColumn10);
                }
            }

            // Item Property CostTables
            foreach (var costTable in Standard.ItemPropertyCostTables)
            {
                if (costTable == null) continue;

                foreach (var tableItem in costTable.Items)
                {
                    if (tableItem == null) continue;

                    tableItem.CustomColumnValue01.Value = SolveCustomValue(tableItem.CustomColumnValue01.Value, costTable.CustomColumn01);
                    tableItem.CustomColumnValue02.Value = SolveCustomValue(tableItem.CustomColumnValue02.Value, costTable.CustomColumn02);
                    tableItem.CustomColumnValue03.Value = SolveCustomValue(tableItem.CustomColumnValue03.Value, costTable.CustomColumn03);
                    tableItem.CustomColumnValue04.Value = SolveCustomValue(tableItem.CustomColumnValue04.Value, costTable.CustomColumn04);
                    tableItem.CustomColumnValue05.Value = SolveCustomValue(tableItem.CustomColumnValue05.Value, costTable.CustomColumn05);
                    tableItem.CustomColumnValue06.Value = SolveCustomValue(tableItem.CustomColumnValue06.Value, costTable.CustomColumn06);
                    tableItem.CustomColumnValue07.Value = SolveCustomValue(tableItem.CustomColumnValue07.Value, costTable.CustomColumn07);
                    tableItem.CustomColumnValue08.Value = SolveCustomValue(tableItem.CustomColumnValue08.Value, costTable.CustomColumn08);
                    tableItem.CustomColumnValue09.Value = SolveCustomValue(tableItem.CustomColumnValue09.Value, costTable.CustomColumn09);
                    tableItem.CustomColumnValue10.Value = SolveCustomValue(tableItem.CustomColumnValue10.Value, costTable.CustomColumn10);
                }
            }
        }

        private void SaveToJson()
        {
            Log.Info("Saving imported data...");

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
            Standard.MasterFeats.SaveToFile(Constants.MasterFeatsFilePath);
            Standard.BaseItems.SaveToFile(Constants.BaseItemsFilePath);
            Standard.ItemPropertySets.SaveToFile(Constants.ItemPropertySetsFilePath);
            Standard.ItemProperties.SaveToFile(Constants.ItemPropertiesFilePath);

            Standard.Appearances.SaveToFile(Constants.AppearancesFilePath);
            Standard.AppearanceSoundsets.SaveToFile(Constants.AppearanceSoundsetsFilePath);
            Standard.WeaponSounds.SaveToFile(Constants.WeaponSoundsFilePath);
            Standard.InventorySounds.SaveToFile(Constants.InventorySoundsFilePath);
            Standard.Portraits.SaveToFile(Constants.PortraitsFilePath);
            Standard.VisualEffects.SaveToFile(Constants.VisualEffectsFilePath);
            Standard.ClassPackages.SaveToFile(Constants.ClassPackagesFilePath);
            Standard.Soundsets.SaveToFile(Constants.SoundsetsFilePath);
            Standard.Polymorphs.SaveToFile(Constants.PolymorphsFilePath);
            Standard.Companions.SaveToFile(Constants.CompanionsFilePath);
            Standard.Familiars.SaveToFile(Constants.FamiliarsFilePath);
            Standard.Traps.SaveToFile(Constants.TrapsFilePath);
            Standard.ProgrammedEffects.SaveToFile(Constants.ProgrammedEffectsFilePath);
            Standard.DamageTypes.SaveToFile(Constants.DamageTypesFilePath);
            Standard.DamageTypeGroups.SaveToFile(Constants.DamageTypeGroupsFilePath);

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
            Standard.SpellPreferencesTables.SaveToFile(Constants.SpellPreferencesTablesFilePath);
            Standard.FeatPreferencesTables.SaveToFile(Constants.FeatPreferencesTablesFilePath);
            Standard.SkillPreferencesTables.SaveToFile(Constants.SkillPreferencesTablesFilePath);
            Standard.PackageEquipmentTables.SaveToFile(Constants.PackageEquipmentTablesFilePath);

            Standard.ItemPropertyTables.SaveToFile(Constants.ItemPropertyTablesFilePath);
            Standard.ItemPropertyCostTables.SaveToFile(Constants.ItemPropertyCostTablesFilePath);
            Standard.ItemPropertyParams.SaveToFile(Constants.ItemPropertyParamsFilePath);
        }

        private void ImportIcons()
        {
            Log.Info("Importing referenced icons...");

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
            Standard.SpellPreferencesTables.Clear();
            Standard.FeatPreferencesTables.Clear();
            Standard.SkillPreferencesTables.Clear();
            Standard.PackageEquipmentTables.Clear();
            Standard.ItemPropertyTables.Clear();
            Standard.ItemPropertyCostTables.Clear();
        }

        public void Import(string nwnBasePath)
        {
            Log.Info("Importing base game data...");
            try
            {
                this.nwnBasePath = nwnBasePath;
                tlk.Load(nwnBasePath);
                bif.Load(nwnBasePath);
                if (File.Exists(Path.Combine(nwnBasePath, "data", "nwn_retail.key")))
                    ovrBif.Load(nwnBasePath, Path.Combine("data", "nwn_retail.key"));

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
                ImportMasterFeats();
                ImportBaseItems();
                ImportItemProperties();
                ImportItemPropertyCostTables();
                ImportItemPropertyParams();
                ImportClassPackages();

                ImportAppearances();
                ImportAppearanceSoundsets();
                ImportWeaponSounds();
                ImportInventorySounds();
                ImportVisualEffects();
                ImportSoundsets();
                ImportPolymorphs();
                ImportPortraits();
                ImportCompanions();
                ImportFamiliars();
                ImportTraps();
                ImportProgrammedEffects();
                ImportDamageTypes();
                ImportDamageTypeGroups();

                ImportText();

                ResolveDependencies();

                Standard.Domains.Sort(d => d?.Name[TLKLanguage.English].Text);
                Standard.Skills.Sort(s => s?.Name[TLKLanguage.English].Text);
                Standard.Feats.Sort(f => f?.Name[TLKLanguage.English].Text);
                Standard.Spells.Sort(s => s?.Name[TLKLanguage.English].Text);
                Standard.Diseases.Sort(d => d?.Name[TLKLanguage.English].Text);
                Standard.Poisons.Sort(p => p?.Name[TLKLanguage.English].Text);
                Standard.Spellbooks.Sort(p => p?.Name);
                Standard.BaseItems.Sort(p => p?.Name[TLKLanguage.English].Text);

                Standard.AreaEffects.Sort(p => p?.Name);
                Standard.Polymorphs.Sort(p => p?.Name);
                Standard.Appearances.Sort(p => p?.Name[TLKLanguage.English].Text);
                Standard.ClassPackages.Sort(p => p?.Name[TLKLanguage.English].Text);
                Standard.Soundsets.Sort(p => p?.Name[TLKLanguage.English].Text);

                Standard.SpellPreferencesTables.Sort(p => p?.Name);
                Standard.FeatPreferencesTables.Sort(p => p?.Name);
                Standard.SkillPreferencesTables.Sort(p => p?.Name);

                SaveToJson();
                ImportIcons();

                EosConfig.BaseGameDataBuildDate = EosConfig.GetGameBuildDate(nwnBasePath);
                EosConfig.Save();
            }
            catch(Exception e)
            {
                Log.Error(e.Message);
                throw;
            }

            Log.Info("Base game data import successful!");
        }
    }
}
