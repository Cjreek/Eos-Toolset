using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{                
    public class AppearanceSoundsetRepository : ModelRepository<AppearanceSoundset>
    {
        static AppearanceSoundsetRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<AppearanceSoundset>(typeof(AppearanceSoundsetRepository));
        }

        public AppearanceSoundsetRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return MasterRepository.Project.Settings.AppearanceSoundsets.ExportOffset;
        }
    }
}
