using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class DiseaseRepository : ModelRepository<Disease>
    {
        static DiseaseRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<Disease>(typeof(DiseaseRepository));
        }

        public DiseaseRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return 17;
        }
    }
}
