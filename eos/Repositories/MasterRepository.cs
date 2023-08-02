using Eos.Models;
using Eos.Models.Tables;
using Eos.Nwn.Bif;
using Eos.Nwn.Tlk;
using Eos.Nwn.TwoDimensionalArray;
using Eos.Repositories.Models;
using Eos.Services;
using Eos.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using ResourceReference = System.String;
using static Eos.Models.JsonUtils;

namespace Eos.Repositories
{
    public static class MasterRepository
    {
        private static readonly List<DataTypeDefinition> defaultDataTypeList = new List<DataTypeDefinition>();
        private static readonly Dictionary<Guid, DataTypeDefinition> dataTypeByIdDict = new Dictionary<Guid, DataTypeDefinition>();
        private static readonly Dictionary<Type, DataTypeDefinition> dataTypeByTypeDict = new Dictionary<Type, DataTypeDefinition>();

        private static readonly ResourceRepository resources;

        private static readonly RepositoryCollection standardCategory;
        private static readonly EosProject project;

        private static readonly VirtualModelRepository<Race> raceVirtualRepository;
        private static readonly VirtualModelRepository<CharacterClass> classVirtualRepository;
        private static readonly VirtualModelRepository<ClassPackage> classPackageVirtualRepository;
        private static readonly VirtualModelRepository<Domain> domainVirtualRepository;
        private static readonly VirtualModelRepository<Spell> spellVirtualRepository;
        private static readonly VirtualModelRepository<Feat> featVirtualRepository;
        private static readonly VirtualModelRepository<Skill> skillVirtualRepository;
        private static readonly VirtualModelRepository<Disease> diseaseVirtualRepository;
        private static readonly VirtualModelRepository<Poison> poisonVirtualRepository;
        private static readonly VirtualModelRepository<Spellbook> spellbookVirtualRepository;
        private static readonly VirtualModelRepository<AreaEffect> aoeVirtualRepository;
        private static readonly VirtualModelRepository<MasterFeat> masterFeatVirtualRepository;
        private static readonly VirtualModelRepository<BaseItem> baseItemVirtualRepository;
        private static readonly VirtualModelRepository<ItemPropertySet> itemPropertySetVirtualRepository;
        private static readonly VirtualModelRepository<ItemProperty> itemPropertyVirtualRepository;

        private static readonly VirtualModelRepository<Appearance> appearanceVirtualRepository;
        private static readonly VirtualModelRepository<AppearanceSoundset> appearanceSoundsetVirtualRepository;
        private static readonly VirtualModelRepository<WeaponSound> weaponSoundVirtualRepository;
        private static readonly VirtualModelRepository<InventorySound> inventorySoundVirtualRepository;
        private static readonly VirtualModelRepository<Portrait> portraitVirtualRepository;
        private static readonly VirtualModelRepository<VisualEffect> vfxVirtualRepository;
        private static readonly VirtualModelRepository<Soundset> soundsetVirtualRepository;
        private static readonly VirtualModelRepository<Polymorph> polymorphVirtualRepository;
        private static readonly VirtualModelRepository<Companion> companionVirtualRepository;
        private static readonly VirtualModelRepository<Familiar> familiarVirtualRepository;
        private static readonly VirtualModelRepository<Trap> trapVirtualRepository;
        private static readonly VirtualModelRepository<ProgrammedEffect> progFXVirtualRepository;
        private static readonly VirtualModelRepository<DamageType> damageTypeVirtualRepository;
        private static readonly VirtualModelRepository<DamageTypeGroup> damageTypeGroupVirtualRepository;

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
        private static readonly VirtualModelRepository<PackageSpellPreferencesTable> packageSpellPreferencesVirtualRepository;
        private static readonly VirtualModelRepository<PackageFeatPreferencesTable> packageFeatPreferencesVirtualRepository;
        private static readonly VirtualModelRepository<PackageSkillPreferencesTable> packageSkillPreferencesVirtualRepository;
        private static readonly VirtualModelRepository<PackageEquipmentTable> packageEquipmentVirtualRepository;
        private static readonly VirtualModelRepository<ItemPropertyTable> itemPropertyTableVirtualRepository;
        private static readonly VirtualModelRepository<ItemPropertyCostTable> itemPropertyCostTableVirtualRepository;
        private static readonly VirtualModelRepository<ItemPropertyParam> itemPropertyParamVirtualRepository;

        // Custom Datatypes
        private static readonly VirtualModelRepository<CustomEnum> customEnumVirtualRepository;
        private static readonly VirtualModelRepository<CustomObject> customObjectVirtualRepository;
        private static readonly VirtualModelRepository<CustomTable> customTableVirtualRepository;
        private static readonly VirtualModelRepository<CustomDynamicTable> customDynamicTableVirtualRepository;

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
            RepositoryFactory.RegisterRepositoryClass<Polymorph>(typeof(PolymorphRepository));
            RepositoryFactory.RegisterRepositoryClass<MasterFeat>(typeof(MasterFeatRepository));
            RepositoryFactory.RegisterRepositoryClass<Appearance>(typeof(AppearanceRepository));
            RepositoryFactory.RegisterRepositoryClass<AppearanceSoundset>(typeof(AppearanceSoundsetRepository));
            RepositoryFactory.RegisterRepositoryClass<WeaponSound>(typeof(WeaponSoundRepository));
            RepositoryFactory.RegisterRepositoryClass<InventorySound>(typeof(InventorySoundRepository));
            RepositoryFactory.RegisterRepositoryClass<BaseItem>(typeof(BaseItemRepository));
            RepositoryFactory.RegisterRepositoryClass<Companion>(typeof(CompanionRepository));
            RepositoryFactory.RegisterRepositoryClass<Familiar>(typeof(FamiliarRepository));
            RepositoryFactory.RegisterRepositoryClass<Trap>(typeof(TrapRepository));
            RepositoryFactory.RegisterRepositoryClass<Portrait>(typeof(PortraitRepository));
            RepositoryFactory.RegisterRepositoryClass<ItemProperty>(typeof(ItemPropertyRepository));
            RepositoryFactory.RegisterRepositoryClass<ItemPropertyParam>(typeof(ItemPropertyParamRepository));
            RepositoryFactory.RegisterRepositoryClass<ItemPropertyTable>(typeof(ItemPropertyTableRepository));
            RepositoryFactory.RegisterRepositoryClass<ItemPropertyCostTable>(typeof(ItemPropertyCostTableRepository));
            RepositoryFactory.RegisterRepositoryClass<ItemPropertySet>(typeof(ItemPropertySetRepository));
            RepositoryFactory.RegisterRepositoryClass<ProgrammedEffect>(typeof(ProgrammedEffectRepository));
            RepositoryFactory.RegisterRepositoryClass<VisualEffect>(typeof(VisualEffectRepository));
            RepositoryFactory.RegisterRepositoryClass<DamageType>(typeof(DamageTypeRepository));
            RepositoryFactory.RegisterRepositoryClass<DamageTypeGroup>(typeof(DamageTypeGroupRepository));

            resources = new ResourceRepository();

            standardCategory = new RepositoryCollection(true);
            project = new EosProject();

            raceVirtualRepository = new VirtualModelRepository<Race>(standardCategory.Races, project.Races);
            classVirtualRepository = new VirtualModelRepository<CharacterClass>(standardCategory.Classes, project.Classes);
            classPackageVirtualRepository = new VirtualModelRepository<ClassPackage>(standardCategory.ClassPackages, project.ClassPackages);
            domainVirtualRepository = new VirtualModelRepository<Domain>(standardCategory.Domains, project.Domains);
            spellVirtualRepository = new VirtualModelRepository<Spell>(standardCategory.Spells, project.Spells);
            featVirtualRepository = new VirtualModelRepository<Feat>(standardCategory.Feats, project.Feats);
            skillVirtualRepository = new VirtualModelRepository<Skill>(standardCategory.Skills, project.Skills);
            diseaseVirtualRepository = new VirtualModelRepository<Disease>(standardCategory.Diseases, project.Diseases);
            poisonVirtualRepository = new VirtualModelRepository<Poison>(standardCategory.Poisons, project.Poisons);
            spellbookVirtualRepository = new VirtualModelRepository<Spellbook>(standardCategory.Spellbooks, project.Spellbooks);
            aoeVirtualRepository = new VirtualModelRepository<AreaEffect>(standardCategory.AreaEffects, project.AreaEffects);
            masterFeatVirtualRepository = new VirtualModelRepository<MasterFeat>(standardCategory.MasterFeats, project.MasterFeats);
            baseItemVirtualRepository = new VirtualModelRepository<BaseItem>(standardCategory.BaseItems, project.BaseItems);
            itemPropertySetVirtualRepository = new VirtualModelRepository<ItemPropertySet>(standardCategory.ItemPropertySets, project.ItemPropertySets);
            itemPropertyVirtualRepository = new VirtualModelRepository<ItemProperty>(standardCategory.ItemProperties, project.ItemProperties);

            appearanceVirtualRepository = new VirtualModelRepository<Appearance>(standardCategory.Appearances, project.Appearances);
            appearanceSoundsetVirtualRepository = new VirtualModelRepository<AppearanceSoundset>(standardCategory.AppearanceSoundsets, project.AppearanceSoundsets);
            weaponSoundVirtualRepository = new VirtualModelRepository<WeaponSound>(standardCategory.WeaponSounds, project.WeaponSounds);
            inventorySoundVirtualRepository = new VirtualModelRepository<InventorySound>(standardCategory.InventorySounds, project.InventorySounds);
            portraitVirtualRepository = new VirtualModelRepository<Portrait>(standardCategory.Portraits, project.Portraits);
            vfxVirtualRepository = new VirtualModelRepository<VisualEffect>(standardCategory.VisualEffects, project.VisualEffects);
            soundsetVirtualRepository = new VirtualModelRepository<Soundset>(standardCategory.Soundsets, project.Soundsets);
            polymorphVirtualRepository = new VirtualModelRepository<Polymorph>(standardCategory.Polymorphs, project.Polymorphs);
            companionVirtualRepository = new VirtualModelRepository<Companion>(standardCategory.Companions, project.Companions);
            familiarVirtualRepository = new VirtualModelRepository<Familiar>(standardCategory.Familiars, project.Familiars);
            trapVirtualRepository = new VirtualModelRepository<Trap>(standardCategory.Traps, project.Traps);
            progFXVirtualRepository = new VirtualModelRepository<ProgrammedEffect>(standardCategory.ProgrammedEffects, project.ProgrammedEffects);
            damageTypeVirtualRepository = new VirtualModelRepository<DamageType>(standardCategory.DamageTypes, project.DamageTypes);
            damageTypeGroupVirtualRepository = new VirtualModelRepository<DamageTypeGroup>(standardCategory.DamageTypeGroups, project.DamageTypeGroups);

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
            packageSpellPreferencesVirtualRepository = new VirtualModelRepository<PackageSpellPreferencesTable>(standardCategory.SpellPreferencesTables, project.SpellPreferencesTables);
            packageFeatPreferencesVirtualRepository = new VirtualModelRepository<PackageFeatPreferencesTable>(standardCategory.FeatPreferencesTables, project.FeatPreferencesTables);
            packageSkillPreferencesVirtualRepository = new VirtualModelRepository<PackageSkillPreferencesTable>(standardCategory.SkillPreferencesTables, project.SkillPreferencesTables);
            packageEquipmentVirtualRepository = new VirtualModelRepository<PackageEquipmentTable>(standardCategory.PackageEquipmentTables, project.PackageEquipmentTables);

            itemPropertyTableVirtualRepository = new VirtualModelRepository<ItemPropertyTable>(standardCategory.ItemPropertyTables, project.ItemPropertyTables);
            itemPropertyCostTableVirtualRepository = new VirtualModelRepository<ItemPropertyCostTable>(standardCategory.ItemPropertyCostTables, project.ItemPropertyCostTables);
            itemPropertyParamVirtualRepository = new VirtualModelRepository<ItemPropertyParam>(standardCategory.ItemPropertyParams, project.ItemPropertyParams);

            // Custom Datatypes
            customEnumVirtualRepository = new VirtualModelRepository<CustomEnum>(standardCategory.CustomEnums, project.CustomEnums);
            customObjectVirtualRepository = new VirtualModelRepository<CustomObject>(standardCategory.CustomObjects, project.CustomObjects);
            customTableVirtualRepository = new VirtualModelRepository<CustomTable>(standardCategory.CustomTables, project.CustomTables);
            customDynamicTableVirtualRepository = new VirtualModelRepository<CustomDynamicTable>(standardCategory.CustomDynamicTables, project.CustomDynamicTables);

            InitDefaultDataTypes();
        }

        private static DataTypeDefinition AddDataType(Guid id, String label, object? type, DataTypeToJsonDelegate toJsonFunc, DataTypeFromJsonDelegate fromJsonFunc, DataTypeTo2DADelegate to2DAFunc, bool isCustom = false, bool isVisualOnly = false)
        {
            var dataType = new DataTypeDefinition(id, label, type, isCustom);
            dataType.ToJson = toJsonFunc;
            dataType.FromJson = fromJsonFunc;
            dataType.To2DA = to2DAFunc;
            dataType.IsVisualOnly = isVisualOnly;
            defaultDataTypeList.Add(dataType);
            dataTypeByIdDict.Add(id, dataType);
            if ((type is Type dataTypeType) && (!dataTypeByTypeDict.ContainsKey(dataTypeType)))
                dataTypeByTypeDict.Add(dataTypeType, dataType);

            return dataType;
        }

        private static void InitDefaultDataTypes()
        {
            DataTypeTo2DADelegate to2daIdentity = (o, _, _) => o;

            AddDataType(Guid.Parse("a136669b-e618-4be1-9a29-8b76f85c60be"), "None", null, o => null, json => null, to2daIdentity, false, true);

            AddDataType(Guid.Parse("0bd1d9ec-7909-4f25-bd98-fa034915c0fb"), "String", typeof(string), o => (string?)o, json => json?.GetValue<string>(), to2daIdentity);
            AddDataType(Guid.Parse("0212ec84-bd23-441a-8269-713a3d765cbe"), "Integer", typeof(int), o => { if (o is Decimal d) return Decimal.ToInt32(d); return (int?)o; }, json => json?.GetValue<int>(), to2daIdentity);
            AddDataType(Guid.Parse("7dd178ac-c87f-4849-a539-ad9bf2c95220"), "Float", typeof(double), o => { if (o is Decimal d) return Decimal.ToDouble(d); return (double?)o; }, json => json?.GetValue<double>(), to2daIdentity);
            AddDataType(Guid.Parse("bd7ee740-d4a5-41b7-bcf6-cf14923e78dc"), "Boolean", typeof(bool), o => (bool?)o ?? false, json => json?.GetValue<bool>() ?? false, to2daIdentity);

            AddDataType(Guid.Parse("e4897c44-4117-45d4-b3fc-37b82fd88247"), "Resource Reference", typeof(ResourceReference), o => (string?)o, json => json?.GetValue<string>(), to2daIdentity);
            var tlkDataType = AddDataType(Guid.Parse("aaae9a67-5b8b-4085-81f6-125fc8cf89a7"), "TLK String", typeof(TLKStringSet), o => ((TLKStringSet?)o)?.ToJson(),
            json =>
            {
                var tlkString = new TLKStringSet();
                tlkString.FromJson(json?.AsObject());
                return tlkString;
            },
            (o, _, tlk2Index) =>
            { 
                return (o is TLKStringSet tlk) ? tlk2Index(tlk) : null;
            });
            tlkDataType.GetDefaultValue = () => new TLKStringSet();

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
                (o, lower, tlk2Index) =>
                {
                    if ((o is VariantValue varValue) && (varValue.DataType?.To2DA != null))
                        return varValue.DataType?.To2DA(varValue.Value, lower, tlk2Index);
                    return null;
                });

            DataTypeToJsonDelegate modelToJson = o => ((BaseModel?)o)?.ToJsonRef();
            AddDataType(Guid.Parse("75a47130-999f-46b3-ac79-0e8e9ca48344"), "Race", typeof(Race), modelToJson, json => JsonUtils.CreateRefFromJson<Race>((JsonObject?)json), (o, _, _) => Project.Races.Get2DAIndex((Race?)o));
            AddDataType(Guid.Parse("8541fb58-1534-464d-baab-0384381cc506"), "Class", typeof(CharacterClass), modelToJson, json => JsonUtils.CreateRefFromJson<CharacterClass>((JsonObject?)json), (o, _, _) => Project.Classes.Get2DAIndex((CharacterClass?)o));
            AddDataType(Guid.Parse("3f2d92d3-2860-4b90-8f78-f8b2bb9c4820"), "Feat", typeof(Feat), modelToJson, json => JsonUtils.CreateRefFromJson<Feat>((JsonObject?)json), (o, _, _) => Project.Feats.Get2DAIndex((Feat?)o));
            AddDataType(Guid.Parse("ff714377-786a-4a29-9fd6-72bd04a0c968"), "Skill", typeof(Skill), modelToJson, json => JsonUtils.CreateRefFromJson<Skill>((JsonObject?)json), (o, _, _) => Project.Skills.Get2DAIndex((Skill?)o));
            AddDataType(Guid.Parse("6380f69e-92fb-4717-ab00-01a1060727fc"), "Domain", typeof(Domain), modelToJson, json => JsonUtils.CreateRefFromJson<Domain>((JsonObject?)json), (o, _, _) => Project.Domains.Get2DAIndex((Domain?)o));
            AddDataType(Guid.Parse("7649f1d6-cd21-4af0-abae-834c5898b75b"), "Spell", typeof(Spell), modelToJson, json => JsonUtils.CreateRefFromJson<Spell>((JsonObject?)json), (o, _, _) => Project.Spells.Get2DAIndex((Spell?)o));
            AddDataType(Guid.Parse("159e9a47-de78-435d-9047-d96847544883"), "Poison", typeof(Poison), modelToJson, json => JsonUtils.CreateRefFromJson<Poison>((JsonObject?)json), (o, _, _) => Project.Poisons.Get2DAIndex((Poison?)o));
            AddDataType(Guid.Parse("c241bc8c-0a05-477d-9cbd-3bb4e50d0bfb"), "Disease", typeof(Disease), modelToJson, json => JsonUtils.CreateRefFromJson<Disease>((JsonObject?)json), (o, _, _) => Project.Diseases.Get2DAIndex((Disease?)o));
            AddDataType(Guid.Parse("f4d8a469-32a6-4f68-bccd-7710ebb026d9"), "Area Effect", typeof(AreaEffect), modelToJson, json => JsonUtils.CreateRefFromJson<AreaEffect>((JsonObject?)json), (o, _, _) => Project.AreaEffects.Get2DAIndex((AreaEffect?)o));
            AddDataType(Guid.Parse("d84ec31c-5e6b-4373-a1c4-9d8fb4a501a7"), "Master Feat", typeof(MasterFeat), modelToJson, json => JsonUtils.CreateRefFromJson<MasterFeat>((JsonObject?)json), (o, _, _) => Project.MasterFeats.Get2DAIndex((MasterFeat?)o));
            AddDataType(Guid.Parse("e552fd8f-599d-4a5a-a536-a58d02e52276"), "Base Item", typeof(BaseItem), modelToJson, json => JsonUtils.CreateRefFromJson<BaseItem>((JsonObject?)json), (o, _, _) => Project.BaseItems.Get2DAIndex((BaseItem?)o));
            AddDataType(Guid.Parse("3bffa036-db5f-4946-ae66-ed4c31a29830"), "Class Package", typeof(ClassPackage), modelToJson, json => JsonUtils.CreateRefFromJson<ClassPackage>((JsonObject?)json), (o, _, _) => Project.ClassPackages.Get2DAIndex((ClassPackage?)o));
            AddDataType(Guid.Parse("f5f2f8d9-8b11-4e2a-9d7c-8bae3bb48856"), "Item Property", typeof(ItemProperty), modelToJson, json => JsonUtils.CreateRefFromJson<ItemProperty>((JsonObject?)json), (o, _, _) => Project.ItemProperties.Get2DAIndex((ItemProperty?)o));

            AddDataType(Guid.Parse("80b538dc-7e6a-40c7-830a-05bdac2fe3a4"), "Appearance", typeof(Appearance), modelToJson, json => JsonUtils.CreateRefFromJson<Appearance>((JsonObject?)json), (o, _, _) => Project.Appearances.Get2DAIndex((Appearance?)o));
            AddDataType(Guid.Parse("824174da-64bf-42fe-b026-c7816eb1625b"), "Appearance Soundset", typeof(AppearanceSoundset), modelToJson, json => JsonUtils.CreateRefFromJson<AppearanceSoundset>((JsonObject?)json), (o, _, _) => Project.AppearanceSoundsets.Get2DAIndex((AppearanceSoundset?)o));
            AddDataType(Guid.Parse("714cc45e-e3a0-44e5-9634-c460161fbc18"), "Weapon Sound", typeof(WeaponSound), modelToJson, json => JsonUtils.CreateRefFromJson<WeaponSound>((JsonObject?)json), (o, _, _) => Project.WeaponSounds.Get2DAIndex((WeaponSound?)o));
            AddDataType(Guid.Parse("f81539e2-3d66-476a-a2dc-919ad5fd8d67"), "Inventory Sound", typeof(InventorySound), modelToJson, json => JsonUtils.CreateRefFromJson<InventorySound>((JsonObject?)json), (o, _, _) => Project.InventorySounds.Get2DAIndex((InventorySound?)o));
            AddDataType(Guid.Parse("3744b4c4-12e6-40e0-a776-cf9866f268a0"), "Visual Effect", typeof(VisualEffect), modelToJson, json => JsonUtils.CreateRefFromJson<VisualEffect>((JsonObject?)json), (o, _, _) => Project.VisualEffects.Get2DAIndex((VisualEffect?)o));
            AddDataType(Guid.Parse("8fb225be-dca3-4dce-bd38-fbde34e6fce1"), "Soundset", typeof(Soundset), modelToJson, json => JsonUtils.CreateRefFromJson<Soundset>((JsonObject?)json), (o, _, _) => Project.Soundsets.Get2DAIndex((Soundset?)o));
            AddDataType(Guid.Parse("0de3877c-ec2d-4f05-86ee-7c7ef26d7df7"), "Polymorph", typeof(Polymorph), modelToJson, json => JsonUtils.CreateRefFromJson<Polymorph>((JsonObject?)json), (o, _, _) => Project.Polymorphs.Get2DAIndex((Polymorph?)o));
            AddDataType(Guid.Parse("3a5fa240-0931-43b1-afb7-b244b2ef9e61"), "Portrait", typeof(Portrait), modelToJson, json => JsonUtils.CreateRefFromJson<Portrait>((JsonObject?)json), (o, _, _) => Project.Portraits.Get2DAIndex((Portrait?)o));
            AddDataType(Guid.Parse("2d39f858-6f18-478c-858a-ac5fbdc6bdd0"), "Companion", typeof(Companion), modelToJson, json => JsonUtils.CreateRefFromJson<Companion>((JsonObject?)json), (o, _, _) => Project.Companions.Get2DAIndex((Companion?)o));
            AddDataType(Guid.Parse("0be9d01b-46cf-4bed-ac35-fbd6f227ef7c"), "Familiar", typeof(Familiar), modelToJson, json => JsonUtils.CreateRefFromJson<Familiar>((JsonObject?)json), (o, _, _) => Project.Familiars.Get2DAIndex((Familiar?)o));
            AddDataType(Guid.Parse("d2060f90-f9dc-4deb-8a28-af6c3fbb5990"), "Trap", typeof(Trap), modelToJson, json => JsonUtils.CreateRefFromJson<Trap>((JsonObject?)json), (o, _, _) => Project.Traps.Get2DAIndex((Trap?)o));
            AddDataType(Guid.Parse("c50c6360-4480-430e-9411-21eb03778d9e"), "Programmed Effect", typeof(ProgrammedEffect), modelToJson, json => JsonUtils.CreateRefFromJson<ProgrammedEffect>((JsonObject?)json), (o, _, _) => Project.ProgrammedEffects.Get2DAIndex((ProgrammedEffect?)o));
            AddDataType(Guid.Parse("1d68cd7c-500d-44c2-a948-b8e61d88958d"), "Damage Type", typeof(DamageType), modelToJson, json => JsonUtils.CreateRefFromJson<DamageType>((JsonObject?)json), (o, _, _) => Project.DamageTypes.Get2DAIndex((DamageType?)o));
            AddDataType(Guid.Parse("2f17a139-20e0-4c9e-b386-a62ede19d76b"), "Damage Type Group", typeof(DamageTypeGroup), modelToJson, json => JsonUtils.CreateRefFromJson<DamageTypeGroup>((JsonObject?)json), (o, _, _) => Project.DamageTypeGroups.Get2DAIndex((DamageTypeGroup?)o));

            //AddDataType(Guid.Parse("6049191e-3cf5-44e8-9c5d-dc3c45136a3e"), "Item Property Param", typeof(ItemPropertyParam), modelToJson, json => JsonUtils.CreateRefFromJson<ItemPropertyParam>((JsonObject?)json), o => Project.ItemPropertyParams.Get2DAIndex((ItemPropertyParam?)o));

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
            //AddDataType(Guid.Parse("db3078ca-0661-420e-86a1-cf55cfd446b3"), "Package Spell Preferences", typeof(PackageSpellPreferencesTable), modelToJson, json => JsonUtils.CreateRefFromJson<PackageSpellPreferencesTable>((JsonObject?)json), o => Project.SpellPreferencesTables.Get2DAIndex((PackageSpellPreferencesTable?)o));
            //AddDataType(Guid.Parse("14601e04-cb75-4598-8432-bf57b943f715"), "Package Feat Preferences", typeof(PackageFeatPreferencesTable), modelToJson, json => JsonUtils.CreateRefFromJson<PackageFeatPreferencesTable>((JsonObject?)json), o => Project.FeatPreferencesTables.Get2DAIndex((PackageFeatPreferencesTable?)o));
            //AddDataType(Guid.Parse("bf526a79-86a3-4eb1-9136-b1574a03d83a"), "Package Skill Preferences", typeof(PackageSkillPreferencesTable), modelToJson, json => JsonUtils.CreateRefFromJson<PackageSkillPreferencesTable>((JsonObject?)json), o => Project.SkillPreferencesTables.Get2DAIndex((PackageSkillPreferencesTable?)o));
            //AddDataType(Guid.Parse("63242a50-9bd6-4531-b362-27df2ec2bd86"), "Package Equipment Table", typeof(PackageEquipmentTable), modelToJson, json => JsonUtils.CreateRefFromJson<PackageEquipmentTable>((JsonObject?)json), o => Project.PackageEquipmentTables.Get2DAIndex((PackageEquipmentTable?)o));
            //AddDataType(Guid.Parse("12e64d02-ea3f-4b21-baf2-bfa84fb22fd7"), "ItemProperty Table", typeof(ItemPropertyTable), modelToJson, json => JsonUtils.CreateRefFromJson<ItemPropertyTable>((JsonObject?)json), o => Project.ItemPropertyTables.Get2DAIndex((ItemPropertyTable?)o));
            //AddDataType(Guid.Parse("8e32551b-9cf7-4c36-b3f3-7d62b1857769"), "ItemProperty Cost Table", typeof(ItemPropertyCostTable), modelToJson, json => JsonUtils.CreateRefFromJson<ItemPropertyCostTable>((JsonObject?)json), o => Project.ItemPropertyCostTables.Get2DAIndex((ItemPropertyCostTable?)o));
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

                var customTable = Project.CustomTables.GetByID(id);
                if (customTable != null)
                    return customTable.DataTypeDefinition;

                var customDynTable = Project.CustomDynamicTables.GetByID(id);
                if (customDynTable != null)
                    return customDynTable.DataTypeDefinition;
            }

            return result;
        }

        public static DataTypeDefinition? GetDataTypeByType(Type type)
        {
            if (!dataTypeByTypeDict.TryGetValue(type, out var result))
                return null;

            return result;
        }

        public static void Initialize(string nwnBasePath)
        {
            resources.Initialize(nwnBasePath);
        }

        public static void LoadExternalResources(IEnumerable<String> externalBasePath)
        {
            Log.Info("Loading external resources from: {0}", string.Join(", ", externalBasePath.Select(path => "\"" + path + "\"")));
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
        public static VirtualModelRepository<ClassPackage> ClassPackages { get { return classPackageVirtualRepository; } }
        public static VirtualModelRepository<Domain> Domains { get { return domainVirtualRepository; } }
        public static VirtualModelRepository<Spell> Spells { get { return spellVirtualRepository; } }
        public static VirtualModelRepository<Feat> Feats { get { return featVirtualRepository; } }
        public static VirtualModelRepository<Skill> Skills { get { return skillVirtualRepository; } }
        public static VirtualModelRepository<Disease> Diseases { get { return diseaseVirtualRepository; } }
        public static VirtualModelRepository<Poison> Poisons { get { return poisonVirtualRepository; } }
        public static VirtualModelRepository<Spellbook> Spellbooks { get { return spellbookVirtualRepository; } }
        public static VirtualModelRepository<AreaEffect> AreaEffects { get { return aoeVirtualRepository; } }
        public static VirtualModelRepository<MasterFeat> MasterFeats { get { return masterFeatVirtualRepository; } }
        public static VirtualModelRepository<BaseItem> BaseItems { get { return baseItemVirtualRepository; } }
        public static VirtualModelRepository<ItemPropertySet> ItemPropertySets { get { return itemPropertySetVirtualRepository; } }
        public static VirtualModelRepository<ItemProperty> ItemProperties { get { return itemPropertyVirtualRepository; } }

        public static VirtualModelRepository<Appearance> Appearances { get { return appearanceVirtualRepository; } }
        public static VirtualModelRepository<AppearanceSoundset> AppearanceSoundsets { get { return appearanceSoundsetVirtualRepository; } }
        public static VirtualModelRepository<WeaponSound> WeaponSounds { get { return weaponSoundVirtualRepository; } }
        public static VirtualModelRepository<InventorySound> InventorySounds { get { return inventorySoundVirtualRepository; } }
        public static VirtualModelRepository<Portrait> Portraits { get { return portraitVirtualRepository; } }
        public static VirtualModelRepository<VisualEffect> VisualEffects { get { return vfxVirtualRepository; } }
        public static VirtualModelRepository<Soundset> Soundsets { get { return soundsetVirtualRepository; } }
        public static VirtualModelRepository<Polymorph> Polymorphs { get { return polymorphVirtualRepository; } }
        public static VirtualModelRepository<Companion> Companions { get { return companionVirtualRepository; } }
        public static VirtualModelRepository<Familiar> Familiars { get { return familiarVirtualRepository; } }
        public static VirtualModelRepository<Trap> Traps { get { return trapVirtualRepository; } }
        public static VirtualModelRepository<ProgrammedEffect> ProgrammedEffects { get { return progFXVirtualRepository; } }
        public static VirtualModelRepository<DamageType> DamageTypes { get { return damageTypeVirtualRepository; } }
        public static VirtualModelRepository<DamageTypeGroup> DamageTypeGroups { get { return damageTypeGroupVirtualRepository; } }

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
        public static VirtualModelRepository<PackageSpellPreferencesTable> SpellPreferencesTables { get { return packageSpellPreferencesVirtualRepository; } }
        public static VirtualModelRepository<PackageFeatPreferencesTable> FeatPreferencesTables { get { return packageFeatPreferencesVirtualRepository; } }
        public static VirtualModelRepository<PackageSkillPreferencesTable> SkillPreferencesTables { get { return packageSkillPreferencesVirtualRepository; } }
        public static VirtualModelRepository<PackageEquipmentTable> PackageEquipmentTables { get { return packageEquipmentVirtualRepository; } }
        public static VirtualModelRepository<ItemPropertyTable> ItemPropertyTables { get { return itemPropertyTableVirtualRepository; } }
        public static VirtualModelRepository<ItemPropertyCostTable> ItemPropertyCostTables { get { return itemPropertyCostTableVirtualRepository; } }
        public static VirtualModelRepository<ItemPropertyParam> ItemPropertyParams { get { return itemPropertyParamVirtualRepository; } }

        // Custom Datatypes
        public static VirtualModelRepository<CustomEnum> CustomEnums { get { return customEnumVirtualRepository; } }
        public static VirtualModelRepository<CustomObject> CustomObjects { get { return customObjectVirtualRepository; } }
        public static VirtualModelRepository<CustomTable> CustomTables { get { return customTableVirtualRepository; } }
        public static VirtualModelRepository<CustomDynamicTable> CustomDynamicTables { get { return customDynamicTableVirtualRepository; } }

        public static IEnumerable<DataTypeDefinition?> DataTypes
        {
            get
            {
                var enumTypes = CustomEnums.Select(ce => ce?.DataTypeDefinition);
                var objectTypes = CustomObjects.Select(co => co?.DataTypeDefinition);
                var tableTypes = CustomTables.Select(ct => ct?.DataTypeDefinition);
                var dynTableTypes = CustomDynamicTables.Select(cdt => cdt?.DataTypeDefinition);
                return defaultDataTypeList.Concat(enumTypes).Concat(objectTypes).Concat(tableTypes).Concat(dynTableTypes);
            }
        }

        public static void Clear()
        {
            Standard.Clear();
            Project.Clear();
        }

        public static void Load()
        {
            Log.Info("Loading imported base game data...");
            try
            {
                Standard.Races.LoadFromFile(Constants.RacesFilePath);
                Standard.Classes.LoadFromFile(Constants.ClassesFilePath);
                Standard.ClassPackages.LoadFromFile(Constants.ClassPackagesFilePath);
                Standard.Domains.LoadFromFile(Constants.DomainsFilePath);
                Standard.Spells.LoadFromFile(Constants.SpellsFilePath);
                Standard.Feats.LoadFromFile(Constants.FeatsFilePath);
                Standard.Skills.LoadFromFile(Constants.SkillsFilePath);
                Standard.Diseases.LoadFromFile(Constants.DiseasesFilePath);
                Standard.Poisons.LoadFromFile(Constants.PoisonsFilePath);
                Standard.Spellbooks.LoadFromFile(Constants.SpellbooksFilePath);
                Standard.AreaEffects.LoadFromFile(Constants.AreaEffectsFilePath);
                Standard.MasterFeats.LoadFromFile(Constants.MasterFeatsFilePath);
                Standard.BaseItems.LoadFromFile(Constants.BaseItemsFilePath);
                Standard.ItemPropertySets.LoadFromFile(Constants.ItemPropertySetsFilePath);
                Standard.ItemProperties.LoadFromFile(Constants.ItemPropertiesFilePath);

                Standard.Appearances.LoadFromFile(Constants.AppearancesFilePath);
                Standard.AppearanceSoundsets.LoadFromFile(Constants.AppearanceSoundsetsFilePath);
                Standard.WeaponSounds.LoadFromFile(Constants.WeaponSoundsFilePath);
                Standard.InventorySounds.LoadFromFile(Constants.InventorySoundsFilePath);
                Standard.Portraits.LoadFromFile(Constants.PortraitsFilePath);
                Standard.VisualEffects.LoadFromFile(Constants.VisualEffectsFilePath);
                Standard.Soundsets.LoadFromFile(Constants.SoundsetsFilePath);
                Standard.Polymorphs.LoadFromFile(Constants.PolymorphsFilePath);
                Standard.Companions.LoadFromFile(Constants.CompanionsFilePath);
                Standard.Familiars.LoadFromFile(Constants.FamiliarsFilePath);
                Standard.Traps.LoadFromFile(Constants.TrapsFilePath);
                Standard.ProgrammedEffects.LoadFromFile(Constants.ProgrammedEffectsFilePath);
                Standard.DamageTypes.LoadFromFile(Constants.DamageTypesFilePath);
                Standard.DamageTypeGroups.LoadFromFile(Constants.DamageTypeGroupsFilePath);

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
                Standard.SpellPreferencesTables.LoadFromFile(Constants.SpellPreferencesTablesFilePath);
                Standard.FeatPreferencesTables.LoadFromFile(Constants.FeatPreferencesTablesFilePath);
                Standard.SkillPreferencesTables.LoadFromFile(Constants.SkillPreferencesTablesFilePath);
                Standard.PackageEquipmentTables.LoadFromFile(Constants.PackageEquipmentTablesFilePath);

                Standard.ItemPropertyTables.LoadFromFile(Constants.ItemPropertyTablesFilePath);
                Standard.ItemPropertyCostTables.LoadFromFile(Constants.ItemPropertyCostTablesFilePath);
                Standard.ItemPropertyParams.LoadFromFile(Constants.ItemPropertyParamsFilePath);

                Standard.Races.ResolveReferences();
                Standard.Classes.ResolveReferences();
                Standard.ClassPackages.ResolveReferences();
                Standard.Domains.ResolveReferences();
                Standard.Spells.ResolveReferences();
                Standard.Feats.ResolveReferences();
                Standard.Skills.ResolveReferences();
                Standard.Diseases.ResolveReferences();
                Standard.Poisons.ResolveReferences();
                Standard.Spellbooks.ResolveReferences();
                Standard.AreaEffects.ResolveReferences();
                Standard.MasterFeats.ResolveReferences();
                Standard.BaseItems.ResolveReferences();
                Standard.ItemPropertySets.ResolveReferences();
                Standard.ItemProperties.ResolveReferences();

                Standard.Appearances.ResolveReferences();
                Standard.AppearanceSoundsets.ResolveReferences();
                Standard.WeaponSounds.ResolveReferences();
                Standard.InventorySounds.ResolveReferences();
                Standard.Portraits.ResolveReferences();
                Standard.VisualEffects.ResolveReferences();
                Standard.Soundsets.ResolveReferences();
                Standard.Polymorphs.ResolveReferences();
                Standard.Companions.ResolveReferences();
                Standard.Familiars.ResolveReferences();
                Standard.Traps.ResolveReferences();
                Standard.ProgrammedEffects.ResolveReferences();
                Standard.DamageTypes.ResolveReferences();
                Standard.DamageTypeGroups.ResolveReferences();

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
                Standard.SpellPreferencesTables.ResolveReferences();
                Standard.FeatPreferencesTables.ResolveReferences();
                Standard.SkillPreferencesTables.ResolveReferences();
                Standard.PackageEquipmentTables.ResolveReferences();

                Standard.ItemPropertyTables.ResolveReferences();
                Standard.ItemPropertyCostTables.ResolveReferences();
                Standard.ItemPropertyParams.ResolveReferences();

                // Load Base Items Icons
                foreach (var baseItem in Standard.BaseItems)
                {
                    if ((baseItem == null) || (baseItem.Icon == null) || (baseItem?.Icon == "")) continue;
                    Resources.AddResource(NWNResourceSource.BIF, baseItem?.Icon, Nwn.NWNResourceType.TGA);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }

            Log.Info("Imported base game data loaded successfully!");
        }
    }
}
