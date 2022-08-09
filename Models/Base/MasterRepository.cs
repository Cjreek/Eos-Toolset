using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models.Base
{
    internal static class MasterRepository
    {
        private static ModelRepository<Race> raceRepository = new ModelRepository<Race>();
        private static ModelRepository<CharacterClass> classRepository = new ModelRepository<CharacterClass>();
        private static ModelRepository<Domain> domainRepository = new ModelRepository<Domain>();
        private static ModelRepository<Spell> spellRepository = new ModelRepository<Spell>();
        private static ModelRepository<Feat> featRepository = new ModelRepository<Feat>();
        private static ModelRepository<Skill> skillRepository = new ModelRepository<Skill>();
        private static ModelRepository<Disease> diseaseRepository = new ModelRepository<Disease>();
        private static ModelRepository<Poison> poisonRepository = new ModelRepository<Poison>();

        // Model Repositories
        public static ModelRepository<Race> Races { get { return raceRepository; } }
        public static ModelRepository<CharacterClass> Classes { get { return classRepository; } }
        public static ModelRepository<Domain> Domains { get { return domainRepository; } }
        public static ModelRepository<Spell> Spells { get { return spellRepository; } }
        public static ModelRepository<Feat> Feats { get { return featRepository; } }
        public static ModelRepository<Skill> Skills { get { return skillRepository; } }
        public static ModelRepository<Disease> Diseases { get { return diseaseRepository; } }
        public static ModelRepository<Poison> Poisons { get { return poisonRepository; } }
    }
}
