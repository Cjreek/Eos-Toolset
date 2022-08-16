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
    public class VirtualModelRepository<T> : IReadOnlyList<T?> where T : BaseModel, new()
    {
        private readonly ModelRepository<T>[] repositories;

        public VirtualModelRepository(params ModelRepository<T>[] repositories)
        {
            this.repositories = repositories;
        }

        public T? this[int index]
        {
            get
            {
                int tmpIndex = index;
                for (int i = 0; i < repositories.Length; i++)
                {
                    if (tmpIndex >= repositories[i].Count)
                        tmpIndex -= repositories[i].Count;
                    else
                        return repositories[i][tmpIndex];
                }

                throw new IndexOutOfRangeException();
            }
        }

        int IReadOnlyCollection<T?>.Count => repositories.Sum(list => list.Count);

        public IEnumerator<T?> GetEnumerator()
        {
            return repositories.SelectMany(list => list).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return repositories.SelectMany(list => list).GetEnumerator();
        }

        public T? GetByID(Guid id)
        {
            T? result = null;
            foreach (var repo in repositories)
            {
                result = repo.GetByID(id);
                if (result != null) break;
            }

            return result;
        }

        public T? GetByIndex(int index)
        {
            T? result = null;
            foreach (var repo in repositories)
            {
                result = repo.GetByIndex(index);
                if (result != null) break;
            }

            return result;
        }

        public bool Contains(Guid id)
        {
            var result = false;
            foreach (var repo in repositories)
            {
                result = repo.Contains(id);
                if (result) break;
            }

            return result;
        }

        public bool Contains(int index)
        {
            var result = false;
            foreach (var repo in repositories)
            {
                result = repo.Contains(index);
                if (result) break;
            }

            return result;
        }
    }

    internal class MasterRepositoryCategory
    {
        private readonly ModelRepository<Race> raceRepository;
        private readonly ModelRepository<CharacterClass> classRepository;
        private readonly ModelRepository<Domain> domainRepository;
        private readonly SpellRepository spellRepository;
        private readonly FeatRepository featRepository;
        private readonly ModelRepository<Skill> skillRepository;
        private readonly ModelRepository<Disease> diseaseRepository;
        private readonly ModelRepository<Poison> poisonRepository;

        public MasterRepositoryCategory(bool isReadonly)
        {
            raceRepository = new ModelRepository<Race>(isReadonly);
            classRepository = new ModelRepository<CharacterClass>(isReadonly);
            domainRepository = new ModelRepository<Domain>(isReadonly);
            spellRepository = new SpellRepository(isReadonly);
            featRepository = new FeatRepository(isReadonly);
            skillRepository = new ModelRepository<Skill>(isReadonly);
            diseaseRepository = new ModelRepository<Disease>(isReadonly);
            poisonRepository = new ModelRepository<Poison>(isReadonly);
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
        }
    }

    internal static class MasterRepository
    {
        private static readonly ResourceRepository resources;

        private static readonly MasterRepositoryCategory standardCategory;
        private static readonly MasterRepositoryCategory customCategory;

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

            standardCategory = new MasterRepositoryCategory(true);
            customCategory = new MasterRepositoryCategory(false);

            raceVirtualRepository = new VirtualModelRepository<Race>(standardCategory.Races, customCategory.Races);
            classVirtualRepository = new VirtualModelRepository<CharacterClass>(standardCategory.Classes, customCategory.Classes);
            domainVirtualRepository = new VirtualModelRepository<Domain>(standardCategory.Domains, customCategory.Domains);
            spellVirtualRepository = new VirtualModelRepository<Spell>(standardCategory.Spells, customCategory.Spells);
            featVirtualRepository = new VirtualModelRepository<Feat>(standardCategory.Feats, customCategory.Feats);
            skillVirtualRepository = new VirtualModelRepository<Skill>(standardCategory.Skills, customCategory.Skills);
            diseaseVirtualRepository = new VirtualModelRepository<Disease>(standardCategory.Diseases, customCategory.Diseases);
            poisonVirtualRepository = new VirtualModelRepository<Poison>(standardCategory.Poisons, customCategory.Poisons);
        }

        public static void Initialize(String nwnBasePath)
        {
            resources.Initialize(nwnBasePath);
        }

        public static void Cleanup()
        {
            resources.Cleanup();
        }

        public static ResourceRepository Resources { get { return resources; } }

        public static MasterRepositoryCategory Standard { get { return standardCategory; } }
        public static MasterRepositoryCategory Custom { get { return customCategory; } }

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
            Custom.Clear();
        }

        public static void Load()
        {
            Standard.Races.LoadFromFile(Constants.RacesFile);
            Standard.Classes.LoadFromFile(Constants.ClassesFile);
            Standard.Domains.LoadFromFile(Constants.DomainsFile);
            Standard.Spells.LoadFromFile(Constants.SpellsFile);
            Standard.Feats.LoadFromFile(Constants.FeatsFile);
            Standard.Skills.LoadFromFile(Constants.SkillsFile);
            Standard.Diseases.LoadFromFile(Constants.DiseasesFile);
            Standard.Poisons.LoadFromFile(Constants.PoisonsFile);

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
