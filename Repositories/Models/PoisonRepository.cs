using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class PoisonRepository : ModelRepository<Poison>
    {
        static PoisonRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<Poison>(typeof(PoisonRepository));
        }

        public PoisonRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return 45;
        }
    }
}
