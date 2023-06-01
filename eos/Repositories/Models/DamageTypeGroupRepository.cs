using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    class DamageTypeGroupRepository : ModelRepository<DamageTypeGroup>
    {
        static DamageTypeGroupRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<DamageTypeGroup>(typeof(DamageTypeGroupRepository));
        }

        public DamageTypeGroupRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return MasterRepository.Project.Settings.DamageTypeGroups.ExportOffset;
        }
    }
}
