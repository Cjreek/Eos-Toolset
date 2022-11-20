using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class SkillRepository : ModelRepository<Skill>
    {
        static SkillRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<Skill>(typeof(SkillRepository));
        }

        public SkillRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return 28;
        }
    }
}
