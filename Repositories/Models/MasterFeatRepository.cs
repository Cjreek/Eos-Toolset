using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    class MasterFeatRepository : ModelRepository<MasterFeat>
    {
        static MasterFeatRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<MasterFeat>(typeof(MasterFeatRepository));
        }

        public MasterFeatRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return MasterRepository.Project.Settings.MasterFeats.ExportOffset;
        }
    }
}
