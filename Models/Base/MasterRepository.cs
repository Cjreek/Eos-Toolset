using Eos.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models.Base
{
    internal class VirtualList<T> : IReadOnlyList<T?>
    {
        private readonly IReadOnlyList<T?>[] lists;

        public VirtualList(params IReadOnlyList<T?>[] list)
        {
            this.lists = list;
        }

        public T? this[int index]
        {
            get
            {
                int tmpIndex = index;
                for (int i=0; i < lists.Length; i++)
                {
                    if (tmpIndex >= lists[i].Count)
                        tmpIndex -= lists[i].Count;
                    else
                        return lists[i][tmpIndex];
                }

                throw new IndexOutOfRangeException();
            }
        }

        int IReadOnlyCollection<T?>.Count => lists.Sum(list => list.Count);

        public IEnumerator<T?> GetEnumerator()
        {
            return lists.SelectMany(list => list).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return lists.SelectMany(list => list).GetEnumerator();
        }
    }

    internal class MasterRepositoryCategory
    {
        private readonly ModelRepository<Race> raceRepository;
        private readonly ModelRepository<CharacterClass> classRepository;
        private readonly ModelRepository<Domain> domainRepository;
        private readonly ModelRepository<Spell> spellRepository;
        private readonly ModelRepository<Feat> featRepository;
        private readonly ModelRepository<Skill> skillRepository;
        private readonly ModelRepository<Disease> diseaseRepository;
        private readonly ModelRepository<Poison> poisonRepository;

        public MasterRepositoryCategory(bool isReadonly)
        {
            raceRepository = new ModelRepository<Race>(isReadonly);
            classRepository = new ModelRepository<CharacterClass>(isReadonly);
            domainRepository = new ModelRepository<Domain>(isReadonly);
            spellRepository = new ModelRepository<Spell>(isReadonly);
            featRepository = new ModelRepository<Feat>(isReadonly);
            skillRepository = new ModelRepository<Skill>(isReadonly);
            diseaseRepository = new ModelRepository<Disease>(isReadonly);
            poisonRepository = new ModelRepository<Poison>(isReadonly);
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
    }

    internal static class MasterRepository
    {
        private static readonly MasterRepositoryCategory standardCategory;
        private static readonly MasterRepositoryCategory customCategory;

        private static readonly VirtualList<Race> raceVirtualList;
        private static readonly VirtualList<CharacterClass> classVirtualList;
        private static readonly VirtualList<Domain> domainVirtualList;
        private static readonly VirtualList<Spell> spellVirtualList;
        private static readonly VirtualList<Feat> featVirtualList;
        private static readonly VirtualList<Skill> skillVirtualList;
        private static readonly VirtualList<Disease> diseaseVirtualList;
        private static readonly VirtualList<Poison> poisonVirtualList;

        static MasterRepository()
        {
            standardCategory = new MasterRepositoryCategory(true);
            customCategory = new MasterRepositoryCategory(false);

            raceVirtualList = new VirtualList<Race>(standardCategory.Races, customCategory.Races);
            classVirtualList = new VirtualList<CharacterClass>(standardCategory.Classes, customCategory.Classes);
            domainVirtualList = new VirtualList<Domain>(standardCategory.Domains, customCategory.Domains);
            spellVirtualList = new VirtualList<Spell>(standardCategory.Spells, customCategory.Spells);
            featVirtualList = new VirtualList<Feat>(standardCategory.Feats, customCategory.Feats);
            skillVirtualList = new VirtualList<Skill>(standardCategory.Skills, customCategory.Skills);
            diseaseVirtualList = new VirtualList<Disease>(standardCategory.Diseases, customCategory.Diseases);
            poisonVirtualList = new VirtualList<Poison>(standardCategory.Poisons, customCategory.Poisons);
        }

        public static MasterRepositoryCategory Standard { get { return standardCategory; } }
        public static MasterRepositoryCategory Custom { get { return customCategory; } }

        public static VirtualList<Race> Races { get { return raceVirtualList; } }
        public static VirtualList<CharacterClass> Classes { get { return classVirtualList; } }
        public static VirtualList<Domain> Domains { get { return domainVirtualList; } }
        public static VirtualList<Spell> Spells { get { return spellVirtualList; } }
        public static VirtualList<Feat> Feats { get { return featVirtualList; } }
        public static VirtualList<Skill> Skills { get { return skillVirtualList; } }
        public static VirtualList<Disease> Diseases { get { return diseaseVirtualList; } }
        public static VirtualList<Poison> Poisons { get { return poisonVirtualList; } }
    }
}
