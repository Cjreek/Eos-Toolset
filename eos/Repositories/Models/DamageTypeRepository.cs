using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    class DamageTypeRepository : ModelRepository<DamageType>
    {
        static DamageTypeRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<DamageType>(typeof(DamageTypeRepository));
        }

        public DamageTypeRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return MasterRepository.Project.Settings.DamageTypes.ExportOffset;
        }
    }
}
