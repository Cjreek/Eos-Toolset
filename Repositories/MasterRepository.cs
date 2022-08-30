using Eos.Models;
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

            Standard.Races.ResolveReferences();
            Standard.Classes.ResolveReferences();
            Standard.Domains.ResolveReferences();
            Standard.Spells.ResolveReferences();
            Standard.Feats.ResolveReferences();
            Standard.Skills.ResolveReferences();
            Standard.Diseases.ResolveReferences();
            Standard.Poisons.ResolveReferences();
        }
    }
}
