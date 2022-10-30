using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class AreaEffectRepository : ModelRepository<AreaEffect>
    {
        static AreaEffectRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<AreaEffect>(typeof(AreaEffectRepository));
        }

        public AreaEffectRepository(bool isReadonly) : base(isReadonly)
        {
        }

        protected override int GetCustomDataStartIndex()
        {
            return 47;
        }
    }
}
