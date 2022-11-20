using Eos.Models;
using Eos.Models.Tables;
using Eos.Nwn.Bif;
using Eos.Nwn.Tlk;
using Eos.Nwn.TwoDimensionalArray;
using Eos.Repositories.Models;
using Eos.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using static Eos.Models.JsonUtils;

namespace Eos.Repositories
{
    internal static class MasterRepository
    {
        private static readonly List<DataTypeDefinition> defaultDataTypeList = new List<DataTypeDefinition>();
        private static readonly Dictionary<Guid, DataTypeDefinition> dataTypeByIdDict = new Dictionary<Guid, DataTypeDefinition>();

        private static readonly ResourceRepository resources;

        private static readonly RepositoryCollection standardCategory;
        private static readonly EosProject project;

        private static readonly VirtualModelRepository<Race> raceVirtualRepository;
        private static readonly VirtualModelRepository<CharacterClass> classVirtualRepository;
        private static readonly VirtualModelRepository<Domain> domainVirtualRepository;
        private static readonly VirtualModelRepository<Spell> spellVirtualRepository;
        private static readonly VirtualModelRepository<Feat> featVirtualRepository;
        private static readonly VirtualModelRepository<Skill> skillVirtualRepository;
        private static readonly VirtualModelRepository<Disease> diseaseVirtualRepository;
        private static readonly VirtualModelRepository<Poison> poisonVirtualRepository;
        private static readonly VirtualModelRepository<Spellbook> spellbookVirtualRepository;
        private static readonly VirtualModelRepository<AreaEffect> aoeVirtualRepository;

        private static readonly VirtualModelRepository<Appearance> appearanceVirtualRepository;
        private static readonly VirtualModelRepository<ClassPackage> classPackageVirtualRepository;
        private static readonly VirtualModelRepository<Soundset> soundsetVirtualRepository;

        private static readonly VirtualModelRepository<AttackBonusTable> babTableVirtualRepository;
        private static readonly VirtualModelRepository<BonusFeatsTable> bonusFeatTableVirtualRepository;
        private static readonly VirtualModelRepository<FeatsTable> featsTableVirtualRepository;
        private static readonly VirtualModelRepository<SavingThrowTable> savesTableVirtualRepository;
        private static readonly VirtualModelRepository<SkillsTable> skillsTableVirtualRepository;
        private static readonly VirtualModelRepository<PrerequisiteTable> requTableVirtualRepository;
        private static readonly VirtualModelRepository<SpellSlotTable> spellSlotTableVirtualRepository;
        private static readonly VirtualModelRepository<KnownSpellsTable> knownSpellsTableVirtualRepository;
        private static readonly VirtualModelRepository<StatGainTable> statGainTableVirtualRepository;
        private static readonly VirtualModelRepository<RacialFeatsTable> racialFeatsTableVirtualRepository;

        // Custom Datatypes
        private static readonly VirtualModelRepository<CustomEnum> customEnumVirtualRepository;
        private static readonly VirtualModelRepository<CustomObject> customObjectVirtualRepository;

        static MasterRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<AreaEffect>(typeof(AreaEffectRepository));
            RepositoryFactory.RegisterRepositoryClass<CharacterClass>(typeof(ClassRepository));
            RepositoryFactory.RegisterRepositoryClass<ClassPackage>(typeof(ClassPackageRepository));
            RepositoryFactory.RegisterRepositoryClass<CustomObjectInstance>(typeof(CustomObjectInstanceRepository));
            RepositoryFactory.RegisterRepositoryClass<Disease>(typeof(DiseaseRepository));
            RepositoryFactory.RegisterRepositoryClass<Domain>(typeof(DomainRepository));
            RepositoryFactory.RegisterRepositoryClass<Feat>(typeof(FeatRepository));
            RepositoryFactory.RegisterRepositoryClass<Poison>(typeof(PoisonRepository));
            RepositoryFactory.RegisterRepositoryClass<Race>(typeof(RaceRepository));
            RepositoryFactory.RegisterRepositoryClass<Skill>(typeof(SkillRepository));
            RepositoryFactory.RegisterRepositoryClass<Soundset>(typeof(SoundsetRepository));
            RepositoryFactory.RegisterRepositoryClass<Spell>(typeof(SpellRepository));
            RepositoryFactory.RegisterRepositoryClass<Spellbook>(typeof(SpellbookRepository));

            resources = new ResourceRepository();

            standardCategory = new RepositoryCollection(true);
            project = new EosProject();

            raceVirtualRepository = new VirtualModelRepository<Race>(standardCategory.Races, project.Races);
            classVirtualRepository = new VirtualModelRepository<CharacterClass>(standardCategory.Classes, project.Classes);
            domainVirtualRepository = new VirtualModelRepository<Domain>(standardCategory.Domains, project.Domains);
            spellVirtualRepository = new VirtualModelRepository<Spell>(standardCategory.Spells, project.Spells);
            featVirtualRepository = new VirtualModelRepository<Feat>(standardCategory.Feats, project.Feats);
            skillVirtualRepository = new VirtualModelRepository<Skill>(standardCategory.Skills, project.Skills);
            diseaseVirtualRepository = new VirtualModelRepository<Disease>(standardCategory.Diseases, project.Diseases);
            poisonVirtualRepository = new VirtualModelRepository<Poison>(standardCategory.Poisons, project.Poisons);
            spellbookVirtualRepository = new VirtualModelRepository<Spellbook>(standardCategory.Spellbooks, project.Spellbooks);
            aoeVirtualRepository = new VirtualModelRepository<AreaEffect>(standardCategory.AreaEffects, project.AreaEffects);

            appearanceVirtualRepository = new VirtualModelRepository<Appearance>(standardCategory.Appearances, project.Appearances);
            classPackageVirtualRepository = new VirtualModelRepository<ClassPackage>(standardCategory.ClassPackages, project.ClassPackages);
            soundsetVirtualRepository = new VirtualModelRepository<Soundset>(standardCategory.Soundsets, project.Soundsets);

            babTableVirtualRepository = new VirtualModelRepository<AttackBonusTable>(standardCategory.AttackBonusTables, project.AttackBonusTables);
            bonusFeatTableVirtualRepository = new VirtualModelRepository<BonusFeatsTable>(standardCategory.BonusFeatTables, project.BonusFeatTables);
            featsTableVirtualRepository = new VirtualModelRepository<FeatsTable>(standardCategory.FeatTables, project.FeatTables);
            savesTableVirtualRepository = new VirtualModelRepository<SavingThrowTable>(standardCategory.SavingThrowTables, project.SavingThrowTables);
            skillsTableVirtualRepository = new VirtualModelRepository<SkillsTable>(standardCategory.SkillTables, project.SkillTables);
            requTableVirtualRepository = new VirtualModelRepository<PrerequisiteTable>(standardCategory.PrerequisiteTables, project.PrerequisiteTables);
            spellSlotTableVirtualRepository = new VirtualModelRepository<SpellSlotTable>(standardCategory.SpellSlotTables, project.SpellSlotTables);
            knownSpellsTableVirtualRepository = new VirtualModelRepository<KnownSpellsTable>(standardCategory.KnownSpellsTables, project.KnownSpellsTables);
            statGainTableVirtualRepository = new VirtualModelRepository<StatGainTable>(standardCategory.StatGainTables, project.StatGainTables);
            racialFeatsTableVirtualRepository = new VirtualModelRepository<RacialFeatsTable>(standardCategory.RacialFeatsTables, project.RacialFeatsTables);

            // Custom Datatypes
            customEnumVirtualRepository = new VirtualModelRepository<CustomEnum>(standardCategory.CustomEnums, project.CustomEnums);
            customObjectVirtualRepository = new VirtualModelRepository<CustomObject>(standardCategory.CustomObjects, project.CustomObjects);

            InitDefaultDataTypes();
        }

        private static void AddDataType(Guid id, String label, object? type, DataTypeToJsonDelegate toJsonFunc, DataTypeFromJsonDelegate fromJsonFunc, DataTypeTo2DADelegate to2DAFunc, bool isCustom = false, bool isVisualOnly = false)
        {
            var dataType = new DataTypeDefinition(id, label, type, isCustom);
            dataType.ToJson = toJsonFunc;
            dataType.FromJson = fromJsonFunc;
            dataType.To2DA = to2DAFunc;
            dataType.IsVisualOnly = isVisualOnly;
            defaultDataTypeList.Add(dataType);
            dataTypeByIdDict.Add(id, dataType);
        }

        private static void InitDefaultDataTypes()
        {
            DataTypeTo2DADelegate to2daIdentity = o => o;

            AddDataType(Guid.Parse("a136669b-e618-4be1-9a29-8b76f85c60be"), "None", null, o => null, json => null, to2daIdentity, false, true);

            AddDataType(Guid.Parse("0bd1d9ec-7909-4f25-bd98-fa034915c0fb"), "String", typeof(string), o => (string?)o, json => json?.GetValue<string>(), to2daIdentity);
            AddDataType(Guid.Parse("0212ec84-bd23-441a-8269-713a3d765cbe"), "Integer", typeof(int), o => (int?)o, json => json?.GetValue<int>(), to2daIdentity);
            AddDataType(Guid.Parse("7dd178ac-c87f-4849-a539-ad9bf2c95220"), "Double", typeof(double), o => (double?)o, json => json?.GetValue<double>(), to2daIdentity);
            AddDataType(Guid.Parse("bd7ee740-d4a5-41b7-bcf6-cf14923e78dc"), "Boolean", typeof(bool), o => (bool?)o ?? false, json => json?.GetValue<bool>() ?? false, to2daIdentity);

            AddDataType(Guid.Parse("e4a815d7-78e1-4e12-84e8-129a69cbb951"), "Variant", typeof(VariantValue),
                o =>
                {
                    if ((o is VariantValue varValue) && (varValue.DataType?.ToJson != null))
                    {
                        var json = new JsonObject();
                        json.Add("DataType", varValue.DataType.ID.ToString());
                        json.Add("Value", varValue.DataType.ToJson(varValue.Value));
                        return json;
                    }
                    return null;
                },
                json =>
                {

                    var varValue = new VariantValue();
                    if (json != null)
                    {
                        varValue.DataType = GetDataTypeById(ParseGuid(json["DataType"]?.GetValue<String>()));
                        if (varValue.DataType?.FromJson != null)
                            varValue.Value = varValue.DataType?.FromJson(json["Value"]);
                    }

                    return varValue;
                },
                o =>
                {
                    if ((o is VariantValue varValue) && (varValue.DataType?.To2DA != null))
                        return varValue.DataType?.To2DA(varValue.Value);
                    return null;
                });

            DataTypeToJsonDelegate modelToJson = o => ((BaseModel?)o)?.ToJsonRef();
            AddDataType(Guid.Parse("75a47130-999f-46b3-ac79-0e8e9ca48344"), "Race", typeof(Race), modelToJson, json => JsonUtils.CreateRefFromJson<Race>((JsonObject?)json), o => Project.Races.Get2DAIndex((Race?)o));
            AddDataType(Guid.Parse("8541fb58-1534-464d-baab-0384381cc506"), "Class", typeof(CharacterClass), modelToJson, json => JsonUtils.CreateRefFromJson<CharacterClass>((JsonObject?)json), o => Project.Classes.Get2DAIndex((CharacterClass?)o));
            AddDataType(Guid.Parse("3f2d92d3-2860-4b90-8f78-f8b2bb9c4820"), "Feat", typeof(Feat), modelToJson, json => JsonUtils.CreateRefFromJson<Feat>((JsonObject?)json), o => Project.Feats.Get2DAIndex((Feat?)o));
            AddDataType(Guid.Parse("ff714377-786a-4a29-9fd6-72bd04a0c968"), "Skill", typeof(Skill), modelToJson, json => JsonUtils.CreateRefFromJson<Skill>((JsonObject?)json), o => Project.Skills.Get2DAIndex((Skill?)o));
            AddDataType(Guid.Parse("6380f69e-92fb-4717-ab00-01a1060727fc"), "Domain", typeof(Domain), modelToJson, json => JsonUtils.CreateRefFromJson<Domain>((JsonObject?)json), o => Project.Domains.Get2DAIndex((Domain?)o));
            AddDataType(Guid.Parse("7649f1d6-cd21-4af0-abae-834c5898b75b"), "Spell", typeof(Spell), modelToJson, json => JsonUtils.CreateRefFromJson<Spell>((JsonObject?)json), o => Project.Spells.Get2DAIndex((Spell?)o));
            AddDataType(Guid.Parse("159e9a47-de78-435d-9047-d96847544883"), "Poison", typeof(Poison), modelToJson, json => JsonUtils.CreateRefFromJson<Poison>((JsonObject?)json), o => Project.Poisons.Get2DAIndex((Poison?)o));
            AddDataType(Guid.Parse("c241bc8c-0a05-477d-9cbd-3bb4e50d0bfb"), "Disease", typeof(Disease), modelToJson, json => JsonUtils.CreateRefFromJson<Disease>((JsonObject?)json), o => Project.Diseases.Get2DAIndex((Disease?)o));
            AddDataType(Guid.Parse("f4d8a469-32a6-4f68-bccd-7710ebb026d9"), "Area Effect", typeof(AreaEffect), modelToJson, json => JsonUtils.CreateRefFromJson<AreaEffect>((JsonObject?)json), o => Project.AreaEffects.Get2DAIndex((AreaEffect?)o));

            AddDataType(Guid.Parse("80b538dc-7e6a-40c7-830a-05bdac2fe3a4"), "Appearance", typeof(Appearance), modelToJson, json => JsonUtils.CreateRefFromJson<Appearance>((JsonObject?)json), o => Project.Appearances.Get2DAIndex((Appearance?)o));
            AddDataType(Guid.Parse("8fb225be-dca3-4dce-bd38-fbde34e6fce1"), "Soundset", typeof(Soundset), modelToJson, json => JsonUtils.CreateRefFromJson<Soundset>((JsonObject?)json), o => Project.Soundsets.Get2DAIndex((Soundset?)o));
            AddDataType(Guid.Parse("3bffa036-db5f-4946-ae66-ed4c31a29830"), "Class Package", typeof(ClassPackage), modelToJson, json => JsonUtils.CreateRefFromJson<ClassPackage>((JsonObject?)json), o => Project.ClassPackages.Get2DAIndex((ClassPackage?)o));

            //AddDataType(Guid.Parse("5045137d-9b31-47e7-8391-2c9f0b4dd4eb"), "BAB Table", typeof(AttackBonusTable), modelToJson, json => JsonUtils.CreateRefFromJson<AttackBonusTable>((JsonObject?)json), o => ((AttackBonusTable?)o)?.Name.ToUpper());
            //AddDataType(Guid.Parse("769b4809-d4c9-404b-a9a7-b8079cd7ac89"), "Bonusfeats Table", typeof(BonusFeatsTable), modelToJson, json => JsonUtils.CreateRefFromJson<BonusFeatsTable>((JsonObject?)json), o => ((BonusFeatsTable?)o)?.Name.ToUpper());
            //AddDataType(Guid.Parse("bd134d10-ea62-4481-b4a6-83d8b43bb570"), "Feats Table", typeof(FeatsTable), modelToJson, json => JsonUtils.CreateRefFromJson<FeatsTable>((JsonObject?)json), o => ((FeatsTable?)o)?.Name.ToUpper());
            //AddDataType(Guid.Parse("852b8aeb-e4da-4731-b4e0-9150ea0808d7"), "Known Spells Table", typeof(KnownSpellsTable), modelToJson, json => JsonUtils.CreateRefFromJson<KnownSpellsTable>((JsonObject?)json), o => ((KnownSpellsTable?)o)?.Name.ToUpper());
            //AddDataType(Guid.Parse("caf890c6-fa71-4fab-99b2-c7201550a8bd"), "Prerequesite Table", typeof(PrerequisiteTable), modelToJson, json => JsonUtils.CreateRefFromJson<PrerequisiteTable>((JsonObject?)json), o => ((PrerequisiteTable?)o)?.Name.ToUpper());
            //AddDataType(Guid.Parse("2e849029-f59a-455a-b77a-06ba01244e70"), "Racial Feats Table", typeof(RacialFeatsTable), modelToJson, json => JsonUtils.CreateRefFromJson<RacialFeatsTable>((JsonObject?)json), o => ((RacialFeatsTable?)o)?.Name.ToUpper());
            //AddDataType(Guid.Parse("54792f94-9eba-411a-a973-0609c6ac53d7"), "Saving Throw Table", typeof(SavingThrowTable), modelToJson, json => JsonUtils.CreateRefFromJson<SavingThrowTable>((JsonObject?)json), o => ((SavingThrowTable?)o)?.Name.ToUpper());
            //AddDataType(Guid.Parse("7b6012ef-09d8-4cfe-9a4b-52771b0f490e"), "Skills Table", typeof(SkillsTable), modelToJson, json => JsonUtils.CreateRefFromJson<SkillsTable>((JsonObject?)json), o => ((SkillsTable?)o)?.Name.ToUpper());
            //AddDataType(Guid.Parse("a553ae68-0582-410f-aa71-28f0a9cd2167"), "Spellslot Table", typeof(SpellSlotTable), modelToJson, json => JsonUtils.CreateRefFromJson<SpellSlotTable>((JsonObject?)json), o => ((SpellSlotTable?)o)?.Name.ToUpper());
            //AddDataType(Guid.Parse("8bc50499-f005-4698-a10d-2994e62e7806"), "Statgain Table", typeof(StatGainTable), modelToJson, json => JsonUtils.CreateRefFromJson<StatGainTable>((JsonObject?)json), o => ((StatGainTable?)o)?.Name.ToUpper());
        }

        public static DataTypeDefinition? GetDataTypeById(Guid id)
        {
            if (!dataTypeByIdDict.TryGetValue(id, out var result))
            {
                var customEnum = Project.CustomEnums.GetByID(id);
                if (customEnum != null)
                    return customEnum.DataTypeDefinition;

                var customObject = Project.CustomObjects.GetByID(id);
                if (customObject != null)
                    return customObject.DataTypeDefinition;
            }

            return result;
        }

        public static void Initialize(string nwnBasePath)
        {
            resources.Initialize(nwnBasePath);
        }

        public static void LoadExternalResources(string externalBasePath)
        {
            resources.LoadExternalResources(externalBasePath);
        }

        public static void Cleanup()
        {
            resources.Cleanup();
        }

        public static BaseModel New(Type modelType)
        {
            return Project.New(modelType);
        }

        public static void Add(BaseModel model)
        {
            Project.Add(model);
        }

        public static ResourceRepository Resources { get { return resources; } }

        public static RepositoryCollection Standard { get { return standardCategory; } }
        public static EosProject Project { get { return project; } }

        public static VirtualModelRepository<Race> Races { get { return raceVirtualRepository; } }
        public static VirtualModelRepository<CharacterClass> Classes { get { return classVirtualRepository; } }
        public static VirtualModelRepository<Domain> Domains { get { return domainVirtualRepository; } }
        public static VirtualModelRepository<Spell> Spells { get { return spellVirtualRepository; } }
        public static VirtualModelRepository<Feat> Feats { get { return featVirtualRepository; } }
        public static VirtualModelRepository<Skill> Skills { get { return skillVirtualRepository; } }
        public static VirtualModelRepository<Disease> Diseases { get { return diseaseVirtualRepository; } }
        public static VirtualModelRepository<Poison> Poisons { get { return poisonVirtualRepository; } }
        public static VirtualModelRepository<Spellbook> Spellbooks { get { return spellbookVirtualRepository; } }
        public static VirtualModelRepository<AreaEffect> AreaEffects { get { return aoeVirtualRepository; } }

        public static VirtualModelRepository<Appearance> Appearances { get { return appearanceVirtualRepository; } }
        public static VirtualModelRepository<ClassPackage> ClassPackages { get { return classPackageVirtualRepository; } }
        public static VirtualModelRepository<Soundset> Soundsets { get { return soundsetVirtualRepository; } }

        public static VirtualModelRepository<AttackBonusTable> AttackBonusTables { get { return babTableVirtualRepository; } }
        public static VirtualModelRepository<BonusFeatsTable> BonusFeatTables { get { return bonusFeatTableVirtualRepository; } }
        public static VirtualModelRepository<FeatsTable> FeatTables { get { return featsTableVirtualRepository; } }
        public static VirtualModelRepository<SavingThrowTable> SavingThrowTables { get { return savesTableVirtualRepository; } }
        public static VirtualModelRepository<SkillsTable> SkillTables { get { return skillsTableVirtualRepository; } }
        public static VirtualModelRepository<PrerequisiteTable> PrerequisiteTables { get { return requTableVirtualRepository; } }
        public static VirtualModelRepository<SpellSlotTable> SpellSlotTables { get { return spellSlotTableVirtualRepository; } }
        public static VirtualModelRepository<KnownSpellsTable> KnownSpellsTables { get { return knownSpellsTableVirtualRepository; } }
        public static VirtualModelRepository<StatGainTable> StatGainTables { get { return statGainTableVirtualRepository; } }
        public static VirtualModelRepository<RacialFeatsTable> RacialFeatsTables { get { return racialFeatsTableVirtualRepository; } }

        // Custom Datatypes
        public static VirtualModelRepository<CustomEnum> CustomEnums { get { return customEnumVirtualRepository; } }
        public static VirtualModelRepository<CustomObject> CustomObjects { get { return customObjectVirtualRepository; } }

        public static IEnumerable<DataTypeDefinition?> DataTypes
        {
            get
            {
                var enumTypes = CustomEnums.Select(ce => ce?.DataTypeDefinition);
                var objectTypes = CustomObjects.Select(co => co?.DataTypeDefinition);
                return defaultDataTypeList.Concat(enumTypes).Concat(objectTypes);
            }
        }

        public static void Clear()
        {
            Standard.Clear();
            Project.Clear();
        }

        public static void Load()
        {
            Standard.Races.LoadFromFile(Constants.RacesFilePath);
            Standard.Classes.LoadFromFile(Constants.ClassesFilePath);
            Standard.Domains.LoadFromFile(Constants.DomainsFilePath);
            Standard.Spells.LoadFromFile(Constants.SpellsFilePath);
            Standard.Feats.LoadFromFile(Constants.FeatsFilePath);
            Standard.Skills.LoadFromFile(Constants.SkillsFilePath);
            Standard.Diseases.LoadFromFile(Constants.DiseasesFilePath);
            Standard.Poisons.LoadFromFile(Constants.PoisonsFilePath);
            Standard.Spellbooks.LoadFromFile(Constants.SpellbooksFilePath);
            Standard.AreaEffects.LoadFromFile(Constants.AreaEffectsFilePath);

            Standard.Appearances.LoadFromFile(Constants.AppearancesFilePath);
            Standard.ClassPackages.LoadFromFile(Constants.ClassPackagesFilePath);
            Standard.Soundsets.LoadFromFile(Constants.SoundsetsFilePath);

            Standard.AttackBonusTables.LoadFromFile(Constants.AttackBonusTablesFilePath);
            Standard.BonusFeatTables.LoadFromFile(Constants.BonusFeatTablesFilePath);
            Standard.FeatTables.LoadFromFile(Constants.FeatTablesFilePath);
            Standard.SavingThrowTables.LoadFromFile(Constants.SavingThrowTablesFilePath);
            Standard.SkillTables.LoadFromFile(Constants.SkillTablesFilePath);
            Standard.PrerequisiteTables.LoadFromFile(Constants.PrerequisiteTablesFilePath);
            Standard.SpellSlotTables.LoadFromFile(Constants.SpellSlotTablesFilePath);
            Standard.KnownSpellsTables.LoadFromFile(Constants.KnownSpellsTablesFilePath);
            Standard.StatGainTables.LoadFromFile(Constants.StatGainTablesFilePath);
            Standard.RacialFeatsTables.LoadFromFile(Constants.RacialFeatsTablesFilePath);

            Standard.Races.ResolveReferences();
            Standard.Classes.ResolveReferences();
            Standard.Domains.ResolveReferences();
            Standard.Spells.ResolveReferences();
            Standard.Feats.ResolveReferences();
            Standard.Skills.ResolveReferences();
            Standard.Diseases.ResolveReferences();
            Standard.Poisons.ResolveReferences();
            Standard.Spellbooks.ResolveReferences();
            Standard.AreaEffects.ResolveReferences();

            Standard.Appearances.ResolveReferences();
            Standard.ClassPackages.ResolveReferences();
            Standard.Soundsets.ResolveReferences();

            Standard.AttackBonusTables.ResolveReferences();
            Standard.BonusFeatTables.ResolveReferences();
            Standard.FeatTables.ResolveReferences();
            Standard.SavingThrowTables.ResolveReferences();
            Standard.SkillTables.ResolveReferences();
            Standard.PrerequisiteTables.ResolveReferences();
            Standard.SpellSlotTables.ResolveReferences();
            Standard.KnownSpellsTables.ResolveReferences();
            Standard.StatGainTables.ResolveReferences();
            Standard.RacialFeatsTables.ResolveReferences();
        }
    }
}
