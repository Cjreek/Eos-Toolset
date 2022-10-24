using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class CustomObjectInstanceRepository : ModelRepository<CustomObjectInstance>
    {
        static CustomObjectInstanceRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<CustomObjectInstance>(typeof(CustomObjectInstanceRepository));
        }

        public CustomObjectInstanceRepository(bool isReadonly) : base(isReadonly)
        {
        }

        protected override int GetCustomDataStartIndex()
        {
            return 0;
        }
    }
}
