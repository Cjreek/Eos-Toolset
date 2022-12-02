using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class PolymorphRepository : ModelRepository<Polymorph>
    {
        static PolymorphRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<Polymorph>(typeof(PolymorphRepository));
        }

        public PolymorphRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return 107;
        }
    }
}
