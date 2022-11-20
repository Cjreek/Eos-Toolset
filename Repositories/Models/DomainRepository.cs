using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class DomainRepository : ModelRepository<Domain>
    {
        static DomainRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<Domain>(typeof(DomainRepository));
        }

        public DomainRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return 22;
        }
    }
}
