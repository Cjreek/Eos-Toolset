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
using Eos.Repositories.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Services
{
    public delegate void ImportPreviewEventHandler(object sender, RepositoryCollection importedData, ref bool continueImport);

    public class ImportService
    {
        private delegate T ImportTableDelegate<T>(string tableName, Guid guid) where T : BaseModel;

        private IEnumerable<string> _importFiles;
        private TlkFile _tlkFile;
        private bool _importOverrides;
        private bool _replaceOverrides;
        private bool _importNewData;
        private string _externalDataPath;

        private TlkCollection gameDataTlk = new TlkCollection();
        private BifCollection gameDataBif = new BifCollection();

        private HashSet<String> iconResourceBuffer = new HashSet<String>();
        private List<(TLKStringSet strSet, int? strRef)> tlkBuffer = new List<(TLKStringSet strSet, int? strRef)>();
        private RepositoryCollection _importCollection = new RepositoryCollection(true);

        private Dictionary<String, TwoDimensionalArrayFile> customDataDict = new Dictionary<string, TwoDimensionalArrayFile>();

        public event ImportPreviewEventHandler? ImportPreview;

        public ImportService(IEnumerable<string> files, string tlkFile, bool importOverrides, bool replaceOverrides, bool importNewData, string externalDataPath) 
        {
            _importFiles = files;
            _tlkFile = new TlkFile();
            _tlkFile.Load(tlkFile);
            _importOverrides = importOverrides;
            _replaceOverrides = replaceOverrides;
            _importNewData = importNewData;
            _externalDataPath = externalDataPath;
        }

        private void Set2da(string name, TwoDimensionalArrayFile data)
        {
            customDataDict[name.ToLower()] = data;
        }

        private void Import2da(string filename)
        {
            Log.Info("Loading 2DA: {0}", filename);

            var data = new TwoDimensionalArrayFile();
            data.Load(filename);
            Set2da(Path.GetFileNameWithoutExtension(filename), data);
        }

        private void ImportToExternalPath(ErfResource resource, ErfFile sourceHak)
        {
            var fileExt = resource.ResourceType.ToString().ToLower().Replace("twoda", "2da");

            Log.Info("Importing external file: \"{0}\"", Path.Combine(_externalDataPath, resource.ResRef + "." + fileExt));
            
            using (var fileStream = File.Create(Path.Combine(_externalDataPath, resource.ResRef + "." + fileExt)))
            {
                var resStream = sourceHak.Read(resource.ResRef, resource.ResourceType);
                resStream.CopyTo(fileStream);
                fileStream.Flush();
            }
        }

        public void ImportHak(string filename)
        {
            Log.Info("Loading HAK: {0}", filename);

            var hak = new ErfFile();
            hak.Load(filename);
            try
            {
                for (int i = 0; i < hak.Count; i++)
                {
                    if (hak[i].ResourceType == NWNResourceType.TWODA)
                    {
                        var data = new TwoDimensionalArrayFile();
                        data.Load(hak.Read(hak[i].ResRef, hak[i].ResourceType));
                        Set2da(hak[i].ResRef, data);
                    }
                }
            }
            finally
            {
                hak.Close();
            }
        }

        private TwoDimensionalArrayFile LoadOriginal2da(string resRef)
        {
            var filename = Path.Combine(EosConfig.NwnBasePath, "ovr", resRef + ".2da");
            if (File.Exists(filename))
            {
                using (var fs = File.OpenRead(filename))
                {
                    return new TwoDimensionalArrayFile(fs);
                }
            }
            else
            {
                var resource = gameDataBif.ReadResource(resRef, NWNResourceType.TWODA);
                return new TwoDimensionalArrayFile(resource.RawData);
            }
        }

        private Guid GenerateGuid(string prefix, int index)
        {
            var data = prefix + "###" + index.ToString();
            var guidBytes = MD5.HashData(Encoding.Unicode.GetBytes(data));
            return new Guid(guidBytes);
        }

        private T CreateRef<T>(int? index) where T : BaseModel, new()
        {
            T result = new T();
            result.Index = index;
            return result;
        }

        private String? AddIconResource(String? resRef, NWNResourceSource source = NWNResourceSource.BIF)
        {
            if (resRef != null)
                iconResourceBuffer.Add(resRef);
            return resRef;
        }

        private bool SetText(TLKStringSet str, int? strRef)
        {
            var result = strRef != null;
            if (result)
            {
                str.OriginalIndex = strRef;
                tlkBuffer.Add((str, strRef));
            }

            return result;
        }

        private void ImportText()
        {
            foreach (var tlkPair in tlkBuffer)
            {
                if (tlkPair.strRef >= 0x01000000)
                {
                    tlkPair.strSet[MasterRepository.Project.DefaultLanguage].Text = _tlkFile.GetString(tlkPair.strRef - 0x01000000);
                    tlkPair.strSet[MasterRepository.Project.DefaultLanguage].TextF = _tlkFile.GetString(tlkPair.strRef - 0x01000000);
                }
                else
                {
                    foreach (TLKLanguage lang in Enum.GetValues(typeof(TLKLanguage)))
                    {
                        tlkPair.strSet[lang].Text = gameDataTlk.GetString(lang, false, tlkPair.strRef);
                        tlkPair.strSet[lang].TextF = gameDataTlk.GetString(lang, true, tlkPair.strRef);
                    }
                }
            }
        }

        private T? GetOrImportTable<T>(string? tableName, ImportTableDelegate<T> importTableFunc) where T : BaseModel
        {
            if (tableName == null) return null;

            var tableGuid = GenerateGuid(tableName.ToLower(), 0);
            var originalModel = MasterRepository.Standard.GetByID(typeof(T), tableGuid);
            var originalOverride = MasterRepository.Project.GetOverride(originalModel);

            T? result = (T?)originalOverride ?? (T?)originalModel;
            if (customDataDict.ContainsKey(tableName.ToLower()))
            {
                if ((originalModel == null) || ((originalModel != null) && (_importOverrides) && ((originalOverride == null) || (_replaceOverrides))))
                {
                    Log.Info("Importing 2DA: {0}.2da", tableName);
                    result = importTableFunc(tableName, result != null ? tableGuid : Guid.NewGuid());
                }
                customDataDict.Remove(tableName.ToLower());
            }

            return result;
        }

        private RacialFeatsTable ImportRacialFeatsTable(string tablename, Guid guid)
        {
            var racialFeatTable2da = customDataDict[tablename.ToLower()];

            var tmpRacialFeatsTable = new RacialFeatsTable();
            tmpRacialFeatsTable.ID = guid;
            tmpRacialFeatsTable.Name = tablename.ToUpper();

            tmpRacialFeatsTable.Clear();
            for (int i = 0; i < racialFeatTable2da.Count; i++)
            {
                var tmpItem = new RacialFeatsTableItem();
                tmpItem.ParentTable = tmpRacialFeatsTable;
                tmpItem.Feat = CreateRef<Feat>(racialFeatTable2da[i].AsInteger("FeatIndex"));
                tmpRacialFeatsTable.Add(tmpItem);
            }

            _importCollection.RacialFeatsTables.Add(tmpRacialFeatsTable);

            return tmpRacialFeatsTable;
        }

        private bool ImportRecord<T>(int index, TwoDimensionalArrayFile import2DA, TwoDimensionalArrayFile origina2DA, out Guid recordId) where T: BaseModel
        {
            recordId = Guid.Empty;
            var result = false;
            if (index < origina2DA.Count)
            {
                if (_importOverrides)
                {
                    var originalModel = MasterRepository.Standard.GetByIndex(typeof(T), index);
                    var originalOverride = MasterRepository.Project.GetOverride(originalModel);
                    if ((originalModel != null) && ((originalOverride == null) || (_replaceOverrides)))
                    {
                        var importRec = import2DA[index];
                        var originalRec = origina2DA[index];

                        if (!importRec.Equals(originalRec))
                        {
                            recordId = originalModel.ID;
                            result = true;
                        }
                    }
                }
            }
            else
                result = _importNewData;

            return result;
        }

        private void ImportRaces()
        {
            if (customDataDict.TryGetValue("racialtypes", out var races2da))
            {
                customDataDict.Remove("racialtypes");

                Log.Info("Importing 2DA: racialtypes.2da");

                var originalRaces2da = LoadOriginal2da("racialtypes");
                for (int i = 0; i < races2da.Count; i++)
                {
                    if (!ImportRecord<Race>(i, races2da, originalRaces2da, out var recordId)) continue;

                    var tmpRace = new Race();
                    tmpRace.ID = recordId;
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
                    tmpRace.Feats = GetOrImportTable(races2da[i].AsString("FeatsTable"), ImportRacialFeatsTable);

                    _importCollection.Races.Add(tmpRace);
                }
            }
        }

        private AttackBonusTable ImportAttackBonusTable(string tablename, Guid guid)
        {
            var abTable2da = customDataDict[tablename.ToLower()];

            var tmpAttackBonusTable = new AttackBonusTable();
            tmpAttackBonusTable.ID = guid;
            tmpAttackBonusTable.Name = tablename.ToUpper();

            tmpAttackBonusTable.Clear();
            for (int i = 0; i < abTable2da.Count; i++)
            {
                var tmpItem = new AttackBonusTableItem();
                tmpItem.ParentTable = tmpAttackBonusTable;
                tmpItem.Level = i + 1;
                tmpItem.AttackBonus = abTable2da[i].AsInteger("BAB") ?? 0;
                tmpAttackBonusTable.Add(tmpItem);
            }

            _importCollection.AttackBonusTables.Add(tmpAttackBonusTable);

            return tmpAttackBonusTable;
        }

        private FeatsTable ImportFeatsTable(string tablename, Guid guid)
        {
            var featTable2da = customDataDict[tablename.ToLower()];

            var tmpFeatsTable = new FeatsTable();
            tmpFeatsTable.ID = guid;
            tmpFeatsTable.Name = tablename.ToUpper();

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

            _importCollection.FeatTables.Add(tmpFeatsTable);

            return tmpFeatsTable;
        }

        private SavingThrowTable ImportSavingThrowTable(string tablename, Guid guid)
        {
            var savesTable2da = customDataDict[tablename.ToLower()];

            var tmpSavesTable = new SavingThrowTable();
            tmpSavesTable.ID = guid;
            tmpSavesTable.Name = tablename.ToUpper();

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

            _importCollection.SavingThrowTables.Add(tmpSavesTable);

            return tmpSavesTable;
        }

        private BonusFeatsTable ImportBonusFeatsTable(string tablename, Guid guid)
        {
            var bfeatTable2da = customDataDict[tablename.ToLower()];

            var tmpBFeatTable = new BonusFeatsTable();
            tmpBFeatTable.ID = guid;
            tmpBFeatTable.Name = tablename.ToUpper();

            tmpBFeatTable.Clear();
            for (int i = 0; i < bfeatTable2da.Count; i++)
            {
                var tmpItem = new BonusFeatsTableItem();
                tmpItem.ParentTable = tmpBFeatTable;
                tmpItem.Level = i + 1;
                tmpItem.BonusFeatCount = bfeatTable2da[i].AsInteger("Bonus") ?? 0;
                tmpBFeatTable.Add(tmpItem);
            }

            _importCollection.BonusFeatTables.Add(tmpBFeatTable);

            return tmpBFeatTable;
        }

        private SkillsTable ImportSkillTable(string tablename, Guid guid)
        {
            var skillTable2da = customDataDict[tablename.ToLower()];

            var skillTable = new SkillsTable();
            skillTable.ID = guid;
            skillTable.Name = tablename.ToUpper();

            skillTable.Clear();
            for (int i = 0; i < skillTable2da.Count; i++)
            {
                var tmpItem = new SkillsTableItem();
                tmpItem.ParentTable = skillTable;
                tmpItem.Skill = CreateRef<Skill>(skillTable2da[i].AsInteger("SkillIndex"));
                tmpItem.IsClassSkill = skillTable2da[i].AsBoolean("ClassSkill");
                skillTable.Add(tmpItem);
            }

            _importCollection.SkillTables.Add(skillTable);

            return skillTable;
        }

        private SpellSlotTable ImportSpellSlotTable(string tablename, Guid guid)
        {
            var spellSlotTable2da = customDataDict[tablename.ToLower()];

            var spellSlotTable = new SpellSlotTable();
            spellSlotTable.ID = guid;
            spellSlotTable.Name = tablename.ToUpper();

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

            _importCollection.SpellSlotTables.Add(spellSlotTable);

            return spellSlotTable;
        }

        private KnownSpellsTable ImportKnownSpellsTable(string tablename, Guid guid)
        {
            var knownSpellsTable2da = customDataDict[tablename.ToLower()];

            var knownSpellsTable = new KnownSpellsTable();
            knownSpellsTable.ID = guid;
            knownSpellsTable.Name = tablename.ToUpper();

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

            _importCollection.KnownSpellsTables.Add(knownSpellsTable);

            return knownSpellsTable;
        }

        private PrerequisiteTable ImportPrerequisiteTable(string tablename, Guid guid)
        {
            var preRequTable2da = customDataDict[tablename.ToLower()];

            var preRequTable = new PrerequisiteTable();
            preRequTable.ID = guid;
            preRequTable.Name = tablename.ToUpper();

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
                        tmpItem.Param1Skill = CreateRef<Skill>(preRequTable2da[i].AsInteger("ReqParam1"));
                        break;

                    case RequirementType.VAR:
                        tmpItem.Param1String = preRequTable2da[i].AsString("ReqParam1");
                        break;

                    case RequirementType.ARCSPELL:
                    case RequirementType.SPELL:
                    case RequirementType.BAB:
                        tmpItem.Param1Int = preRequTable2da[i].AsInteger("ReqParam1");
                        break;

                    case RequirementType.SAVE:
                        tmpItem.Param1Save = preRequTable2da[i].AsInteger("ReqParam1");
                        break;
                }

                tmpItem.RequirementParam2 = preRequTable2da[i].AsInteger("ReqParam2");
                preRequTable.Add(tmpItem);
            }

            _importCollection.PrerequisiteTables.Add(preRequTable);

            return preRequTable;
        }

        private StatGainTable ImportStatGainTable(string tablename, Guid guid)
        {
            var statGainTable2da = customDataDict[tablename.ToLower()];

            var statGainTable = new StatGainTable();
            statGainTable.ID = guid;
            statGainTable.Name = tablename.ToUpper();

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

            _importCollection.StatGainTables.Add(statGainTable);

            return statGainTable;
        }

        private void ImportClasses()
        {
            if (customDataDict.TryGetValue("classes", out var classes2da))
            {
                customDataDict.Remove("classes");

                Log.Info("Importing 2DA: classes.2da");

                var originalClasses2da = LoadOriginal2da("classes");
                for (int i = 0; i < classes2da.Count; i++)
                {
                    if (!ImportRecord<CharacterClass>(i, classes2da, originalClasses2da, out var recordId)) continue;

                    var tmpClass = new CharacterClass();
                    tmpClass.ID = recordId;
                    tmpClass.Index = i;

                    if (!SetText(tmpClass.Name, classes2da[i].AsInteger("Name"))) continue;
                    SetText(tmpClass.Abbreviation, classes2da[i].AsInteger("Short", 0));
                    SetText(tmpClass.NamePlural, classes2da[i].AsInteger("Plural"));
                    SetText(tmpClass.Description, classes2da[i].AsInteger("Description"));

                    tmpClass.Icon = AddIconResource(classes2da[i].AsString("Icon"));
                    tmpClass.HitDie = classes2da[i].AsInteger("HitDie") ?? 0;
                    tmpClass.SkillPointsPerLevel = classes2da[i].AsInteger("SkillPointBase") ?? 0;
                    
                    tmpClass.AttackBonusTable = GetOrImportTable(classes2da[i].AsString("AttackBonusTable"), ImportAttackBonusTable);
                    tmpClass.Feats = GetOrImportTable(classes2da[i].AsString("FeatsTable"), ImportFeatsTable);
                    tmpClass.SavingThrows = GetOrImportTable(classes2da[i].AsString("SavingThrowTable"), ImportSavingThrowTable);
                    tmpClass.Skills = GetOrImportTable(classes2da[i].AsString("SkillsTable"), ImportSkillTable);
                    tmpClass.BonusFeats = GetOrImportTable(classes2da[i].AsString("BonusFeatsTable"), ImportBonusFeatsTable);
                    tmpClass.SpellSlots = GetOrImportTable(classes2da[i].AsString("SpellGainTable"), ImportSpellSlotTable);
                    tmpClass.KnownSpells = GetOrImportTable(classes2da[i].AsString("SpellKnownTable"), ImportKnownSpellsTable);

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

                    tmpClass.Requirements = GetOrImportTable(classes2da[i].AsString("PreReqTable"), ImportPrerequisiteTable);
                    tmpClass.MaxLevel = classes2da[i].AsInteger("MaxLevel") ?? 0;
                    tmpClass.MulticlassXPPenalty = classes2da[i].AsBoolean("XPPenalty");
                    tmpClass.ArcaneCasterLevelMod = classes2da[i].AsInteger("ArcSpellLvlMod") ?? 0;
                    tmpClass.DivineCasterLevelMod = classes2da[i].AsInteger("DivSpellLvlMod") ?? 0;
                    tmpClass.PreEpicMaxLevel = classes2da[i].AsInteger("EpicLevel") ?? 0;
                    tmpClass.DefaultPackage = CreateRef<ClassPackage>(classes2da[i].AsInteger("Package"));
                    tmpClass.StatGainTable = GetOrImportTable(classes2da[i].AsString("StatGainTable"), ImportStatGainTable);
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

                    //Spellbook
                    var spellbookName = classes2da[i].AsString("SpellTableColumn");
                    if (spellbookName != null)
                    {
                        var stdSpellbooks = (SpellbookRepository)MasterRepository.Standard.Spellbooks;
                        var prjSpellbooks = (SpellbookRepository)MasterRepository.Project.Spellbooks;

                        var originalModel = stdSpellbooks.GetByName(spellbookName);
                        var originalOverride = prjSpellbooks.GetOverride(originalModel);
                        var projSpellbook = prjSpellbooks.GetByName(spellbookName);
                        if ((originalModel == null) && (originalOverride == null) && (projSpellbook == null))
                        {
                            Spellbook tmpSpellbook = new Spellbook();
                            tmpSpellbook.ID = Guid.NewGuid();
                            tmpSpellbook.Name = spellbookName;
                            _importCollection.Spellbooks.Add(tmpSpellbook);

                            tmpClass.Spellbook = tmpSpellbook;
                        }
                        else
                            tmpClass.Spellbook = projSpellbook ?? (Spellbook?)originalOverride ?? originalModel;
                    }

                    tmpClass.CasterLevelMultiplier = classes2da[i].AsFloat("CLMultiplier") ?? 1.0;
                    tmpClass.MinCastingLevel = classes2da[i].AsInteger("MinCastingLevel") ?? 0;
                    tmpClass.MinAssociateLevel = classes2da[i].AsInteger("MinAssociateLevel") ?? 0;
                    tmpClass.CanCastSpontaneously = classes2da[i].AsBoolean("CanCastSpontaneously");
                    tmpClass.SkipSpellSelection = classes2da[i].AsBoolean("SkipSpellSelection", null);

                    _importCollection.Classes.Add(tmpClass);
                }
            }
        }

        private void ImportDomains()
        {
            if (customDataDict.TryGetValue("domains", out var domains2da))
            {
                customDataDict.Remove("domains");

                Log.Info("Importing 2DA: domains.2da");

                var originalDomains2da = LoadOriginal2da("domains");
                for (int i = 0; i < domains2da.Count; i++)
                {
                    if (!ImportRecord<Domain>(i, domains2da, originalDomains2da, out var recordId)) continue;

                    var tmpDomain = new Domain();
                    tmpDomain.ID = recordId;
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

                    _importCollection.Domains.Add(tmpDomain);
                }
            }
        }

        private void ImportSkills()
        {
            if (customDataDict.TryGetValue("skills", out var skills2da))
            {
                customDataDict.Remove("skills");

                Log.Info("Importing 2DA: skills.2da");

                var originalSkills2da = LoadOriginal2da("skills");
                for (int i = 0; i < skills2da.Count; i++)
                {
                    if (!ImportRecord<Skill>(i, skills2da, originalSkills2da, out var recordId)) continue;

                    var tmpSkill = new Skill();
                    tmpSkill.ID = recordId;
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

                    _importCollection.Skills.Add(tmpSkill);
                }
            }
        }

        private void ImportFeats()
        {
            if (customDataDict.TryGetValue("feat", out var feat2da))
            {
                customDataDict.Remove("feat");

                Log.Info("Importing 2DA: feat.2da");

                var originalFeat2da = LoadOriginal2da("feat");
                for (int i = 0; i < feat2da.Count; i++)
                {
                    if (!ImportRecord<Feat>(i, feat2da, originalFeat2da, out var recordId)) continue;

                    var tmpFeat = new Feat();
                    tmpFeat.ID = recordId;
                    tmpFeat.Index = i;

                    if (!SetText(tmpFeat.Name, feat2da[i].AsInteger("FEAT"))) continue; 
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

                    _importCollection.Feats.Add(tmpFeat);
                }
            }
        }

        private void ImportSpells()
        {
            if (customDataDict.TryGetValue("spells", out var spells2da))
            {
                customDataDict.Remove("spells");

                Log.Info("Importing 2DA: spells.2da");

                var spellbooksToOverride = new Dictionary<string, Spellbook>();

                var originalSpells2da = LoadOriginal2da("spells");
                for (int i = 0; i < spells2da.Count; i++)
                {
                    if (!ImportRecord<Spell>(i, spells2da, originalSpells2da, out var recordId)) continue;

                    var tmpSpell = new Spell();
                    tmpSpell.ID = recordId;
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
                    tmpSpell.TargetingFlags = (TargetFlag?)spells2da[i].AsInteger("TargetFlags", null) ?? (TargetFlag)0;

                    
                    for (int j = 0; j < spells2da.Columns.Count; j++)
                    {
                        // Entries for new Spellbooks: 
                        var spellbook = ((SpellbookRepository)_importCollection.Spellbooks).GetByName(spells2da.Columns[j]);
                        if (spellbook != null)
                        {
                            var level = spells2da[i].AsInteger(j);
                            if (level != null)
                                spellbook.AddSpell(level ?? 99, tmpSpell);
                        }

                        // Changes to standard spellbooks
                        if (_importOverrides)
                        {
                            var standardSpellbook = ((SpellbookRepository)MasterRepository.Standard.Spellbooks).GetByName(spells2da.Columns[j]);
                            if (standardSpellbook != null)
                            {
                                var projectOverrideSpellbook = (Spellbook?)MasterRepository.Project.Spellbooks.GetOverride(standardSpellbook);
                                if ((projectOverrideSpellbook == null) || (_replaceOverrides))
                                {
                                    var level = spells2da[i].AsInteger(j);
                                    int? originalLevel = null;
                                    if (i < originalSpells2da.Count)
                                        originalLevel = (projectOverrideSpellbook ?? standardSpellbook).GetSpellLevel(i);

                                    if (originalLevel != level)
                                    {
                                        if (!spellbooksToOverride.TryGetValue(spells2da.Columns[j].ToLower(), out var overrideSpellbook))
                                        {
                                            overrideSpellbook = (Spellbook?)standardSpellbook.Override();
                                            if (overrideSpellbook != null)
                                                spellbooksToOverride.Add(spells2da.Columns[j].ToLower(), overrideSpellbook);
                                        }

                                        if (overrideSpellbook != null)
                                        {
                                            if (originalLevel != null)
                                                overrideSpellbook.RemoveSpellById(originalLevel ?? -1, i);
                                            overrideSpellbook.AddSpell(level ?? 99, tmpSpell);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                    _importCollection.Spells.Add(tmpSpell);
                }

                foreach (var spellbook in spellbooksToOverride.Values)
                    _importCollection.Spellbooks.Add(spellbook);
            }
        }

        private void ImportDiseases()
        {
            if (customDataDict.TryGetValue("disease", out var disease2da))
            {
                customDataDict.Remove("disease");

                Log.Info("Importing 2DA: disease.2da");

                var originalDisease2da = LoadOriginal2da("disease");
                for (int i = 0; i < disease2da.Count; i++)
                {
                    if (!ImportRecord<Disease>(i, disease2da, originalDisease2da, out var recordId)) continue;

                    var tmpDisease = new Disease();
                    tmpDisease.ID = recordId;
                    tmpDisease.Index = i;

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

                    _importCollection.Diseases.Add(tmpDisease);
                }
            }
        }

        private void ImportPoisons()
        {
            if (customDataDict.TryGetValue("poison", out var poison2da))
            {
                customDataDict.Remove("poison");

                Log.Info("Importing 2DA: poison.2da");

                var originalPoison2da = LoadOriginal2da("poison");
                for (int i = 0; i < poison2da.Count; i++)
                {
                    if (!ImportRecord<Poison>(i, poison2da, originalPoison2da, out var recordId)) continue;

                    var tmpPoison = new Poison();
                    tmpPoison.ID = recordId;
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

                    _importCollection.Poisons.Add(tmpPoison);
                }
            }
        }

        private void ImportAreaOfEffects()
        {
            if (customDataDict.TryGetValue("vfx_persistent", out var aoe2da))
            {
                customDataDict.Remove("vfx_persistent");

                Log.Info("Importing 2DA: vfx_persistent.2da");

                var originalAoe2da = LoadOriginal2da("vfx_persistent");
                for (int i = 0; i < aoe2da.Count; i++)
                {
                    if (!ImportRecord<AreaEffect>(i, aoe2da, originalAoe2da, out var recordId)) continue;

                    var tmpAreaEffect = new AreaEffect();
                    tmpAreaEffect.ID = recordId;
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

                    _importCollection.AreaEffects.Add(tmpAreaEffect);
                }
            }
        }

        private void ImportMasterFeats()
        {
            if (customDataDict.TryGetValue("masterfeats", out var masterFeats2da))
            {
                customDataDict.Remove("masterfeats");

                Log.Info("Importing 2DA: masterfeats.2da");

                var originalMasterFeats2da = LoadOriginal2da("masterfeats");
                for (int i = 0; i < masterFeats2da.Count; i++)
                {
                    if (!ImportRecord<MasterFeat>(i, masterFeats2da, originalMasterFeats2da, out var recordId)) continue;

                    var tmpMasterFeat = new MasterFeat();
                    tmpMasterFeat.ID = recordId;
                    tmpMasterFeat.Index = i;

                    if (!SetText(tmpMasterFeat.Name, masterFeats2da[i].AsInteger("STRREF"))) continue;
                    SetText(tmpMasterFeat.Description, masterFeats2da[i].AsInteger("DESCRIPTION"));
                    tmpMasterFeat.Icon = AddIconResource(masterFeats2da[i].AsString("ICON"));

                    _importCollection.MasterFeats.Add(tmpMasterFeat);
                }
            }
        }

        private void ImportAppearances()
        {
            if (customDataDict.TryGetValue("appearance", out var appearance2da))
            {
                // customDataDict.Remove("appearance"); // TODO: uncomment when appearances are fully supported

                Log.Info("Importing 2DA: appearance.2da");

                var originalAppearance2da = LoadOriginal2da("appearance");
                for (int i = 0; i < appearance2da.Count; i++)
                {
                    if (!ImportRecord<Appearance>(i, appearance2da, originalAppearance2da, out var recordId)) continue;

                    var tmpAppearance = new Appearance();
                    tmpAppearance.ID = recordId;
                    tmpAppearance.Index = i;

                    if (!SetText(tmpAppearance.Name, appearance2da[i].AsInteger("STRING_REF"))) continue;

                    _importCollection.Appearances.Add(tmpAppearance);
                }
            }
        }

        private void ImportVisualEffects()
        {
            if (customDataDict.TryGetValue("visualeffects", out var vfx2da))
            {
                // customDataDict.Remove("visualeffects"); // TODO: uncomment when visualeffects are fully supported

                Log.Info("Importing 2DA: visualeffects.2da");

                var originalVfx2da = LoadOriginal2da("visualeffects");
                for (int i = 0; i < vfx2da.Count; i++)
                {
                    if (!ImportRecord<VisualEffect>(i, vfx2da, originalVfx2da, out var recordId)) continue;

                    var tmpVfx = new VisualEffect();
                    tmpVfx.ID = recordId;
                    tmpVfx.Index = i;

                    if (vfx2da[i].IsNull("Type_FD")) continue;
                    tmpVfx.Name = vfx2da[i].AsString("Label") ?? "";

                    _importCollection.VisualEffects.Add(tmpVfx);
                }
            }
        }

        private void ImportClassPackages()
        {
            if (customDataDict.TryGetValue("packages", out var packages2da))
            {
                // customDataDict.Remove("packages"); // TODO: uncomment when packages are fully supported

                Log.Info("Importing 2DA: packages.2da");

                var originalPackages2da = LoadOriginal2da("packages");
                for (int i = 0; i < packages2da.Count; i++)
                {
                    if (!ImportRecord<ClassPackage>(i, packages2da, originalPackages2da, out var recordId)) continue;

                    var tmpPackage = new ClassPackage();
                    tmpPackage.ID = recordId;
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

                    _importCollection.ClassPackages.Add(tmpPackage);
                }
            }
        }

        private void ImportSoundsets()
        {
            if (customDataDict.TryGetValue("soundset", out var soundset2da))
            {
                // customDataDict.Remove("soundset"); // TODO: uncomment when soundsets are fully supported

                Log.Info("Importing 2DA: soundset.2da");

                var originalSoundset2da = LoadOriginal2da("soundset");
                for (int i = 0; i < soundset2da.Count; i++)
                {
                    if (!ImportRecord<Soundset>(i, soundset2da, originalSoundset2da, out var recordId)) continue;

                    var tmpSoundset = new Soundset();
                    tmpSoundset.ID = recordId;
                    tmpSoundset.Index = i;

                    if (!SetText(tmpSoundset.Name, soundset2da[i].AsInteger("STRREF"))) continue;

                    tmpSoundset.Gender = !soundset2da[i].IsNull("GENDER") ? (Gender)Enum.ToObject(typeof(Gender), soundset2da[i].AsInteger("GENDER") ?? 0) : Gender.Male;
                    tmpSoundset.Type = !soundset2da[i].IsNull("TYPE") ? (SoundsetType)Enum.ToObject(typeof(SoundsetType), soundset2da[i].AsInteger("TYPE") ?? 0) : SoundsetType.Player;
                    tmpSoundset.SoundsetResource = soundset2da[i].AsString("RESREF") ?? "";

                    _importCollection.Soundsets.Add(tmpSoundset);
                }
            }
        }

        private void ImportPolymorphs()
        {
            if (customDataDict.TryGetValue("polymorph", out var polymorph2da))
            {
                customDataDict.Remove("polymorph");

                Log.Info("Importing 2DA: polymorph.2da");

                var originalPolymorph2da = LoadOriginal2da("polymorph");
                for (int i = 0; i < polymorph2da.Count; i++)
                {
                    if (!ImportRecord<Polymorph>(i, polymorph2da, originalPolymorph2da, out var recordId)) continue;

                    var tmpPolymorph = new Polymorph();
                    tmpPolymorph.ID = recordId;
                    tmpPolymorph.Index = i;

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

                    _importCollection.Polymorphs.Add(tmpPolymorph);
                }
            }
        }

        private void ImportPortraits()
        {
            if (customDataDict.TryGetValue("portraits", out var portraits2da))
            {
                //customDataDict.Remove("portraits"); // TODO: uncomment when portraits are fully supported

                Log.Info("Importing 2DA: portraits.2da");

                var originalPortraits2da = LoadOriginal2da("portraits");
                for (int i = 0; i < portraits2da.Count; i++)
                {
                    if (!ImportRecord<Portrait>(i, portraits2da, originalPortraits2da, out var recordId)) continue;

                    var tmpPortrait = new Portrait();
                    tmpPortrait.ID = recordId;
                    tmpPortrait.Index = i;

                    if (portraits2da[i].IsNull("BaseResRef")) continue;
                    tmpPortrait.ResRef = portraits2da[i].AsString("BaseResRef") ?? "";

                    _importCollection.Portraits.Add(tmpPortrait);
                }
            }
        }

        private T? SolveInstance<T>(T? instance) where T : BaseModel, new()
        {
            if (instance?.Index == null)
                return null;
            else
            {
                var importedMatch = (T?)_importCollection.GetByIndex(typeof(T), instance.Index ?? -1);
                if (importedMatch != null)
                    return importedMatch;

                var standardMatch = (T?)MasterRepository.Standard.GetByIndex(typeof(T), instance.Index ?? -1);
                var overrideMatch = (T?)MasterRepository.Project.GetOverride(standardMatch);

                return overrideMatch ?? standardMatch;
            }
        }

        private void ResolveDependencies()
        {
            Log.Info("Resolving object dependencies...");

            // Classes
            foreach (var cls in _importCollection.Classes)
            {
                if (cls == null) continue;
                cls.DefaultPackage = SolveInstance(cls.DefaultPackage);
            }

            // Races
            foreach (var race in _importCollection.Races)
            {
                if (race == null) continue;
                race.Appearance = SolveInstance(race.Appearance);
                race.FavoredClass = SolveInstance(race.FavoredClass);
                race.ToolsetDefaultClass = SolveInstance(race.ToolsetDefaultClass);
                race.FavoredEnemyFeat = SolveInstance(race.FavoredEnemyFeat);
            }

            // Domains
            foreach (var domain in _importCollection.Domains)
            {
                if (domain == null) continue;
                domain.Level0Spell = SolveInstance(domain.Level0Spell);
                domain.Level1Spell = SolveInstance(domain.Level1Spell);
                domain.Level2Spell = SolveInstance(domain.Level2Spell);
                domain.Level3Spell = SolveInstance(domain.Level3Spell);
                domain.Level4Spell = SolveInstance(domain.Level4Spell);
                domain.Level5Spell = SolveInstance(domain.Level5Spell);
                domain.Level6Spell = SolveInstance(domain.Level6Spell);
                domain.Level7Spell = SolveInstance(domain.Level7Spell);
                domain.Level8Spell = SolveInstance(domain.Level8Spell);
                domain.Level9Spell = SolveInstance(domain.Level9Spell);
                domain.GrantedFeat = SolveInstance(domain.GrantedFeat);
            }

            // Spells
            foreach (var spell in _importCollection.Spells)
            {
                if (spell == null) continue;
                spell.CounterSpell1 = SolveInstance(spell.CounterSpell1);
                spell.CounterSpell2 = SolveInstance(spell.CounterSpell2);
                spell.ParentSpell = SolveInstance(spell.ParentSpell);
                spell.SubSpell1 = SolveInstance(spell.SubSpell1);
                spell.SubSpell2 = SolveInstance(spell.SubSpell2);
                spell.SubSpell3 = SolveInstance(spell.SubSpell3);
                spell.SubSpell4 = SolveInstance(spell.SubSpell4);
                spell.SubSpell5 = SolveInstance(spell.SubSpell5);
                spell.SubSpell6 = SolveInstance(spell.SubSpell6);
                spell.SubSpell7 = SolveInstance(spell.SubSpell7);
                spell.SubSpell8 = SolveInstance(spell.SubSpell8);
            }

            // Feats
            foreach (var feat in _importCollection.Feats)
            {
                if (feat == null) continue;
                feat.OnUseEffect = SolveInstance(feat.OnUseEffect);
                feat.MasterFeat = SolveInstance(feat.MasterFeat);
                feat.RequiredFeat1 = SolveInstance(feat.RequiredFeat1);
                feat.RequiredFeat2 = SolveInstance(feat.RequiredFeat2);
                feat.RequiredFeatSelection1 = SolveInstance(feat.RequiredFeatSelection1);
                feat.RequiredFeatSelection2 = SolveInstance(feat.RequiredFeatSelection2);
                feat.RequiredFeatSelection3 = SolveInstance(feat.RequiredFeatSelection3);
                feat.RequiredFeatSelection4 = SolveInstance(feat.RequiredFeatSelection4);
                feat.RequiredFeatSelection5 = SolveInstance(feat.RequiredFeatSelection5);
                feat.RequiredSkill1 = SolveInstance(feat.RequiredSkill1);
                feat.RequiredSkill2 = SolveInstance(feat.RequiredSkill2);
                feat.SuccessorFeat = SolveInstance(feat.SuccessorFeat);
                feat.MinLevelClass = SolveInstance(feat.MinLevelClass);
            }

            // Polymorphs
            foreach (var polymorph in _importCollection.Polymorphs)
            {
                if (polymorph == null) continue;
                polymorph.Appearance = SolveInstance(polymorph.Appearance);
                polymorph.RacialType = SolveInstance(polymorph.RacialType);
                polymorph.Portrait = SolveInstance(polymorph.Portrait);
                //polymorph.Soundset = SolveInstance(polymorph.Soundset, Standard.Soundsets); // Unused will not be imported!
                polymorph.Spell1 = SolveInstance(polymorph.Spell1);
                polymorph.Spell2 = SolveInstance(polymorph.Spell2);
                polymorph.Spell3 = SolveInstance(polymorph.Spell3);
            }

            // FeatsTable
            foreach (var featTable in _importCollection.FeatTables)
            {
                if (featTable == null) continue;

                featTable.Items.Sort(p => p?.GrantedOnLevel == -1 ? int.MaxValue : p?.GrantedOnLevel);
                for (int i = 0; i < featTable.Count; i++)
                {
                    var item = featTable[i];
                    if (item == null) continue;
                    item.Feat = SolveInstance(item.Feat);
                }
            }

            // SkillsTable
            foreach (var skillTable in _importCollection.SkillTables)
            {
                if (skillTable == null) continue;
                for (int i = 0; i < skillTable.Count; i++)
                {
                    var item = skillTable[i];
                    if (item == null) continue;
                    item.Skill = SolveInstance(item.Skill);
                }
            }

            // Prerequisites Table
            foreach (var preRequTable in _importCollection.PrerequisiteTables)
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
                            item.Param1Class = SolveInstance(item.Param1Class);
                            break;

                        case RequirementType.FEAT:
                        case RequirementType.FEATOR:
                            item.Param1Feat = SolveInstance(item.Param1Feat);
                            break;

                        case RequirementType.RACE:
                            item.Param1Race = SolveInstance(item.Param1Race);
                            break;

                        case RequirementType.SKILL:
                            item.Param1Skill = SolveInstance(item.Param1Skill);
                            break;
                    }
                }
            }

            // Racial Feats Table
            foreach (var racialFeatsTable in _importCollection.RacialFeatsTables)
            {
                if (racialFeatsTable == null) continue;
                for (int i = 0; i < racialFeatsTable.Count; i++)
                {
                    var item = racialFeatsTable[i];
                    if (item == null) continue;
                    item.Feat = SolveInstance(item.Feat);
                }
            }

            // Class Packages
            foreach (var package in _importCollection.ClassPackages)
            {
                if (package == null) continue;
                package.ForClass = SolveInstance(package.ForClass);
                package.Domain1 = SolveInstance(package.Domain1);
                package.Domain2 = SolveInstance(package.Domain2);
            }

            // Area Effects
            foreach (var aoe in _importCollection.AreaEffects)
            {
                if (aoe == null) continue;
                aoe.VisualEffect = SolveInstance(aoe.VisualEffect);
            }
        }

        private void TransferImportedData()
        {
            Log.Info("Transfering imported data into project...");

            // Races
            foreach (var race in _importCollection.Races)
            {
                if (race == null) continue;

                var standardRace = MasterRepository.Standard.Races.GetByID(race.ID);
                if (standardRace != null)
                {
                    race.ID = Guid.Empty;
                    race.Overrides = standardRace.ID;

                    var projectOverrideRace = MasterRepository.Project.GetOverride(standardRace);
                    if (projectOverrideRace != null)
                        MasterRepository.Project.Races.Remove(projectOverrideRace);
                }

                MasterRepository.Project.Races.Add(race);
            }

            // Classes
            foreach (var cls in _importCollection.Classes)
            {
                if (cls == null) continue;

                var standardClass = MasterRepository.Standard.Classes.GetByID(cls.ID);
                if (standardClass != null)
                {
                    cls.ID = Guid.Empty;
                    cls.Overrides = standardClass.ID;

                    var projectOverrideClass = MasterRepository.Project.GetOverride(standardClass);
                    if (projectOverrideClass != null)
                        MasterRepository.Project.Classes.Remove(projectOverrideClass);
                }

                MasterRepository.Project.Classes.Add(cls);
            }

            // Class Packages
            foreach (var package in _importCollection.ClassPackages)
            {
                if (package == null) continue;

                var standardPackage = MasterRepository.Standard.ClassPackages.GetByID(package.ID);
                if (standardPackage != null)
                {
                    package.ID = Guid.Empty;
                    package.Overrides = standardPackage.ID;

                    var projectOverridePackage = MasterRepository.Project.GetOverride(standardPackage);
                    if (projectOverridePackage != null)
                        MasterRepository.Project.ClassPackages.Remove(projectOverridePackage);
                }

                MasterRepository.Project.ClassPackages.Add(package);
            }

            // Domains
            foreach (var domain in _importCollection.Domains)
            {
                if (domain == null) continue;

                var standardDomain = MasterRepository.Standard.Domains.GetByID(domain.ID);
                if (standardDomain != null)
                {
                    domain.ID = Guid.Empty;
                    domain.Overrides = standardDomain.ID;

                    var projectOverrideDomain = MasterRepository.Project.GetOverride(standardDomain);
                    if (projectOverrideDomain != null)
                        MasterRepository.Project.Domains.Remove(projectOverrideDomain);
                }

                MasterRepository.Project.Domains.Add(domain);
            }

            // Spellbooks
            foreach (var spellbook in _importCollection.Spellbooks)
            {
                if (spellbook == null) continue;

                var standardSpellbook = MasterRepository.Standard.Spellbooks.GetByID(spellbook.ID);
                if (standardSpellbook != null)
                {
                    var projectOverrideSpellbook = MasterRepository.Project.GetOverride(standardSpellbook);
                    if (projectOverrideSpellbook != null)
                        MasterRepository.Project.Spellbooks.Remove(projectOverrideSpellbook);
                }

                MasterRepository.Project.Spellbooks.Add(spellbook);
            }

            // Spells
            foreach (var spell in _importCollection.Spells)
            {
                if (spell == null) continue;

                var standardSpell = MasterRepository.Standard.Spells.GetByID(spell.ID);
                if (standardSpell != null)
                {
                    spell.ID = Guid.Empty;
                    spell.Overrides = standardSpell.ID;

                    var projectOverrideSpell = MasterRepository.Project.GetOverride(standardSpell);
                    if (projectOverrideSpell != null)
                        MasterRepository.Project.Spells.Remove(projectOverrideSpell);
                }

                MasterRepository.Project.Spells.Add(spell);
            }

            // Master Feats
            foreach (var masterFeat in _importCollection.MasterFeats)
            {
                if (masterFeat == null) continue;

                var standardMasterFeat = MasterRepository.Standard.MasterFeats.GetByID(masterFeat.ID);
                if (standardMasterFeat != null)
                {
                    masterFeat.ID = Guid.Empty;
                    masterFeat.Overrides = standardMasterFeat.ID;

                    var projectOverrideMasterFeat = MasterRepository.Project.GetOverride(standardMasterFeat);
                    if (projectOverrideMasterFeat != null)
                        MasterRepository.Project.MasterFeats.Remove(projectOverrideMasterFeat);
                }

                MasterRepository.Project.MasterFeats.Add(masterFeat);
            }

            // Feats
            foreach (var feat in _importCollection.Feats)
            {
                if (feat == null) continue;

                var standardFeat = MasterRepository.Standard.Feats.GetByID(feat.ID);
                if (standardFeat != null)
                {
                    feat.ID = Guid.Empty;
                    feat.Overrides = standardFeat.ID;

                    var projectOverrideFeat = MasterRepository.Project.GetOverride(standardFeat);
                    if (projectOverrideFeat != null)
                        MasterRepository.Project.Feats.Remove(projectOverrideFeat);
                }

                MasterRepository.Project.Feats.Add(feat);
            }

            // Skills
            foreach (var skill in _importCollection.Skills)
            {
                if (skill == null) continue;

                var standardSkill = MasterRepository.Standard.Skills.GetByID(skill.ID);
                if (standardSkill != null)
                {
                    skill.ID = Guid.Empty;
                    skill.Overrides = standardSkill.ID;

                    var projectOverrideSkill = MasterRepository.Project.GetOverride(standardSkill);
                    if (projectOverrideSkill != null)
                        MasterRepository.Project.Skills.Remove(projectOverrideSkill);
                }

                MasterRepository.Project.Skills.Add(skill);
            }

            // Diseases
            foreach (var disease in _importCollection.Diseases)
            {
                if (disease == null) continue;

                var standardDisease = MasterRepository.Standard.Diseases.GetByID(disease.ID);
                if (standardDisease != null)
                {
                    disease.ID = Guid.Empty;
                    disease.Overrides = standardDisease.ID;

                    var projectOverrideDisease = MasterRepository.Project.GetOverride(standardDisease);
                    if (projectOverrideDisease != null)
                        MasterRepository.Project.Diseases.Remove(projectOverrideDisease);
                }

                MasterRepository.Project.Diseases.Add(disease);
            }

            // Poisons
            foreach (var poison in _importCollection.Poisons)
            {
                if (poison == null) continue;

                var standardPoison = MasterRepository.Standard.Poisons.GetByID(poison.ID);
                if (standardPoison != null)
                {
                    poison.ID = Guid.Empty;
                    poison.Overrides = standardPoison.ID;

                    var projectOverridePoison = MasterRepository.Project.GetOverride(standardPoison);
                    if (projectOverridePoison != null)
                        MasterRepository.Project.Poisons.Remove(projectOverridePoison);
                }

                MasterRepository.Project.Poisons.Add(poison);
            }

            // Area Effects
            foreach (var aoe in _importCollection.AreaEffects)
            {
                if (aoe == null) continue;

                var standardAoe = MasterRepository.Standard.AreaEffects.GetByID(aoe.ID);
                if (standardAoe != null)
                {
                    aoe.ID = Guid.Empty;
                    aoe.Overrides = standardAoe.ID;

                    var projectOverrideAoe = MasterRepository.Project.GetOverride(standardAoe);
                    if (projectOverrideAoe != null)
                        MasterRepository.Project.AreaEffects.Remove(projectOverrideAoe);
                }

                MasterRepository.Project.AreaEffects.Add(aoe);
            }

            // Polymorphs
            foreach (var polymorph in _importCollection.Polymorphs)
            {
                if (polymorph == null) continue;

                var standardPolymorph = MasterRepository.Standard.Polymorphs.GetByID(polymorph.ID);
                if (standardPolymorph != null)
                {
                    polymorph.ID = Guid.Empty;
                    polymorph.Overrides = standardPolymorph.ID;

                    var projectOverridePolymorph = MasterRepository.Project.GetOverride(standardPolymorph);
                    if (projectOverridePolymorph != null)
                        MasterRepository.Project.Polymorphs.Remove(projectOverridePolymorph);
                }

                MasterRepository.Project.Polymorphs.Add(polymorph);
            }

            // Soundsets
            foreach (var soundset in _importCollection.Soundsets)
            {
                if (soundset == null) continue;

                var standardSoundset = MasterRepository.Standard.Soundsets.GetByID(soundset.ID);
                if (standardSoundset != null)
                {
                    soundset.ID = Guid.Empty;
                    soundset.Overrides = standardSoundset.ID;

                    var projectOverrideSoundset = MasterRepository.Project.GetOverride(standardSoundset);
                    if (projectOverrideSoundset != null)
                        MasterRepository.Project.Soundsets.Remove(projectOverrideSoundset);
                }

                MasterRepository.Project.Soundsets.Add(soundset);
            }

            // Visual Effects
            foreach (var vfx in _importCollection.VisualEffects)
            {
                if (vfx == null) continue;

                var standardVfx = MasterRepository.Standard.VisualEffects.GetByID(vfx.ID);
                if (standardVfx != null)
                {
                    vfx.ID = Guid.Empty;
                    vfx.Overrides = standardVfx.ID;

                    var projectOverrideVfx = MasterRepository.Project.GetOverride(standardVfx);
                    if (projectOverrideVfx != null)
                        MasterRepository.Project.VisualEffects.Remove(projectOverrideVfx);
                }

                MasterRepository.Project.VisualEffects.Add(vfx);
            }

            // Appearances
            foreach (var appearance in _importCollection.Appearances)
            {
                if (appearance == null) continue;

                var standardAppearance = MasterRepository.Standard.Appearances.GetByID(appearance.ID);
                if (standardAppearance != null)
                {
                    appearance.ID = Guid.Empty;
                    appearance.Overrides = standardAppearance.ID;

                    var projectOverrideAppearance = MasterRepository.Project.GetOverride(standardAppearance);
                    if (projectOverrideAppearance != null)
                        MasterRepository.Project.Appearances.Remove(projectOverrideAppearance);
                }

                MasterRepository.Project.Appearances.Add(appearance);
            }

            // Portraits
            foreach (var portrait in _importCollection.Portraits)
            {
                if (portrait == null) continue;

                var standardPortrait = MasterRepository.Standard.Portraits.GetByID(portrait.ID);
                if (standardPortrait != null)
                {
                    portrait.ID = Guid.Empty;
                    portrait.Overrides = standardPortrait.ID;

                    var projectOverridePortrait = MasterRepository.Project.GetOverride(standardPortrait);
                    if (projectOverridePortrait != null)
                        MasterRepository.Project.Portraits.Remove(projectOverridePortrait);
                }

                MasterRepository.Project.Portraits.Add(portrait);
            }

            // Racial Feats
            foreach (var racialFeatsTable in _importCollection.RacialFeatsTables)
            {
                if (racialFeatsTable == null) continue;

                var standardRacialFeatsTable = MasterRepository.Standard.RacialFeatsTables.GetByID(racialFeatsTable.ID);
                if (standardRacialFeatsTable != null)
                {
                    racialFeatsTable.ID = Guid.Empty;
                    racialFeatsTable.Overrides = standardRacialFeatsTable.ID;

                    var projectOverrideRacialFeatsTable = MasterRepository.Project.GetOverride(standardRacialFeatsTable);
                    if (projectOverrideRacialFeatsTable != null)
                        MasterRepository.Project.RacialFeatsTables.Remove(projectOverrideRacialFeatsTable);
                }

                MasterRepository.Project.RacialFeatsTables.Add(racialFeatsTable);
            }

            // Class Feats
            foreach (var featsTable in _importCollection.FeatTables)
            {
                if (featsTable == null) continue;

                var standardFeatsTable = MasterRepository.Standard.FeatTables.GetByID(featsTable.ID);
                if (standardFeatsTable != null)
                {
                    featsTable.ID = Guid.Empty;
                    featsTable.Overrides = standardFeatsTable.ID;

                    var projectOverrideFeatsTable = MasterRepository.Project.GetOverride(standardFeatsTable);
                    if (projectOverrideFeatsTable != null)
                        MasterRepository.Project.FeatTables.Remove(projectOverrideFeatsTable);
                }

                MasterRepository.Project.FeatTables.Add(featsTable);
            }

            // Class Bonus Feats
            foreach (var bfeatsTable in _importCollection.BonusFeatTables)
            {
                if (bfeatsTable == null) continue;

                var standardBFeatsTable = MasterRepository.Standard.BonusFeatTables.GetByID(bfeatsTable.ID);
                if (standardBFeatsTable != null)
                {
                    bfeatsTable.ID = Guid.Empty;
                    bfeatsTable.Overrides = standardBFeatsTable.ID;

                    var projectOverrideBFeatsTable = MasterRepository.Project.GetOverride(standardBFeatsTable);
                    if (projectOverrideBFeatsTable != null)
                        MasterRepository.Project.BonusFeatTables.Remove(projectOverrideBFeatsTable);
                }

                MasterRepository.Project.BonusFeatTables.Add(bfeatsTable);
            }

            // Class Skills
            foreach (var skillTable in _importCollection.SkillTables)
            {
                if (skillTable == null) continue;

                var standardSkillTable = MasterRepository.Standard.SkillTables.GetByID(skillTable.ID);
                if (standardSkillTable != null)
                {
                    skillTable.ID = Guid.Empty;
                    skillTable.Overrides = standardSkillTable.ID;

                    var projectOverrideSkillTable = MasterRepository.Project.GetOverride(standardSkillTable);
                    if (projectOverrideSkillTable != null)
                        MasterRepository.Project.SkillTables.Remove(projectOverrideSkillTable);
                }

                MasterRepository.Project.SkillTables.Add(skillTable);
            }

            // Class BAB Tables
            foreach (var attackTable in _importCollection.AttackBonusTables)
            {
                if (attackTable == null) continue;

                var standardAttackTable = MasterRepository.Standard.AttackBonusTables.GetByID(attackTable.ID);
                if (standardAttackTable != null)
                {
                    attackTable.ID = Guid.Empty;
                    attackTable.Overrides = standardAttackTable.ID;

                    var projectOverrideAttackTable = MasterRepository.Project.GetOverride(standardAttackTable);
                    if (projectOverrideAttackTable != null)
                        MasterRepository.Project.AttackBonusTables.Remove(projectOverrideAttackTable);
                }

                MasterRepository.Project.AttackBonusTables.Add(attackTable);
            }

            // Class Savingthrows
            foreach (var savesTable in _importCollection.SavingThrowTables)
            {
                if (savesTable == null) continue;

                var standardSavesTable = MasterRepository.Standard.SavingThrowTables.GetByID(savesTable.ID);
                if (standardSavesTable != null)
                {
                    savesTable.ID = Guid.Empty;
                    savesTable.Overrides = standardSavesTable.ID;

                    var projectOverrideSavesTable = MasterRepository.Project.GetOverride(standardSavesTable);
                    if (projectOverrideSavesTable != null)
                        MasterRepository.Project.SavingThrowTables.Remove(projectOverrideSavesTable);
                }

                MasterRepository.Project.SavingThrowTables.Add(savesTable);
            }

            // Class Prerequisites
            foreach (var requTable in _importCollection.PrerequisiteTables)
            {
                if (requTable == null) continue;

                var standardRequTable = MasterRepository.Standard.PrerequisiteTables.GetByID(requTable.ID);
                if (standardRequTable != null)
                {
                    requTable.ID = Guid.Empty;
                    requTable.Overrides = standardRequTable.ID;

                    var projectOverrideRequTable = MasterRepository.Project.GetOverride(standardRequTable);
                    if (projectOverrideRequTable != null)
                        MasterRepository.Project.PrerequisiteTables.Remove(projectOverrideRequTable);
                }

                MasterRepository.Project.PrerequisiteTables.Add(requTable);
            }

            // Class Statgain Tables
            foreach (var statGainTable in _importCollection.StatGainTables)
            {
                if (statGainTable == null) continue;

                var standardStatGainTable = MasterRepository.Standard.StatGainTables.GetByID(statGainTable.ID);
                if (standardStatGainTable != null)
                {
                    statGainTable.ID = Guid.Empty;
                    statGainTable.Overrides = standardStatGainTable.ID;

                    var projectOverrideStatGainTable = MasterRepository.Project.GetOverride(standardStatGainTable);
                    if (projectOverrideStatGainTable != null)
                        MasterRepository.Project.StatGainTables.Remove(projectOverrideStatGainTable);
                }

                MasterRepository.Project.StatGainTables.Add(statGainTable);
            }

            // Class Spell Slots
            foreach (var spellSlotTable in _importCollection.SpellSlotTables)
            {
                if (spellSlotTable == null) continue;

                var standardSpellSlotTable = MasterRepository.Standard.SpellSlotTables.GetByID(spellSlotTable.ID);
                if (standardSpellSlotTable != null)
                {
                    spellSlotTable.ID = Guid.Empty;
                    spellSlotTable.Overrides = standardSpellSlotTable.ID;

                    var projectOverrideSpellSlotTable = MasterRepository.Project.GetOverride(standardSpellSlotTable);
                    if (projectOverrideSpellSlotTable != null)
                        MasterRepository.Project.SpellSlotTables.Remove(projectOverrideSpellSlotTable);
                }

                MasterRepository.Project.SpellSlotTables.Add(spellSlotTable);
            }

            // Class Known Spells
            foreach (var knownSpellsTable in _importCollection.KnownSpellsTables)
            {
                if (knownSpellsTable == null) continue;

                var standardKnownSpellsTable = MasterRepository.Standard.KnownSpellsTables.GetByID(knownSpellsTable.ID);
                if (standardKnownSpellsTable != null)
                {
                    knownSpellsTable.ID = Guid.Empty;
                    knownSpellsTable.Overrides = standardKnownSpellsTable.ID;

                    var projectOverrideKnownSpellsTable = MasterRepository.Project.GetOverride(standardKnownSpellsTable);
                    if (projectOverrideKnownSpellsTable != null)
                        MasterRepository.Project.KnownSpellsTables.Remove(projectOverrideKnownSpellsTable);
                }

                MasterRepository.Project.KnownSpellsTables.Add(knownSpellsTable);
            }
        }

        private void ImportExternalData()
        {
            Log.Info("Importing external data...");

            foreach (var file in _importFiles)
            {
                var extension = Path.GetExtension(file).ToLower();
                if ((extension == ".hak") || (extension == ".erf"))
                {
                    var hak = new ErfFile();
                    hak.Load(file);
                    try
                    {
                        for (int i = 0; i < hak.Count; i++)
                        {
                            if ((hak[i].ResourceType != NWNResourceType.TWODA) || (customDataDict.ContainsKey(hak[i].ResRef.ToLower())))
                                ImportToExternalPath(hak[i], hak);
                        }
                    }
                    finally
                    {
                        hak.Close();
                    }
                }
            }
        }

        public void DoImport()
        {
            Log.Info("Importing data...");
            try
            {
                gameDataTlk.Load(EosConfig.NwnBasePath);
                gameDataBif.Load(EosConfig.NwnBasePath);

                foreach (var file in _importFiles)
                {
                    var extension = Path.GetExtension(file).ToLower();
                    if ((extension == ".hak") || (extension == ".erf"))
                        ImportHak(file);
                    else if (extension == ".2da")
                        Import2da(file);
                }

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

                ImportAppearances();
                ImportVisualEffects();
                ImportClassPackages();
                ImportSoundsets();
                ImportPolymorphs();
                ImportPortraits();

                ImportText();

                var continueImport = true;
                ImportPreview?.Invoke(this, _importCollection, ref continueImport);

                if (continueImport)
                {
                    ResolveDependencies();
                    TransferImportedData();
                    ImportExternalData();

                    MasterRepository.Resources.LoadExternalResources(MasterRepository.Project.Settings.ExternalFolders);
                }
            }
            catch(Exception e)
            {
                Log.Error(e.Message);
                throw;
            }

            Log.Info("Importing successful!");
        }
    }
}
