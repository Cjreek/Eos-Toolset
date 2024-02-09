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
using System.Reflection.Emit;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
        private Dictionary<String, SsfFile> ssfDict = new Dictionary<string, SsfFile>();

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
                    else if (hak[i].ResourceType == NWNResourceType.SSF)
                    {
                        var ssf = new SsfFile();
                        ssf.Load(hak.Read(hak[i].ResRef, hak[i].ResourceType));
                        ssfDict[hak[i].ResRef.ToLower()] = ssf;
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
                tmpItem.SourceLabel = racialFeatTable2da[i].AsString("FeatLabel") ?? "";
                tmpItem.Feat = CreateRef<Feat>(racialFeatTable2da[i].AsInteger("FeatIndex"));
                tmpRacialFeatsTable.Add(tmpItem);
            }

            _importCollection.RacialFeatsTables.Add(tmpRacialFeatsTable);

            return tmpRacialFeatsTable;
        }

        private bool ImportRecord<T>(int index, TwoDimensionalArrayFile import2DA, TwoDimensionalArrayFile origina2DA, out Guid recordId) where T : BaseModel
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
                    tmpRace.Index = i - originalRaces2da.Count; 
                    tmpRace.SourceLabel = races2da[i].AsString("Label");

                    if (!SetText(tmpRace.Name, races2da[i].AsInteger("Name"))) continue;
                    SetText(tmpRace.NamePlural, races2da[i].AsInteger("NamePlural"));
                    SetText(tmpRace.Adjective, races2da[i].AsInteger("ConverName"));
                    SetText(tmpRace.Description, races2da[i].AsInteger("Description"));
                    SetText(tmpRace.Biography, races2da[i].AsInteger("Biography"));

                    tmpRace.Icon = AddIconResource(races2da[i].AsString("Icon", ""));
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

                    tmpRace.NameGenTableA = races2da[i].AsString("NameGenTableA", "");
                    tmpRace.NameGenTableB = races2da[i].AsString("NameGenTableB", "");

                    tmpRace.FirstLevelExtraFeats = races2da[i].AsInteger("ExtraFeatsAtFirstLevel", 0) ?? 0;
                    tmpRace.ExtraSkillPointsPerLevel = races2da[i].AsInteger("ExtraSkillPointsPerLevel", 0) ?? 0;
                    tmpRace.FirstLevelSkillPointsMultiplier = races2da[i].AsInteger("FirstLevelSkillPointsMultiplier", 4);
                    tmpRace.FirstLevelAbilityPoints = races2da[i].AsInteger("AbilitiesPointBuyNumber", 30);
                    tmpRace.FeatEveryNthLevel = races2da[i].AsInteger("NormalFeatEveryNthLevel", 3);
                    tmpRace.FeatEveryNthLevelCount = races2da[i].AsInteger("NumberNormalFeatsEveryNthLevel", 1);
                    tmpRace.SkillPointModifierAbility = Enum.Parse<AbilityType>(races2da[i].AsString("SkillPointModifierAbility", "INT") ?? "INT", true);
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
                tmpItem.SourceLabel = featTable2da[i].AsString("FeatLabel") ?? "";
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
                tmpItem.SourceLabel = skillTable2da[i].AsString("SkillLabel") ?? "";
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
                if (preRequTable2da[i].IsNull("ReqType")) continue;

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
                    tmpClass.Index = i - originalClasses2da.Count;
                    tmpClass.SourceLabel = classes2da[i].AsString("Label");

                    if (!SetText(tmpClass.Name, classes2da[i].AsInteger("Name"))) continue;
                    SetText(tmpClass.Abbreviation, classes2da[i].AsInteger("Short", null));
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
                    tmpDomain.Index = i - originalDomains2da.Count;
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
                    tmpSkill.Index = i - originalSkills2da.Count;
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
                    tmpFeat.Index = i - originalFeat2da.Count;
                    tmpFeat.SourceLabel = feat2da[i].AsString("LABEL");

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
                    tmpSpell.Index = i - originalSpells2da.Count;
                    tmpSpell.SourceLabel = spells2da[i].AsString("Label");

                    if (!SetText(tmpSpell.Name, spells2da[i].AsInteger("Name"))) continue;
                    SetText(tmpSpell.Description, spells2da[i].AsInteger("SpellDesc"));
                    SetText(tmpSpell.AlternativeCastMessage, spells2da[i].AsInteger("AltMessage"));

                    tmpSpell.Icon = AddIconResource(spells2da[i].AsString("IconResRef"));
                    tmpSpell.School = !spells2da[i].IsNull("School") ? Enum.Parse<SpellSchool>(spells2da[i].AsString("School") ?? "", true) : SpellSchool.G;
                    tmpSpell.Range = !spells2da[i].IsNull("Range") ? Enum.Parse<SpellRange>(spells2da[i].AsString("Range") ?? "", true) : SpellRange.S;

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
                    tmpDisease.Index = i - originalDisease2da.Count;
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
                    tmpPoison.Index = i - originalPoison2da.Count;
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
                    tmpAreaEffect.Index = i - originalAoe2da.Count;
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
                    tmpMasterFeat.Index = i - originalMasterFeats2da.Count;
                    tmpMasterFeat.SourceLabel = masterFeats2da[i].AsString("LABEL");

                    if (!SetText(tmpMasterFeat.Name, masterFeats2da[i].AsInteger("STRREF"))) continue;
                    SetText(tmpMasterFeat.Description, masterFeats2da[i].AsInteger("DESCRIPTION"));
                    tmpMasterFeat.Icon = AddIconResource(masterFeats2da[i].AsString("ICON"));

                    _importCollection.MasterFeats.Add(tmpMasterFeat);
                }
            }
        }

        private void ImportBaseItems()
        {
            if (customDataDict.TryGetValue("baseitems", out var baseitems2da))
            {
                customDataDict.Remove("baseitems");

                Log.Info("Importing 2DA: baseitems.2da");

                var originalBaseitems2da = LoadOriginal2da("baseitems");
                for (int i = 0; i < baseitems2da.Count; i++)
                {
                    if (!ImportRecord<BaseItem>(i, baseitems2da, originalBaseitems2da, out var recordId)) continue;

                    var tmpBaseItem = new BaseItem();
                    tmpBaseItem.ID = recordId;
                    tmpBaseItem.Index = i - originalBaseitems2da.Count;
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
                    tmpBaseItem.WeaponFocusFeat = CreateRef<Feat>(baseitems2da[i].AsInteger("WeaponFocusFeat", null));
                    tmpBaseItem.EpicWeaponFocusFeat = CreateRef<Feat>(baseitems2da[i].AsInteger("EpicWeaponFocusFeat", null));
                    tmpBaseItem.WeaponSpecializationFeat = CreateRef<Feat>(baseitems2da[i].AsInteger("WeaponSpecializationFeat", null));
                    tmpBaseItem.EpicWeaponSpecializationFeat = CreateRef<Feat>(baseitems2da[i].AsInteger("EpicWeaponSpecializationFeat", null));
                    tmpBaseItem.ImprovedCriticalFeat = CreateRef<Feat>(baseitems2da[i].AsInteger("WeaponImprovedCriticalFeat", null));
                    tmpBaseItem.OverwhelmingCriticalFeat = CreateRef<Feat>(baseitems2da[i].AsInteger("EpicWeaponOverwhelmingCriticalFeat", null));
                    tmpBaseItem.DevastatingCriticalFeat = CreateRef<Feat>(baseitems2da[i].AsInteger("EpicWeaponDevastatingCriticalFeat", null));
                    tmpBaseItem.WeaponOfChoiceFeat = CreateRef<Feat>(baseitems2da[i].AsInteger("WeaponOfChoiceFeat", null));
                    tmpBaseItem.IsMonkWeapon = baseitems2da[i].AsBoolean("IsMonkWeapon");
                    tmpBaseItem.WeaponFinesseMinimumCreatureSize = baseitems2da[i].IsNull("WeaponFinesseMinimumCreatureSize", false) ? null : (SizeCategory)Enum.ToObject(typeof(SizeCategory), baseitems2da[i].AsInteger("WeaponFinesseMinimumCreatureSize") ?? 0);

                    _importCollection.BaseItems.Add(tmpBaseItem);
                }
            }
        }

        private void ImportAppearances()
        {
            if (customDataDict.TryGetValue("appearance", out var appearance2da))
            {
                customDataDict.Remove("appearance");

                Log.Info("Importing 2DA: appearance.2da");

                var originalAppearance2da = LoadOriginal2da("appearance");
                for (int i = 0; i < appearance2da.Count; i++)
                {
                    if (!ImportRecord<Appearance>(i, appearance2da, originalAppearance2da, out var recordId)) continue;

                    var tmpAppearance = new Appearance();
                    tmpAppearance.ID = recordId;
                    tmpAppearance.Index = i - originalAppearance2da.Count;
                    tmpAppearance.SourceLabel = appearance2da[i].AsString("LABEL");

                    if (!SetText(tmpAppearance.Name, appearance2da[i].AsInteger("STRING_REF"))) continue;
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

                    _importCollection.Appearances.Add(tmpAppearance);
                }
            }
        }

        private void ImportAppearanceSoundsets()
        {
            if (customDataDict.TryGetValue("appearancesndset", out var appearancesndset2da))
            {
                customDataDict.Remove("appearancesndset");

                Log.Info("Importing 2DA: appearancesndset.2da");

                var originalAppearanceSoundset2da = LoadOriginal2da("appearancesndset");
                for (int i = 0; i < appearancesndset2da.Count; i++)
                {
                    if (!ImportRecord<AppearanceSoundset>(i, appearancesndset2da, originalAppearanceSoundset2da, out var recordId)) continue;

                    var tmpAppearanceSoundset = new AppearanceSoundset();
                    tmpAppearanceSoundset.ID = recordId;
                    tmpAppearanceSoundset.Index = i - originalAppearanceSoundset2da.Count;
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

                    _importCollection.AppearanceSoundsets.Add(tmpAppearanceSoundset);
                }
            }
        }

        private void ImportWeaponSounds()
        {
            if (customDataDict.TryGetValue("weaponsounds", out var weaponsounds2da))
            {
                customDataDict.Remove("weaponsounds");

                Log.Info("Importing 2DA: weaponsounds.2da");

                var originalWeaponSounds2da = LoadOriginal2da("weaponsounds");
                for (int i = 0; i < weaponsounds2da.Count; i++)
                {
                    if (!ImportRecord<WeaponSound>(i, weaponsounds2da, originalWeaponSounds2da, out var recordId)) continue;

                    var tmpWeaponSound = new WeaponSound();
                    tmpWeaponSound.ID = recordId;
                    tmpWeaponSound.Index = i - originalWeaponSounds2da.Count;
                    tmpWeaponSound.SourceLabel = weaponsounds2da[i].AsString("Label");

                    if (weaponsounds2da[i].IsNull("Label")) continue;
                    tmpWeaponSound.Name = weaponsounds2da[i].AsString("Label") ?? "";
                    tmpWeaponSound.Leather0 = weaponsounds2da[i].AsString("Leather0", null);
                    tmpWeaponSound.Leather1 = weaponsounds2da[i].AsString("Leather1", null);
                    tmpWeaponSound.Chain0 = weaponsounds2da[i].AsString("Chain0", null);
                    tmpWeaponSound.Chain1 = weaponsounds2da[i].AsString("Chain1", null);
                    tmpWeaponSound.Plate0 = weaponsounds2da[i].AsString("Plate0", null);
                    tmpWeaponSound.Plate1 = weaponsounds2da[i].AsString("Plate1", null);
                    tmpWeaponSound.Stone0 = weaponsounds2da[i].AsString("Stone0", null);
                    tmpWeaponSound.Stone1 = weaponsounds2da[i].AsString("Stone1", null);
                    tmpWeaponSound.Wood0 = weaponsounds2da[i].AsString("Wood0", null);
                    tmpWeaponSound.Wood1 = weaponsounds2da[i].AsString("Wood1", null);
                    tmpWeaponSound.Chitin0 = weaponsounds2da[i].AsString("Chitin0", null);
                    tmpWeaponSound.Chitin1 = weaponsounds2da[i].AsString("Chitin1", null);
                    tmpWeaponSound.Scale0 = weaponsounds2da[i].AsString("Scale0", null);
                    tmpWeaponSound.Scale1 = weaponsounds2da[i].AsString("Scale1", null);
                    tmpWeaponSound.Ethereal0 = weaponsounds2da[i].AsString("Ethereal0", null);
                    tmpWeaponSound.Ethereal1 = weaponsounds2da[i].AsString("Ethereal1", null);
                    tmpWeaponSound.Crystal0 = weaponsounds2da[i].AsString("Crystal0", null);
                    tmpWeaponSound.Crystal1 = weaponsounds2da[i].AsString("Crystal1", null);
                    tmpWeaponSound.Miss0 = weaponsounds2da[i].AsString("Miss0", null);
                    tmpWeaponSound.Miss1 = weaponsounds2da[i].AsString("Miss1", null);
                    tmpWeaponSound.Parry = weaponsounds2da[i].AsString("Parry0", null);
                    tmpWeaponSound.Critical = weaponsounds2da[i].AsString("Critical0", null);

                    _importCollection.WeaponSounds.Add(tmpWeaponSound);
                }
            }
        }

        private void ImportInventorySounds()
        {
            if (customDataDict.TryGetValue("inventorysnds", out var inventorysnds2da))
            {
                customDataDict.Remove("inventorysnds");

                Log.Info("Importing 2DA: inventorysnds.2da");

                var originalInventorySounds2da = LoadOriginal2da("inventorysnds");
                for (int i = 0; i < inventorysnds2da.Count; i++)
                {
                    if (!ImportRecord<InventorySound>(i, inventorysnds2da, originalInventorySounds2da, out var recordId)) continue;

                    var tmpInventorySound = new InventorySound();
                    tmpInventorySound.ID = recordId;
                    tmpInventorySound.Index = i - originalInventorySounds2da.Count;
                    tmpInventorySound.SourceLabel = inventorysnds2da[i].AsString("Label");

                    if (inventorysnds2da[i].IsNull("Label")) continue;
                    tmpInventorySound.Name = inventorysnds2da[i].AsString("Label") ?? "";
                    tmpInventorySound.Sound = inventorysnds2da[i].AsString("InventorySound") ?? "";

                    _importCollection.InventorySounds.Add(tmpInventorySound);
                }
            }
        }

        private void ImportVisualEffects()
        {
            if (customDataDict.TryGetValue("visualeffects", out var vfx2da))
            {
                customDataDict.Remove("visualeffects");

                Log.Info("Importing 2DA: visualeffects.2da");

                var originalVfx2da = LoadOriginal2da("visualeffects");
                for (int i = 0; i < vfx2da.Count; i++)
                {
                    if (!ImportRecord<VisualEffect>(i, vfx2da, originalVfx2da, out var recordId)) continue;

                    var tmpVfx = new VisualEffect();
                    tmpVfx.ID = recordId;
                    tmpVfx.Index = i - originalVfx2da.Count;
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

                    _importCollection.VisualEffects.Add(tmpVfx);
                }
            }
        }

        private void ImportProgrammedEffects()
        {
            if (customDataDict.TryGetValue("progfx", out var progFX2da))
            {
                customDataDict.Remove("progfx");

                Log.Info("Importing 2DA: progfx.2da");

                var originalProgFX2da = LoadOriginal2da("progfx");
                for (int i = 0; i < progFX2da.Count; i++)
                {
                    if (!ImportRecord<ProgrammedEffect>(i, progFX2da, originalProgFX2da, out var recordId)) continue;

                    var tmpProgFX = new ProgrammedEffect();
                    tmpProgFX.ID = recordId;
                    tmpProgFX.Index = i - originalProgFX2da.Count;
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

                    _importCollection.ProgrammedEffects.Add(tmpProgFX);
                }
            }
        }

        private PackageSpellPreferencesTable ImportSpellPreferencesTable(string tablename, Guid guid)
        {
            var spellPreferencesTable2da = customDataDict[tablename.ToLower()];

            var tmpSpellPreferencesTable = new PackageSpellPreferencesTable();
            tmpSpellPreferencesTable.ID = guid;
            tmpSpellPreferencesTable.Name = tablename.ToUpper();

            tmpSpellPreferencesTable.Clear();
            for (int i = 0; i < spellPreferencesTable2da.Count; i++)
            {
                var tmpItem = new PackageSpellPreferencesTableItem();
                tmpItem.ParentTable = tmpSpellPreferencesTable;
                tmpItem.SourceLabel = spellPreferencesTable2da[i].AsString("Label") ?? "";
                tmpItem.Spell = CreateRef<Spell>(spellPreferencesTable2da[i].AsInteger("SpellIndex"));
                tmpSpellPreferencesTable.Add(tmpItem);
            }

            _importCollection.SpellPreferencesTables.Add(tmpSpellPreferencesTable);

            return tmpSpellPreferencesTable;
        }

        private PackageFeatPreferencesTable ImportFeatPreferencesTable(string tablename, Guid guid)
        {
            var featPreferencesTable2da = customDataDict[tablename.ToLower()];

            var tmpFeatPreferencesTable = new PackageFeatPreferencesTable();
            tmpFeatPreferencesTable.ID = guid;
            tmpFeatPreferencesTable.Name = tablename.ToUpper();

            tmpFeatPreferencesTable.Clear();
            for (int i = 0; i < featPreferencesTable2da.Count; i++)
            {
                var tmpItem = new PackageFeatPreferencesTableItem();
                tmpItem.ParentTable = tmpFeatPreferencesTable;
                tmpItem.SourceLabel = featPreferencesTable2da[i].AsString("Label") ?? "";
                tmpItem.Feat = CreateRef<Feat>(featPreferencesTable2da[i].AsInteger("FeatIndex"));
                tmpFeatPreferencesTable.Add(tmpItem);
            }

            _importCollection.FeatPreferencesTables.Add(tmpFeatPreferencesTable);

            return tmpFeatPreferencesTable;
        }

        private PackageSkillPreferencesTable ImportSkillPreferencesTable(string tablename, Guid guid)
        {
            var skillPreferencesTable2da = customDataDict[tablename.ToLower()];

            var tmpSkillPreferencesTable = new PackageSkillPreferencesTable();
            tmpSkillPreferencesTable.ID = guid;
            tmpSkillPreferencesTable.Name = tablename.ToUpper();

            tmpSkillPreferencesTable.Clear();
            for (int i = 0; i < skillPreferencesTable2da.Count; i++)
            {
                var tmpItem = new PackageSkillPreferencesTableItem();
                tmpItem.ParentTable = tmpSkillPreferencesTable;
                tmpItem.SourceLabel = skillPreferencesTable2da[i].AsString("Label") ?? "";
                tmpItem.Skill = CreateRef<Skill>(skillPreferencesTable2da[i].AsInteger("SkillIndex"));
                tmpSkillPreferencesTable.Add(tmpItem);
            }

            _importCollection.SkillPreferencesTables.Add(tmpSkillPreferencesTable);

            return tmpSkillPreferencesTable;
        }

        private PackageEquipmentTable ImportPackageEquipmentTable(string tablename, Guid guid)
        {
            var equipmentTable2da = customDataDict[tablename.ToLower()];

            var tmpEquipmentTable = new PackageEquipmentTable();
            tmpEquipmentTable.ID = guid;
            tmpEquipmentTable.Name = tablename.ToUpper();

            tmpEquipmentTable.Clear();
            for (int i = 0; i < equipmentTable2da.Count; i++)
            {
                var tmpItem = new PackageEquipmentTableItem();
                tmpItem.ParentTable = tmpEquipmentTable;
                tmpItem.BlueprintResRef = equipmentTable2da[i].AsString("Label") ?? "";
                tmpEquipmentTable.Add(tmpItem);
            }

            _importCollection.PackageEquipmentTables.Add(tmpEquipmentTable);

            return tmpEquipmentTable;
        }

        private void ImportClassPackages()
        {
            if (customDataDict.TryGetValue("packages", out var packages2da))
            {
                customDataDict.Remove("packages");

                Log.Info("Importing 2DA: packages.2da");

                var originalPackages2da = LoadOriginal2da("packages");
                for (int i = 0; i < packages2da.Count; i++)
                {
                    if (!ImportRecord<ClassPackage>(i, packages2da, originalPackages2da, out var recordId)) continue;

                    var tmpPackage = new ClassPackage();
                    tmpPackage.ID = recordId;
                    tmpPackage.Index = i - originalPackages2da.Count;
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
                    tmpPackage.SpellPreferences = GetOrImportTable(packages2da[i].AsString("SpellPref2DA"), ImportSpellPreferencesTable);
                    tmpPackage.FeatPreferences = GetOrImportTable(packages2da[i].AsString("FeatPref2DA"), ImportFeatPreferencesTable);
                    tmpPackage.SkillPreferences = GetOrImportTable(packages2da[i].AsString("SkillPref2DA"), ImportSkillPreferencesTable);
                    tmpPackage.StartingEquipment = GetOrImportTable(packages2da[i].AsString("Equip2DA"), ImportPackageEquipmentTable);
                    tmpPackage.Playable = packages2da[i].AsBoolean("PlayerClass");

                    _importCollection.ClassPackages.Add(tmpPackage);
                }
            }
        }

        private void ImportSoundsets()
        {
            if (customDataDict.TryGetValue("soundset", out var soundset2da))
            {
                customDataDict.Remove("soundset");

                Log.Info("Importing 2DA: soundset.2da");

                var originalSoundset2da = LoadOriginal2da("soundset");
                for (int i = 0; i < soundset2da.Count; i++)
                {
                    if (!ImportRecord<Soundset>(i, soundset2da, originalSoundset2da, out var recordId)) continue;

                    var tmpSoundset = new Soundset();
                    tmpSoundset.ID = recordId;
                    tmpSoundset.Index = i - originalSoundset2da.Count;
                    tmpSoundset.SourceLabel = soundset2da[i].AsString("LABEL");

                    if (!SetText(tmpSoundset.Name, soundset2da[i].AsInteger("STRREF"))) continue;

                    tmpSoundset.Gender = !soundset2da[i].IsNull("GENDER") ? (Gender)Enum.ToObject(typeof(Gender), soundset2da[i].AsInteger("GENDER") ?? 0) : Gender.Male;
                    tmpSoundset.Type = !soundset2da[i].IsNull("TYPE") ? (SoundsetType)Enum.ToObject(typeof(SoundsetType), soundset2da[i].AsInteger("TYPE") ?? 0) : SoundsetType.Player;
                    tmpSoundset.SoundsetResource = soundset2da[i].AsString("RESREF") ?? "";

                    if (ssfDict.TryGetValue(tmpSoundset.SoundsetResource.ToLower(), out var ssf))
                        ssfDict.Remove(tmpSoundset.SoundsetResource.ToLower());
                    else
                        ssf = new SsfFile(gameDataBif.ReadResource(tmpSoundset.SoundsetResource, NWNResourceType.SSF).RawData);

                    for (int j = 0; j < ssf.Data.Count; j++)
                    {
                        var soundsetEntry = tmpSoundset.Entries.GetByType((SoundsetEntryType)j);
                        if (soundsetEntry != null)
                        {
                            SetText(soundsetEntry.Text, ssf.Data[j].StringRef >= 0x01000000 ? null : (int)ssf.Data[j].StringRef);
                            soundsetEntry.SoundFile = new string(ssf.Data[j].ResRef).Trim('\0');
                        }
                    }

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
                    tmpPolymorph.Index = i - originalPolymorph2da.Count;
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

                    _importCollection.Polymorphs.Add(tmpPolymorph);
                }
            }
        }

        private void ImportCompanions()
        {
            if (customDataDict.TryGetValue("hen_companion", out var companions2da))
            {
                customDataDict.Remove("hen_companion");

                Log.Info("Importing 2DA: hen_companion.2da");

                var originalCompanions2da = LoadOriginal2da("hen_companion");
                for (int i = 0; i < companions2da.Count; i++)
                {
                    if (!ImportRecord<Companion>(i, companions2da, originalCompanions2da, out var recordId)) continue;

                    var tmpCompanion = new Companion();
                    tmpCompanion.ID = recordId;
                    tmpCompanion.Index = i - originalCompanions2da.Count;
                    tmpCompanion.SourceLabel = companions2da[i].AsString("NAME");

                    if (!SetText(tmpCompanion.Name, companions2da[i].AsInteger("STRREF"))) continue;
                    SetText(tmpCompanion.Description, companions2da[i].AsInteger("DESCRIPTION"));
                    tmpCompanion.Template = companions2da[i].AsString("BASERESREF") ?? "";

                    _importCollection.Companions.Add(tmpCompanion);
                }
            }
        }

        private void ImportFamiliars()
        {
            if (customDataDict.TryGetValue("hen_familiar", out var familiars2da))
            {
                customDataDict.Remove("hen_familiar");

                Log.Info("Importing 2DA: hen_familiar.2da");

                var originalFamiliars2da = LoadOriginal2da("hen_familiar");
                for (int i = 0; i < familiars2da.Count; i++)
                {
                    if (!ImportRecord<Familiar>(i, familiars2da, originalFamiliars2da, out var recordId)) continue;

                    var tmpFamiliar = new Familiar();
                    tmpFamiliar.ID = recordId;
                    tmpFamiliar.Index = i - originalFamiliars2da.Count;
                    tmpFamiliar.SourceLabel = familiars2da[i].AsString("NAME");

                    if (!SetText(tmpFamiliar.Name, familiars2da[i].AsInteger("STRREF"))) continue;
                    SetText(tmpFamiliar.Description, familiars2da[i].AsInteger("DESCRIPTION"));
                    tmpFamiliar.Template = familiars2da[i].AsString("BASERESREF") ?? "";

                    _importCollection.Familiars.Add(tmpFamiliar);
                }
            }
        }

        private void ImportTraps()
        {
            if (customDataDict.TryGetValue("traps", out var traps2da))
            {
                customDataDict.Remove("traps");

                Log.Info("Importing 2DA: traps.2da");

                var originalTraps2da = LoadOriginal2da("traps");
                for (int i = 0; i < traps2da.Count; i++)
                {
                    if (!ImportRecord<Trap>(i, traps2da, originalTraps2da, out var recordId)) continue;

                    var tmpTrap = new Trap();
                    tmpTrap.ID = recordId;
                    tmpTrap.Index = i - originalTraps2da.Count;
                    tmpTrap.SourceLabel = traps2da[i].AsString("Label");

                    if (!SetText(tmpTrap.Name, traps2da[i].AsInteger("TrapName"))) continue;
                    tmpTrap.TrapScript = traps2da[i].AsString("TrapScript") ?? "";
                    tmpTrap.SetDC = traps2da[i].AsInteger("SetDC") ?? 10;
                    tmpTrap.DetectDC = traps2da[i].AsInteger("DetectDCMod") ?? 10;
                    tmpTrap.DisarmDC = traps2da[i].AsInteger("DisarmDCMod") ?? 10;
                    tmpTrap.BlueprintResRef = traps2da[i].AsString("ResRef") ?? "";
                    tmpTrap.Icon = traps2da[i].AsString("IconResRef") ?? "";

                    _importCollection.Traps.Add(tmpTrap);
                }
            }
        }

        private void ImportDamageTypes()
        {
            if (customDataDict.TryGetValue("damagetypes", out var damagetypes2da))
            {
                customDataDict.Remove("damagetypes");

                Log.Info("Importing 2DA: damagetypes.2da");

                var originalDamageTypes2da = LoadOriginal2da("damagetypes");
                for (int i = 0; i < damagetypes2da.Count; i++)
                {
                    if (!ImportRecord<DamageType>(i, damagetypes2da, originalDamageTypes2da, out var recordId)) continue;

                    var tmpDamageType = new DamageType();
                    tmpDamageType.ID = recordId;
                    tmpDamageType.Index = i - originalDamageTypes2da.Count;
                    tmpDamageType.SourceLabel = damagetypes2da[i].AsString("Label");

                    if (!SetText(tmpDamageType.Name, damagetypes2da[i].AsInteger("CharsheetStrref"))) continue;
                    tmpDamageType.Group = CreateRef<DamageTypeGroup>(damagetypes2da[i].AsInteger("DamageTypeGroup"));

                    _importCollection.DamageTypes.Add(tmpDamageType);
                }
            }
        }

        private void ImportDamageTypeGroups()
        {
            if (customDataDict.TryGetValue("damagetypegroups", out var damagetypeGroups2da))
            {
                customDataDict.Remove("damagetypegroups");

                Log.Info("Importing 2DA: damagetypegroups.2da");

                var originalDamageTypeGroups2da = LoadOriginal2da("damagetypegroups");
                for (int i = 0; i < damagetypeGroups2da.Count; i++)
                {
                    if (!ImportRecord<DamageTypeGroup>(i, damagetypeGroups2da, originalDamageTypeGroups2da, out var recordId)) continue;

                    var tmpDamageTypeGroup = new DamageTypeGroup();
                    tmpDamageTypeGroup.ID = recordId;
                    tmpDamageTypeGroup.Index = i - originalDamageTypeGroups2da.Count;
                    tmpDamageTypeGroup.SourceLabel = damagetypeGroups2da[i].AsString("Label");

                    if (!SetText(tmpDamageTypeGroup.FeedbackText, damagetypeGroups2da[i].AsInteger("FeedbackStrref"))) continue;
                    tmpDamageTypeGroup.Name = damagetypeGroups2da[i].AsString("Label") ?? "";
                    if (!damagetypeGroups2da[i].IsNull("ColorR") && !damagetypeGroups2da[i].IsNull("ColorG") && !damagetypeGroups2da[i].IsNull("ColorB"))
                    {
                        var colorR = Convert.ToUInt32(damagetypeGroups2da[i].AsInteger("ColorR")) & 0xFF;
                        var colorG = Convert.ToUInt32(damagetypeGroups2da[i].AsInteger("ColorG")) & 0xFF;
                        var colorB = Convert.ToUInt32(damagetypeGroups2da[i].AsInteger("ColorB")) & 0xFF;
                        tmpDamageTypeGroup.Color = colorB | (colorG << 8) | (colorR << 16);
                    }

                    _importCollection.DamageTypeGroups.Add(tmpDamageTypeGroup);
                }
            }
        }

        private void ImportPortraits()
        {
            if (customDataDict.TryGetValue("portraits", out var portraits2da))
            {
                customDataDict.Remove("portraits");

                Log.Info("Importing 2DA: portraits.2da");

                var originalPortraits2da = LoadOriginal2da("portraits");
                for (int i = 0; i < portraits2da.Count; i++)
                {
                    if (!ImportRecord<Portrait>(i, portraits2da, originalPortraits2da, out var recordId)) continue;

                    var tmpPortrait = new Portrait();
                    tmpPortrait.ID = recordId;
                    tmpPortrait.Index = i - originalPortraits2da.Count;
                    tmpPortrait.SourceLabel = portraits2da[i].AsString("BaseResRef");

                    if (portraits2da[i].IsNull("BaseResRef")) continue;
                    tmpPortrait.ResRef = portraits2da[i].AsString("BaseResRef") ?? "";
                    tmpPortrait.LowGoreResRef = portraits2da[i].AsString("LowGore");
                    tmpPortrait.Gender = (PortraitGender?)portraits2da[i].AsInteger("Sex") ?? PortraitGender.Male;
                    tmpPortrait.PlaceableType = (PlaceableType?)portraits2da[i].AsInteger("InanimateType");
                    tmpPortrait.Race = CreateRef<Race>(portraits2da[i].AsInteger("Race"));
                    tmpPortrait.IsPlayerPortrait = !portraits2da[i].AsBoolean("Plot");

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

                if (package.ForClass != null && package.ForClass.IsSpellCaster)
                {
                    if (package.ForClass.IsArcaneCaster)
                    {
                        package.AssociateCompanion = null;
                        package.AssociateFamiliar = SolveInstance(package.AssociateFamiliar); ;
                    }
                    else
                    {
                        package.AssociateCompanion = SolveInstance(package.AssociateCompanion);
                        package.AssociateFamiliar = null;
                    }
                }
                else
                {
                    package.AssociateCompanion = null;
                    package.AssociateFamiliar = null;
                }
            }

            // Package Spell Preferences Tables
            foreach (var spellPrefsTable in _importCollection.SpellPreferencesTables)
            {
                if (spellPrefsTable == null) continue;
                for (int i = 0; i < spellPrefsTable.Count; i++)
                {
                    var item = spellPrefsTable[i];
                    if (item == null) continue;
                    item.Spell = SolveInstance(item.Spell);
                }
            }

            // Package Feat Preferences Tables
            foreach (var featPrefsTable in _importCollection.FeatPreferencesTables)
            {
                if (featPrefsTable == null) continue;
                for (int i = 0; i < featPrefsTable.Count; i++)
                {
                    var item = featPrefsTable[i];
                    if (item == null) continue;
                    item.Feat = SolveInstance(item.Feat);
                }
            }

            // Package Skill Preferences Tables
            foreach (var skillPrefsTable in _importCollection.SkillPreferencesTables)
            {
                if (skillPrefsTable == null) continue;
                for (int i = 0; i < skillPrefsTable.Count; i++)
                {
                    var item = skillPrefsTable[i];
                    if (item == null) continue;
                    item.Skill = SolveInstance(item.Skill);
                }
            }

            // Area Effects
            foreach (var aoe in _importCollection.AreaEffects)
            {
                if (aoe == null) continue;
                aoe.VisualEffect = SolveInstance(aoe.VisualEffect);
            }

            // Appearances
            foreach (var appearance in _importCollection.Appearances)
            {
                if (appearance == null) continue;
                appearance.AppearanceSoundset = SolveInstance(appearance.AppearanceSoundset);
            }

            // Appearance Soundsets
            foreach (var appearanceSoundset in _importCollection.AppearanceSoundsets)
            {
                if (appearanceSoundset == null) continue;
                appearanceSoundset.LeftAttack = SolveInstance(appearanceSoundset.LeftAttack);
                appearanceSoundset.RightAttack = SolveInstance(appearanceSoundset.RightAttack);
                appearanceSoundset.StraightAttack = SolveInstance(appearanceSoundset.StraightAttack);
                appearanceSoundset.LowCloseAttack = SolveInstance(appearanceSoundset.LowCloseAttack);
                appearanceSoundset.HighCloseAttack = SolveInstance(appearanceSoundset.HighCloseAttack);
                appearanceSoundset.ReachAttack = SolveInstance(appearanceSoundset.ReachAttack);
                appearanceSoundset.Miss = SolveInstance(appearanceSoundset.Miss);
            }

            // Base Items
            foreach (var baseItem in _importCollection.BaseItems)
            {
                if (baseItem == null) continue;
                baseItem.AmmunitionBaseItem = SolveInstance(baseItem.AmmunitionBaseItem);
                baseItem.InventorySound = SolveInstance(baseItem.InventorySound);
                baseItem.ItemPropertySet = SolveInstance(baseItem.ItemPropertySet);
                baseItem.RequiredFeat1 = SolveInstance(baseItem.RequiredFeat1);
                baseItem.RequiredFeat2 = SolveInstance(baseItem.RequiredFeat2);
                baseItem.RequiredFeat3 = SolveInstance(baseItem.RequiredFeat3);
                baseItem.RequiredFeat4 = SolveInstance(baseItem.RequiredFeat4);
                baseItem.RequiredFeat5 = SolveInstance(baseItem.RequiredFeat5);
                baseItem.WeaponSound = SolveInstance(baseItem.WeaponSound);
                baseItem.WeaponFocusFeat = SolveInstance(baseItem.WeaponFocusFeat);
                baseItem.EpicWeaponFocusFeat = SolveInstance(baseItem.EpicWeaponFocusFeat);
                baseItem.WeaponSpecializationFeat = SolveInstance(baseItem.WeaponSpecializationFeat);
                baseItem.EpicWeaponSpecializationFeat = SolveInstance(baseItem.EpicWeaponSpecializationFeat);
                baseItem.ImprovedCriticalFeat = SolveInstance(baseItem.ImprovedCriticalFeat);
                baseItem.OverwhelmingCriticalFeat = SolveInstance(baseItem.OverwhelmingCriticalFeat);
                baseItem.DevastatingCriticalFeat = SolveInstance(baseItem.DevastatingCriticalFeat);
                baseItem.WeaponOfChoiceFeat = SolveInstance(baseItem.WeaponOfChoiceFeat);
            }

            // Visual Effects
            foreach (var vfx in _importCollection.VisualEffects)
            {
                if (vfx == null) continue;
                vfx.ImpactProgFX = SolveInstance(vfx.ImpactProgFX);
                vfx.DurationProgFX = SolveInstance(vfx.DurationProgFX);
                vfx.CessationProgFX = SolveInstance(vfx.CessationProgFX);
            }

            // Programmed Effects
            foreach (var progFX in _importCollection.ProgrammedEffects)
            {
                if (progFX == null) continue;
                progFX.T1OnHitVFX = SolveInstance(progFX.T1OnHitVFX);
                progFX.T1OnHitVFXSmall = SolveInstance(progFX.T1OnHitVFXSmall);
                progFX.T10Spell = SolveInstance(progFX.T10Spell);
            }

            // Damage Types
            foreach (var damageType in _importCollection.DamageTypes)
            {
                if (damageType == null) continue;
                damageType.Group = SolveInstance(damageType.Group);
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
                    race.Index = -1;

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
                    cls.Index = -1;

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
                    package.Index = -1;

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
                    domain.Index = -1;

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
                    spell.Index = -1;

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
                    masterFeat.Index = -1;

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
                    feat.Index = -1;

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
                    skill.Index = -1;

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
                    disease.Index = -1;

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
                    poison.Index = -1;

                    var projectOverridePoison = MasterRepository.Project.GetOverride(standardPoison);
                    if (projectOverridePoison != null)
                        MasterRepository.Project.Poisons.Remove(projectOverridePoison);
                }

                MasterRepository.Project.Poisons.Add(poison);
            }

            // Base Items
            foreach (var baseItem in _importCollection.BaseItems)
            {
                if (baseItem == null) continue;

                var standardBaseItem = MasterRepository.Standard.BaseItems.GetByID(baseItem.ID);
                if (standardBaseItem != null)
                {
                    baseItem.ID = Guid.Empty;
                    baseItem.Overrides = standardBaseItem.ID;
                    baseItem.Index = -1;

                    var projectOverrideBaseItem = MasterRepository.Project.GetOverride(standardBaseItem);
                    if (projectOverrideBaseItem != null)
                        MasterRepository.Project.BaseItems.Remove(projectOverrideBaseItem);
                }

                MasterRepository.Project.BaseItems.Add(baseItem);
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
                    aoe.Index = -1;

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
                    polymorph.Index = -1;

                    var projectOverridePolymorph = MasterRepository.Project.GetOverride(standardPolymorph);
                    if (projectOverridePolymorph != null)
                        MasterRepository.Project.Polymorphs.Remove(projectOverridePolymorph);
                }

                MasterRepository.Project.Polymorphs.Add(polymorph);
            }

            // Companions
            foreach (var companion in _importCollection.Companions)
            {
                if (companion == null) continue;

                var standardCompanion = MasterRepository.Standard.Companions.GetByID(companion.ID);
                if (standardCompanion != null)
                {
                    companion.ID = Guid.Empty;
                    companion.Overrides = standardCompanion.ID;
                    companion.Index = -1;

                    var projectOverrideCompanion = MasterRepository.Project.GetOverride(standardCompanion);
                    if (projectOverrideCompanion != null)
                        MasterRepository.Project.Companions.Remove(projectOverrideCompanion);
                }

                MasterRepository.Project.Companions.Add(companion);
            }

            // Familiars
            foreach (var familiar in _importCollection.Familiars)
            {
                if (familiar == null) continue;

                var standardFamiliar = MasterRepository.Standard.Familiars.GetByID(familiar.ID);
                if (standardFamiliar != null)
                {
                    familiar.ID = Guid.Empty;
                    familiar.Overrides = standardFamiliar.ID;
                    familiar.Index = -1;

                    var projectOverrideFamiliar = MasterRepository.Project.GetOverride(standardFamiliar);
                    if (projectOverrideFamiliar != null)
                        MasterRepository.Project.Familiars.Remove(projectOverrideFamiliar);
                }

                MasterRepository.Project.Familiars.Add(familiar);
            }

            // Traps
            foreach (var trap in _importCollection.Traps)
            {
                if (trap == null) continue;

                var standardTrap = MasterRepository.Standard.Traps.GetByID(trap.ID);
                if (standardTrap != null)
                {
                    trap.ID = Guid.Empty;
                    trap.Overrides = standardTrap.ID;
                    trap.Index = -1;

                    var projectOverrideTrap = MasterRepository.Project.GetOverride(standardTrap);
                    if (projectOverrideTrap != null)
                        MasterRepository.Project.Traps.Remove(projectOverrideTrap);
                }

                MasterRepository.Project.Traps.Add(trap);
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
                    soundset.Index = -1;

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
                    vfx.Index = -1;

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
                    appearance.Index = -1;

                    var projectOverrideAppearance = MasterRepository.Project.GetOverride(standardAppearance);
                    if (projectOverrideAppearance != null)
                        MasterRepository.Project.Appearances.Remove(projectOverrideAppearance);
                }

                MasterRepository.Project.Appearances.Add(appearance);
            }

            // Appearance Soundsets
            foreach (var appearanceSoundset in _importCollection.AppearanceSoundsets)
            {
                if (appearanceSoundset == null) continue;

                var standardAppearanceSoundset = MasterRepository.Standard.AppearanceSoundsets.GetByID(appearanceSoundset.ID);
                if (standardAppearanceSoundset != null)
                {
                    appearanceSoundset.ID = Guid.Empty;
                    appearanceSoundset.Overrides = standardAppearanceSoundset.ID;
                    appearanceSoundset.Index = -1;

                    var projectOverrideAppearanceSoundset = MasterRepository.Project.GetOverride(standardAppearanceSoundset);
                    if (projectOverrideAppearanceSoundset != null)
                        MasterRepository.Project.AppearanceSoundsets.Remove(projectOverrideAppearanceSoundset);
                }

                MasterRepository.Project.AppearanceSoundsets.Add(appearanceSoundset);
            }

            // Weapon Sounds
            foreach (var weaponSound in _importCollection.WeaponSounds)
            {
                if (weaponSound == null) continue;

                var standardWeaponSound = MasterRepository.Standard.WeaponSounds.GetByID(weaponSound.ID);
                if (standardWeaponSound != null)
                {
                    weaponSound.ID = Guid.Empty;
                    weaponSound.Overrides = standardWeaponSound.ID;
                    weaponSound.Index = -1;

                    var projectOverrideWeaponSound = MasterRepository.Project.GetOverride(standardWeaponSound);
                    if (projectOverrideWeaponSound != null)
                        MasterRepository.Project.WeaponSounds.Remove(projectOverrideWeaponSound);
                }

                MasterRepository.Project.WeaponSounds.Add(weaponSound);
            }

            // Inventory Sounds
            foreach (var inventorySound in _importCollection.InventorySounds)
            {
                if (inventorySound == null) continue;

                var standardInventorySound = MasterRepository.Standard.InventorySounds.GetByID(inventorySound.ID);
                if (standardInventorySound != null)
                {
                    inventorySound.ID = Guid.Empty;
                    inventorySound.Overrides = standardInventorySound.ID;
                    inventorySound.Index = -1;

                    var projectOverrideInventorySound = MasterRepository.Project.GetOverride(standardInventorySound);
                    if (projectOverrideInventorySound != null)
                        MasterRepository.Project.InventorySounds.Remove(projectOverrideInventorySound);
                }

                MasterRepository.Project.InventorySounds.Add(inventorySound);
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
                    portrait.Index = -1;

                    var projectOverridePortrait = MasterRepository.Project.GetOverride(standardPortrait);
                    if (projectOverridePortrait != null)
                        MasterRepository.Project.Portraits.Remove(projectOverridePortrait);
                }

                MasterRepository.Project.Portraits.Add(portrait);
            }

            // Programmed Effects
            foreach (var progFX in _importCollection.ProgrammedEffects)
            {
                if (progFX == null) continue;

                var standardProgFX = MasterRepository.Standard.ProgrammedEffects.GetByID(progFX.ID);
                if (standardProgFX != null)
                {
                    progFX.ID = Guid.Empty;
                    progFX.Overrides = standardProgFX.ID;
                    progFX.Index = -1;

                    var projectOverrideProgFX = MasterRepository.Project.GetOverride(standardProgFX);
                    if (projectOverrideProgFX != null)
                        MasterRepository.Project.ProgrammedEffects.Remove(projectOverrideProgFX);
                }

                MasterRepository.Project.ProgrammedEffects.Add(progFX);
            }

            // Damage Types
            foreach (var damageType in _importCollection.DamageTypes)
            {
                if (damageType == null) continue;

                var standardDamageType = MasterRepository.Standard.DamageTypes.GetByID(damageType.ID);
                if (standardDamageType != null)
                {
                    damageType.ID = Guid.Empty;
                    damageType.Overrides = standardDamageType.ID;
                    damageType.Index = -1;

                    var projectOverrideDamageType = MasterRepository.Project.GetOverride(standardDamageType);
                    if (projectOverrideDamageType != null)
                        MasterRepository.Project.DamageTypes.Remove(projectOverrideDamageType);
                }

                MasterRepository.Project.DamageTypes.Add(damageType);
            }

            // Damage Type Groups
            foreach (var damageTypeGroup in _importCollection.DamageTypeGroups)
            {
                if (damageTypeGroup == null) continue;

                var standardDamageTypeGroup = MasterRepository.Standard.DamageTypeGroups.GetByID(damageTypeGroup.ID);
                if (standardDamageTypeGroup != null)
                {
                    damageTypeGroup.ID = Guid.Empty;
                    damageTypeGroup.Overrides = standardDamageTypeGroup.ID;
                    damageTypeGroup.Index = -1;

                    var projectOverrideDamageTypeGroup = MasterRepository.Project.GetOverride(standardDamageTypeGroup);
                    if (projectOverrideDamageTypeGroup != null)
                        MasterRepository.Project.DamageTypeGroups.Remove(projectOverrideDamageTypeGroup);
                }

                MasterRepository.Project.DamageTypeGroups.Add(damageTypeGroup);
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
                    racialFeatsTable.Index = -1;

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
                    featsTable.Index = -1;

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
                    bfeatsTable.Index = -1;

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
                    skillTable.Index = -1;

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
                    attackTable.Index = -1;

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
                    savesTable.Index = -1;

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
                    requTable.Index = -1;

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
                    statGainTable.Index = -1;

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
                    spellSlotTable.Index = -1;

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
                    knownSpellsTable.Index = -1;

                    var projectOverrideKnownSpellsTable = MasterRepository.Project.GetOverride(standardKnownSpellsTable);
                    if (projectOverrideKnownSpellsTable != null)
                        MasterRepository.Project.KnownSpellsTables.Remove(projectOverrideKnownSpellsTable);
                }

                MasterRepository.Project.KnownSpellsTables.Add(knownSpellsTable);
            }

            // Package Spell Preferences
            foreach (var spellPrefTable in _importCollection.SpellPreferencesTables)
            {
                if (spellPrefTable == null) continue;

                var standardSpellPrefsTable = MasterRepository.Standard.SpellPreferencesTables.GetByID(spellPrefTable.ID);
                if (standardSpellPrefsTable != null)
                {
                    spellPrefTable.ID = Guid.Empty;
                    spellPrefTable.Overrides = standardSpellPrefsTable.ID;
                    spellPrefTable.Index = -1;

                    var projectOverrideSpellPrefTable = MasterRepository.Project.GetOverride(standardSpellPrefsTable);
                    if (projectOverrideSpellPrefTable != null)
                        MasterRepository.Project.SpellPreferencesTables.Remove(projectOverrideSpellPrefTable);
                }

                MasterRepository.Project.SpellPreferencesTables.Add(spellPrefTable);
            }

            // Package Feat Preferences
            foreach (var featPrefTable in _importCollection.FeatPreferencesTables)
            {
                if (featPrefTable == null) continue;

                var standardFeatPrefsTable = MasterRepository.Standard.FeatPreferencesTables.GetByID(featPrefTable.ID);
                if (standardFeatPrefsTable != null)
                {
                    featPrefTable.ID = Guid.Empty;
                    featPrefTable.Overrides = standardFeatPrefsTable.ID;
                    featPrefTable.Index = -1;

                    var projectOverrideFeatPrefTable = MasterRepository.Project.GetOverride(standardFeatPrefsTable);
                    if (projectOverrideFeatPrefTable != null)
                        MasterRepository.Project.FeatPreferencesTables.Remove(projectOverrideFeatPrefTable);
                }

                MasterRepository.Project.FeatPreferencesTables.Add(featPrefTable);
            }

            // Package Skill Preferences
            foreach (var skillPrefTable in _importCollection.SkillPreferencesTables)
            {
                if (skillPrefTable == null) continue;

                var standardSkillPrefsTable = MasterRepository.Standard.SkillPreferencesTables.GetByID(skillPrefTable.ID);
                if (standardSkillPrefsTable != null)
                {
                    skillPrefTable.ID = Guid.Empty;
                    skillPrefTable.Overrides = standardSkillPrefsTable.ID;
                    skillPrefTable.Index = -1;

                    var projectOverrideSkillPrefTable = MasterRepository.Project.GetOverride(standardSkillPrefsTable);
                    if (projectOverrideSkillPrefTable != null)
                        MasterRepository.Project.SkillPreferencesTables.Remove(projectOverrideSkillPrefTable);
                }

                MasterRepository.Project.SkillPreferencesTables.Add(skillPrefTable);
            }

            // Package Equipment
            foreach (var equipmentTable in _importCollection.PackageEquipmentTables)
            {
                if (equipmentTable == null) continue;

                var standardEquipmentTable = MasterRepository.Standard.PackageEquipmentTables.GetByID(equipmentTable.ID);
                if (standardEquipmentTable != null)
                {
                    equipmentTable.ID = Guid.Empty;
                    equipmentTable.Overrides = standardEquipmentTable.ID;
                    equipmentTable.Index = -1;

                    var projectOverrideEquipmentTable = MasterRepository.Project.GetOverride(standardEquipmentTable);
                    if (projectOverrideEquipmentTable != null)
                        MasterRepository.Project.PackageEquipmentTables.Remove(projectOverrideEquipmentTable);
                }

                MasterRepository.Project.PackageEquipmentTables.Add(equipmentTable);
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
                            if (((hak[i].ResourceType != NWNResourceType.TWODA) || (customDataDict.ContainsKey(hak[i].ResRef.ToLower()))) &&
                                ((hak[i].ResourceType != NWNResourceType.SSF) || (ssfDict.ContainsKey(hak[i].ResRef.ToLower()))))
                            {
                                ImportToExternalPath(hak[i], hak);
                            }
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
                ImportBaseItems();
                ImportClassPackages();

                ImportAppearances();
                ImportAppearanceSoundsets();
                ImportWeaponSounds();
                ImportInventorySounds();
                ImportVisualEffects();
                ImportProgrammedEffects();
                ImportSoundsets();
                ImportPolymorphs();
                ImportPortraits();
                ImportCompanions();
                ImportFamiliars();
                ImportTraps();
                ImportDamageTypes();
                ImportDamageTypeGroups();

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
