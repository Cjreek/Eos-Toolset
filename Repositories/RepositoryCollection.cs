using Eos.Models;
using Eos.Models.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories
{
    public class RepositoryCollection
    {
        private readonly Dictionary<Type, IRepository> repositoryDict = new Dictionary<Type, IRepository>();

        private readonly ModelRepository<Race> raceRepository;
        private readonly ModelRepository<CharacterClass> classRepository;
        private readonly ModelRepository<Domain> domainRepository;
        private readonly SpellRepository spellRepository;
        private readonly FeatRepository featRepository;
        private readonly ModelRepository<Skill> skillRepository;
        private readonly ModelRepository<Disease> diseaseRepository;
        private readonly ModelRepository<Poison> poisonRepository;

        private readonly ModelRepository<AttackBonusTable> attackBonusTableRepository;
        private readonly ModelRepository<BonusFeatsTable> bonusFeatTableRepository;
        private readonly ModelRepository<FeatsTable> featTableRepository;
        private readonly ModelRepository<SavingThrowTable> savingThrowTableRepository;
        private readonly ModelRepository<SkillsTable> skillTableRepository;
        private readonly ModelRepository<PrerequisiteTable> prerequisiteTableRepository;
        private readonly ModelRepository<SpellSlotTable> spellSlotTableRepository;
        private readonly ModelRepository<KnownSpellsTable> knownSpellsTableRepository;

        public RepositoryCollection(bool isReadonly)
        {
            raceRepository = new ModelRepository<Race>(isReadonly);
            classRepository = new ModelRepository<CharacterClass>(isReadonly);
            domainRepository = new ModelRepository<Domain>(isReadonly);
            spellRepository = new SpellRepository(isReadonly);
            featRepository = new FeatRepository(isReadonly);
            skillRepository = new ModelRepository<Skill>(isReadonly);
            diseaseRepository = new ModelRepository<Disease>(isReadonly);
            poisonRepository = new ModelRepository<Poison>(isReadonly);

            attackBonusTableRepository = new ModelRepository<AttackBonusTable>(isReadonly); // Always writeable?
            bonusFeatTableRepository = new ModelRepository<BonusFeatsTable>(isReadonly); // Always writeable?
            featTableRepository = new ModelRepository<FeatsTable>(isReadonly); // Always writeable?
            savingThrowTableRepository = new ModelRepository<SavingThrowTable>(isReadonly); // Always writeable?
            skillTableRepository = new ModelRepository<SkillsTable>(isReadonly); // Always writeable?
            prerequisiteTableRepository = new ModelRepository<PrerequisiteTable>(isReadonly); // Always writeable?
            spellSlotTableRepository = new ModelRepository<SpellSlotTable>(isReadonly); // Always writeable?
            knownSpellsTableRepository = new ModelRepository<KnownSpellsTable>(isReadonly); // Always writeable?

            repositoryDict.Add(typeof(Race), raceRepository);
            repositoryDict.Add(typeof(CharacterClass), classRepository);
            repositoryDict.Add(typeof(Domain), domainRepository);
            repositoryDict.Add(typeof(Spell), spellRepository);
            repositoryDict.Add(typeof(Feat), featRepository);
            repositoryDict.Add(typeof(Skill), skillRepository);
            repositoryDict.Add(typeof(Disease), diseaseRepository);
            repositoryDict.Add(typeof(Poison), poisonRepository);

            repositoryDict.Add(typeof(AttackBonusTable), attackBonusTableRepository);
            repositoryDict.Add(typeof(BonusFeatsTable), bonusFeatTableRepository);
            repositoryDict.Add(typeof(FeatsTable), featTableRepository);
            repositoryDict.Add(typeof(SavingThrowTable), savingThrowTableRepository);
            repositoryDict.Add(typeof(SkillsTable), skillTableRepository);
            repositoryDict.Add(typeof(PrerequisiteTable), prerequisiteTableRepository);
            repositoryDict.Add(typeof(SpellSlotTable), prerequisiteTableRepository);
            repositoryDict.Add(typeof(KnownSpellsTable), knownSpellsTableRepository);
        }

        // Model Repositories
        public ModelRepository<Race> Races { get { return raceRepository; } }
        public ModelRepository<CharacterClass> Classes { get { return classRepository; } }
        public ModelRepository<Domain> Domains { get { return domainRepository; } }
        public SpellRepository Spells { get { return spellRepository; } }
        public FeatRepository Feats { get { return featRepository; } }
        public ModelRepository<Skill> Skills { get { return skillRepository; } }
        public ModelRepository<Disease> Diseases { get { return diseaseRepository; } }
        public ModelRepository<Poison> Poisons { get { return poisonRepository; } }

        public ModelRepository<AttackBonusTable> AttackBonusTables { get { return attackBonusTableRepository; } }
        public ModelRepository<BonusFeatsTable> BonusFeatTables { get { return bonusFeatTableRepository; } }
        public ModelRepository<FeatsTable> FeatTables { get { return featTableRepository; } }
        public ModelRepository<SavingThrowTable> SavingThrowTables { get { return savingThrowTableRepository; } }
        public ModelRepository<SkillsTable> SkillTables { get { return skillTableRepository; } }
        public ModelRepository<PrerequisiteTable> PrerequisiteTables { get { return prerequisiteTableRepository; } }
        public ModelRepository<SpellSlotTable> SpellSlotTables { get { return spellSlotTableRepository; } }
        public ModelRepository<KnownSpellsTable> KnownSpellsTables { get { return knownSpellsTableRepository; } }

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

            AttackBonusTables.Clear();
            BonusFeatTables.Clear();
            FeatTables.Clear();
            SavingThrowTables.Clear();
            SkillTables.Clear();
            PrerequisiteTables.Clear();
            SpellSlotTables.Clear();
            KnownSpellsTables.Clear();
        }
    }
}
