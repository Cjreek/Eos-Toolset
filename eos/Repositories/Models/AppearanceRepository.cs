using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class AppearanceRepository : ModelRepository<Appearance>
    {
        static AppearanceRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<Appearance>(typeof(AppearanceRepository));
        }

        public AppearanceRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return MasterRepository.Project.Settings.Appearances.ExportOffset;
        }
    }
}
