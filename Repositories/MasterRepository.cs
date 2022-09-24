using Eos.Models;
using Eos.Models.Tables;
using Eos.Nwn.Bif;
using Eos.Nwn.Tlk;
using Eos.Nwn.TwoDimensionalArray;
using Eos.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories
{
    internal static class MasterRepository
    {
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

        static MasterRepository()
        {
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
        }

        public static void Initialize(String nwnBasePath)
        {
            resources.Initialize(nwnBasePath);
        }

        public static void Cleanup()
        {
            resources.Cleanup();
        }

        public static BaseModel New(Type modelType)
        {
            return Project.New(modelType);
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
