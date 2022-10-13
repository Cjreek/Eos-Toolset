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

        private readonly ModelRepository<Appearance> appearanceRepository;
        private readonly ModelRepository<ClassPackage> classPackageRepository;
        private readonly ModelRepository<Soundset> soundsetRepository;

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

        private void InitRepository<T>(out ModelRepository<T> repository, String propertyName) where T: BaseModel, new() 
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

            InitRepository(out appearanceRepository, nameof(Appearances));
            InitRepository(out classPackageRepository, nameof(ClassPackages));
            InitRepository(out soundsetRepository, nameof(Soundsets));

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

        public ModelRepository<Appearance> Appearances { get { return appearanceRepository; } }
        public ModelRepository<ClassPackage> ClassPackages { get { return classPackageRepository; } }
        public ModelRepository<Soundset> Soundsets { get { return soundsetRepository; } }

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

        public BaseModel New(Type modelType)
        {
            var constructor = modelType.GetConstructor(new Type[] { });
            if (constructor != null)
            {
                var newModel = (BaseModel)constructor.Invoke(new object[] { });
                repositoryDict[modelType].AddBase(newModel);
                return newModel;
            }
            else
                throw new Exception();
        }

        public void Add(BaseModel model)
        {
            var modelType = model.GetType();
            repositoryDict[modelType].AddBase(model);
        }

        public void Delete(BaseModel model)
        {
            var modelType = model.GetType();
            repositoryDict[modelType].RemoveBase(model);
        }

        public bool HasOverride(BaseModel model)
        {
            var modelType = model.GetType();
            return repositoryDict[modelType].HasOverride(model);
        }

        public BaseModel? GetOverride(BaseModel model)
        {
            var modelType = model.GetType();
            return repositoryDict[modelType].GetOverride(model);
        }

        public void Clear()
        {
            Races.Clear();
            Classes.Clear();
            Domains.Clear();
            Spells.Clear();
            Feats.Clear();
            Skills.Clear();
            Diseases.Clear();
            Poisons.Clear();
            Spellbooks.Clear();

            Appearances.Clear();
            ClassPackages.Clear();
            Soundsets.Clear();

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
