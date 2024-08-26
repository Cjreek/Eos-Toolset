using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    class RangedDamageTypeRepository : ModelRepository<RangedDamageType>
    {
        static RangedDamageTypeRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<RangedDamageType>(typeof(RangedDamageTypeRepository));
        }

        public RangedDamageTypeRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return MasterRepository.Project.Settings.RangedDamageTypes.ExportOffset;
        }
    }
}
