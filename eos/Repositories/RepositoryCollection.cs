using Eos.Models;
using Eos.Models.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories
{
    public class CustomObjectRepositoryCollection
    {
        private Dictionary<CustomObject, ModelRepository<CustomObjectInstance>> repositoryDict = new Dictionary<CustomObject, ModelRepository<CustomObjectInstance>>();

        public ModelRepository<CustomObjectInstance> this[CustomObject index] => AddRepository(index);

        public ModelRepository<CustomObjectInstance> AddRepository(CustomObject customObjectTemplate)
        {
            if (!repositoryDict.TryGetValue(customObjectTemplate, out var repo))
            {
                repo = RepositoryFactory.Create<CustomObjectInstance>(false);
                //repo.CollectionChanged += (_, _) => NotifyPropertyChanged(nameof(CustomObjectInstances)); // TODO: ?
                repositoryDict.Add(customObjectTemplate, repo);
            }

            return repo;
        }

        public void Clear()
        {
            foreach (var repo in repositoryDict.Values)
                repo.Clear();
            repositoryDict.Clear();
        }
    }

    public class CustomDynamicTableRepositoryCollection
    {
        private Dictionary<CustomDynamicTable, ModelRepository<CustomDynamicTableInstance>> repositoryDict = new Dictionary<CustomDynamicTable, ModelRepository<CustomDynamicTableInstance>>();

        public ModelRepository<CustomDynamicTableInstance> this[CustomDynamicTable index] => AddRepository(index);

        public ModelRepository<CustomDynamicTableInstance> AddRepository(CustomDynamicTable customDynamicTable)
        {
            if (!repositoryDict.TryGetValue(customDynamicTable, out var repo))
            {
                repo = RepositoryFactory.Create<CustomDynamicTableInstance>(false);
                //repo.CollectionChanged += (_, _) => NotifyPropertyChanged(nameof(CustomObjectInstances)); // TODO: ?
                repositoryDict.Add(customDynamicTable, repo);
            }

            return repo;
        }

        public void Clear()
        {
            foreach (var repo in repositoryDict.Values)
                repo.Clear();
            repositoryDict.Clear();
        }
    }

    public class RepositoryCollection : INotifyPropertyChanged
    {
        private readonly bool isReadonly;
        private readonly Dictionary<Type, IRepository> repositoryDict = new Dictionary<Type, IRepository>();

        private readonly ModelRepository<Race> raceRepository;
        private readonly ModelRepository<CharacterClass> classRepository;
        private readonly ModelRepository<Domain> domainRepository;
        private readonly ModelRepository<Spell> spellRepository;
        private readonly ModelRepository<Feat> featRepository;
        private readonly ModelRepository<Skill> skillRepository;
        private readonly ModelRepository<Disease> diseaseRepository;
        private readonly ModelRepository<Poison> poisonRepository;
        private readonly ModelRepository<Spellbook> spellbookRepository;
        private readonly ModelRepository<AreaEffect> aoeRepository;
        private readonly ModelRepository<MasterFeat> masterFeatRepository;
        private readonly ModelRepository<BaseItem> baseItemRepository;
        private readonly ModelRepository<ItemPropertySet> itemPropertySetRepository;
        private readonly ModelRepository<ItemProperty> itemPropertyRepository;

        private readonly ModelRepository<Appearance> appearanceRepository;
        private readonly ModelRepository<AppearanceSoundset> appearanceSoundsetRepository;
        private readonly ModelRepository<WeaponSound> weaponSoundRepository;
        private readonly ModelRepository<InventorySound> inventorySoundRepository;
        private readonly ModelRepository<Portrait> portraitRepository;
        private readonly ModelRepository<VisualEffect> vfxRepository;
        private readonly ModelRepository<ClassPackage> classPackageRepository;
        private readonly ModelRepository<Soundset> soundsetRepository;
        private readonly ModelRepository<Polymorph> polymorphRepository;
        private readonly ModelRepository<Companion> companionRepository;
        private readonly ModelRepository<Familiar> familiarRepository;
        private readonly ModelRepository<Trap> trapRepository;
        private readonly ModelRepository<ProgrammedEffect> progFXRepository;
        private readonly ModelRepository<DamageType> damageTypeRepository;
        private readonly ModelRepository<DamageTypeGroup> damageTypeGroupRepository;

        private readonly ModelRepository<AttackBonusTable> attackBonusTableRepository;
        private readonly ModelRepository<BonusFeatsTable> bonusFeatTableRepository;
        private readonly ModelRepository<FeatsTable> featTableRepository;
        private readonly ModelRepository<SavingThrowTable> savingThrowTableRepository;
        private readonly ModelRepository<SkillsTable> skillTableRepository;
        private readonly ModelRepository<PrerequisiteTable> prerequisiteTableRepository;
        private readonly ModelRepository<SpellSlotTable> spellSlotTableRepository;
        private readonly ModelRepository<KnownSpellsTable> knownSpellsTableRepository;
        private readonly ModelRepository<StatGainTable> statGainTableRepository;
        private readonly ModelRepository<RacialFeatsTable> racialFeatsTableRepository;

        private readonly ModelRepository<PackageSpellPreferencesTable> packageSpellPreferencesRepository;
        private readonly ModelRepository<PackageFeatPreferencesTable> packageFeatPreferencesRepository;
        private readonly ModelRepository<PackageSkillPreferencesTable> packageSkillPreferencesRepository;
        private readonly ModelRepository<PackageEquipmentTable> packageEquipmentRepository;

        private readonly ModelRepository<ItemPropertyTable> itemPropertyTableRepository;
        private readonly ModelRepository<ItemPropertyCostTable> itemPropertyCostTableRepository;
        private readonly ModelRepository<ItemPropertyParam> itemPropertyParamRepository;

        // Custom Datatypes
        private readonly ModelRepository<CustomEnum> customEnumRepository;
        private readonly ModelRepository<CustomObject> customObjectRepository;
        private readonly ModelRepository<CustomDynamicTable> customDynamicTableRepository;

        private readonly CustomObjectRepositoryCollection customObjectRepositoryCollection = new CustomObjectRepositoryCollection();
        private readonly CustomDynamicTableRepositoryCollection customDynamicTableRepositoryCollection = new CustomDynamicTableRepositoryCollection();

        private void InitRepository<T>(out ModelRepository<T> repository, string propertyName) where T : BaseModel, new()
        {
            repository = RepositoryFactory.Create<T>(isReadonly);
            repository.CollectionChanged += (_, _) => NotifyPropertyChanged(propertyName);
            repositoryDict.Add(typeof(T), repository);
        }

        public RepositoryCollection(bool isReadonly)
        {
            this.isReadonly = isReadonly;

            InitRepository(out raceRepository, nameof(Races));
            InitRepository(out classRepository, nameof(Classes));
            InitRepository(out domainRepository, nameof(Domains));
            InitRepository(out spellRepository, nameof(Spells));
            InitRepository(out featRepository, nameof(Feats));
            InitRepository(out skillRepository, nameof(Skills));
            InitRepository(out diseaseRepository, nameof(Diseases));
            InitRepository(out poisonRepository, nameof(Poisons));
            InitRepository(out spellbookRepository, nameof(Spellbooks));
            InitRepository(out aoeRepository, nameof(AreaEffects));
            InitRepository(out masterFeatRepository, nameof(MasterFeats));
            InitRepository(out baseItemRepository, nameof(BaseItems));
            InitRepository(out itemPropertySetRepository, nameof(ItemPropertySets));
            InitRepository(out itemPropertyRepository, nameof(ItemProperties));

            InitRepository(out appearanceRepository, nameof(Appearances));
            InitRepository(out appearanceSoundsetRepository, nameof(AppearanceSoundsets));
            InitRepository(out weaponSoundRepository, nameof(WeaponSounds));
            InitRepository(out inventorySoundRepository, nameof(InventorySounds));
            InitRepository(out portraitRepository, nameof(Portraits));
            InitRepository(out vfxRepository, nameof(VisualEffects));
            InitRepository(out classPackageRepository, nameof(ClassPackages));
            InitRepository(out soundsetRepository, nameof(Soundsets));
            InitRepository(out polymorphRepository, nameof(Polymorphs));
            InitRepository(out companionRepository, nameof(Companions));
            InitRepository(out familiarRepository, nameof(Familiars));
            InitRepository(out trapRepository, nameof(Traps));
            InitRepository(out progFXRepository, nameof(ProgrammedEffects));
            InitRepository(out damageTypeRepository, nameof(DamageTypes));
            InitRepository(out damageTypeGroupRepository, nameof(DamageTypeGroups));

            InitRepository(out attackBonusTableRepository, nameof(AttackBonusTables));
            InitRepository(out bonusFeatTableRepository, nameof(BonusFeatTables));
            InitRepository(out featTableRepository, nameof(FeatTables));
            InitRepository(out savingThrowTableRepository, nameof(SavingThrowTables));
            InitRepository(out skillTableRepository, nameof(SkillTables));
            InitRepository(out prerequisiteTableRepository, nameof(PrerequisiteTables));
            InitRepository(out spellSlotTableRepository, nameof(SpellSlotTables));
            InitRepository(out knownSpellsTableRepository, nameof(KnownSpellsTables));
            InitRepository(out statGainTableRepository, nameof(StatGainTables));
            InitRepository(out racialFeatsTableRepository, nameof(RacialFeatsTables));
            InitRepository(out packageSpellPreferencesRepository, nameof(SpellPreferencesTables));
            InitRepository(out packageFeatPreferencesRepository, nameof(FeatPreferencesTables));
            InitRepository(out packageSkillPreferencesRepository, nameof(SkillPreferencesTables));
            InitRepository(out packageEquipmentRepository, nameof(PackageEquipmentTables));

            InitRepository(out itemPropertyTableRepository, nameof(ItemPropertyTables));
            InitRepository(out itemPropertyCostTableRepository, nameof(ItemPropertyCostTables));
            InitRepository(out itemPropertyParamRepository, nameof(ItemPropertyParams));

            // Custom Datatypes
            InitRepository(out customEnumRepository, nameof(CustomEnums));
            InitRepository(out customObjectRepository, nameof(CustomObjects));
            InitRepository(out customDynamicTableRepository, nameof(CustomDynamicTables));
        }

        // Model Repositories
        public ModelRepository<Race> Races { get { return raceRepository; } }
        public ModelRepository<CharacterClass> Classes { get { return classRepository; } }
        public ModelRepository<Domain> Domains { get { return domainRepository; } }
        public ModelRepository<Spell> Spells { get { return spellRepository; } }
        public ModelRepository<Feat> Feats { get { return featRepository; } }
        public ModelRepository<Skill> Skills { get { return skillRepository; } }
        public ModelRepository<Disease> Diseases { get { return diseaseRepository; } }
        public ModelRepository<Poison> Poisons { get { return poisonRepository; } }
        public ModelRepository<Spellbook> Spellbooks { get { return spellbookRepository; } }
        public ModelRepository<AreaEffect> AreaEffects { get { return aoeRepository; } }
        public ModelRepository<MasterFeat> MasterFeats { get { return masterFeatRepository; } }
        public ModelRepository<BaseItem> BaseItems { get { return baseItemRepository; } }
        public ModelRepository<ItemPropertySet> ItemPropertySets { get { return itemPropertySetRepository; } }
        public ModelRepository<ItemProperty> ItemProperties { get { return itemPropertyRepository; } }

        public ModelRepository<Appearance> Appearances { get { return appearanceRepository; } }
        public ModelRepository<AppearanceSoundset> AppearanceSoundsets { get { return appearanceSoundsetRepository; } }
        public ModelRepository<WeaponSound> WeaponSounds { get { return weaponSoundRepository; } }
        public ModelRepository<InventorySound> InventorySounds { get { return inventorySoundRepository; } }
        public ModelRepository<Portrait> Portraits { get { return portraitRepository; } }
        public ModelRepository<VisualEffect> VisualEffects { get { return vfxRepository; } }
        public ModelRepository<ClassPackage> ClassPackages { get { return classPackageRepository; } }
        public ModelRepository<Soundset> Soundsets { get { return soundsetRepository; } }
        public ModelRepository<Polymorph> Polymorphs { get { return polymorphRepository; } }
        public ModelRepository<Companion> Companions { get { return companionRepository; } }
        public ModelRepository<Familiar> Familiars { get { return familiarRepository; } }
        public ModelRepository<Trap> Traps { get { return trapRepository; } }
        public ModelRepository<ProgrammedEffect> ProgrammedEffects { get { return progFXRepository; } }
        public ModelRepository<DamageType> DamageTypes { get { return damageTypeRepository; } }
        public ModelRepository<DamageTypeGroup> DamageTypeGroups { get { return damageTypeGroupRepository; } }
        public ModelRepository<AttackBonusTable> AttackBonusTables { get { return attackBonusTableRepository; } }
        public ModelRepository<BonusFeatsTable> BonusFeatTables { get { return bonusFeatTableRepository; } }
        public ModelRepository<FeatsTable> FeatTables { get { return featTableRepository; } }
        public ModelRepository<SavingThrowTable> SavingThrowTables { get { return savingThrowTableRepository; } }
        public ModelRepository<SkillsTable> SkillTables { get { return skillTableRepository; } }
        public ModelRepository<PrerequisiteTable> PrerequisiteTables { get { return prerequisiteTableRepository; } }
        public ModelRepository<SpellSlotTable> SpellSlotTables { get { return spellSlotTableRepository; } }
        public ModelRepository<KnownSpellsTable> KnownSpellsTables { get { return knownSpellsTableRepository; } }
        public ModelRepository<StatGainTable> StatGainTables { get { return statGainTableRepository; } }
        public ModelRepository<RacialFeatsTable> RacialFeatsTables { get { return racialFeatsTableRepository; } }
        public ModelRepository<PackageSpellPreferencesTable> SpellPreferencesTables { get { return packageSpellPreferencesRepository; } }
        public ModelRepository<PackageFeatPreferencesTable> FeatPreferencesTables { get { return packageFeatPreferencesRepository; } }
        public ModelRepository<PackageSkillPreferencesTable> SkillPreferencesTables { get { return packageSkillPreferencesRepository; } }
        public ModelRepository<PackageEquipmentTable> PackageEquipmentTables { get { return packageEquipmentRepository; } }
        public ModelRepository<ItemPropertyTable> ItemPropertyTables { get { return itemPropertyTableRepository; } }
        public ModelRepository<ItemPropertyCostTable> ItemPropertyCostTables { get { return itemPropertyCostTableRepository; } }
        public ModelRepository<ItemPropertyParam> ItemPropertyParams { get { return itemPropertyParamRepository; } }

        // Custom Datatypes
        public ModelRepository<CustomEnum> CustomEnums { get { return customEnumRepository; } }
        public ModelRepository<CustomObject> CustomObjects { get { return customObjectRepository; } }
        public ModelRepository<CustomDynamicTable> CustomDynamicTables { get { return customDynamicTableRepository; } }

        public CustomObjectRepositoryCollection CustomObjectRepositories { get { return customObjectRepositoryCollection; } }
        public CustomDynamicTableRepositoryCollection CustomDynamicTableRepositories { get { return customDynamicTableRepositoryCollection; } }

        public IEnumerable<IRepository> AllRepositories => repositoryDict.Values;

        public BaseModel New(Type modelType)
        {
            var constructor = modelType.GetConstructor(new Type[] { });
            if (constructor != null)
            {
                var newModel = (BaseModel)constructor.Invoke(new object[] { });
                newModel.Index = repositoryDict[modelType].GetNextFreeIndex();
                repositoryDict[modelType].AddBase(newModel);
                return newModel;
            }
            else
                throw new Exception();
        }

        public void Add(BaseModel model)
        {
            if (model is CustomDynamicTableInstance cdti)
            {
                if (cdti.Template != null)
                {
                    if (cdti.Index == null)
                        cdti.Index = CustomDynamicTableRepositories[cdti.Template].GetNextFreeIndex();
                    CustomDynamicTableRepositories[cdti.Template].Add(cdti);
                }
            }
            else if (model is CustomObjectInstance coi)
            {
                if (coi.Template != null)
                {
                    if (coi.Index == null)
                        coi.Index = CustomObjectRepositories[coi.Template].GetNextFreeIndex();
                    CustomObjectRepositories[coi.Template].Add(coi);
                }
            }
            else
            {
                var modelType = model.GetType();
                if (model.Index == null)
                    model.Index = repositoryDict[modelType].GetNextFreeIndex();
                repositoryDict[modelType].AddBase(model);
            }
        }

        public BaseModel? GetByID(Type modelType, Guid id)
        {
            return repositoryDict[modelType].GetBaseByID(id);
        }

        public BaseModel? GetByIndex(Type modelType, int index)
        {
            return repositoryDict[modelType].GetBaseByIndex(index);
        }

        public void Delete(BaseModel model)
        {
            if (model is CustomDynamicTableInstance cdti)
            {
                if (cdti.Template != null)
                    CustomDynamicTableRepositories[cdti.Template].Remove(cdti);
            }
            else if (model is CustomObjectInstance coi)
            {
                if (coi.Template != null)
                    CustomObjectRepositories[coi.Template].Remove(coi);
            }
            else
            {
                var modelType = model.GetType();
                repositoryDict[modelType].RemoveBase(model);
            }
        }

        public bool HasOverride(BaseModel model)
        {
            var modelType = model.GetType();
            if (repositoryDict.ContainsKey(modelType))
                return repositoryDict[modelType].HasOverride(model);
            return false;
        }

        public BaseModel? GetOverride(BaseModel? model)
        {
            if (model == null) return null;

            var modelType = model.GetType();
            return repositoryDict[modelType].GetOverride(model);
        }

        public int? GetBase2DAIndex(BaseModel model, bool returnCustomDataIndex = true)
        {
            if (model is CustomDynamicTableInstance cdti)
            {
                if (cdti.Template != null)
                    return CustomDynamicTableRepositories[cdti.Template].GetBase2DAIndex(model, returnCustomDataIndex);
                return -1;
            }
            else if (model is CustomObjectInstance coi)
            {
                if (coi.Template != null)
                    return CustomObjectRepositories[coi.Template].GetBase2DAIndex(model, returnCustomDataIndex);
                return -1;
            }
            else
            {
                var modelType = model.GetType();
                return repositoryDict[modelType].GetBase2DAIndex(model, returnCustomDataIndex);
            }
        }

        public void Clear()
        {
            Races.Clear();
            Classes.Clear();
            ClassPackages.Clear();
            Domains.Clear();
            Spells.Clear();
            Feats.Clear();
            Skills.Clear();
            Diseases.Clear();
            Poisons.Clear();
            Spellbooks.Clear();
            AreaEffects.Clear();
            MasterFeats.Clear();
            BaseItems.Clear();
            ItemPropertySets.Clear();
            ItemProperties.Clear();

            Appearances.Clear();
            AppearanceSoundsets.Clear();
            WeaponSounds.Clear();
            InventorySounds.Clear();
            Portraits.Clear();
            VisualEffects.Clear();
            Soundsets.Clear();
            Polymorphs.Clear();
            Companions.Clear();
            Familiars.Clear();
            Traps.Clear();
            ProgrammedEffects.Clear();
            DamageTypes.Clear();
            DamageTypeGroups.Clear();

            AttackBonusTables.Clear();
            BonusFeatTables.Clear();
            FeatTables.Clear();
            SavingThrowTables.Clear();
            SkillTables.Clear();
            PrerequisiteTables.Clear();
            SpellSlotTables.Clear();
            KnownSpellsTables.Clear();
            StatGainTables.Clear();
            RacialFeatsTables.Clear();
            SpellPreferencesTables.Clear();
            FeatPreferencesTables.Clear();
            SkillPreferencesTables.Clear();
            PackageEquipmentTables.Clear();

            ItemPropertyTables.Clear();
            ItemPropertyCostTables.Clear();
            ItemPropertyParams.Clear();

            // Custom Datatypes
            CustomEnums.Clear();
            CustomObjects.Clear();
            CustomDynamicTables.Clear();

            CustomObjectRepositories.Clear();
            CustomDynamicTableRepositories.Clear();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
