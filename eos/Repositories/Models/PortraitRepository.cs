using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class PortraitRepository : ModelRepository<Portrait>
    {
        static PortraitRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<Portrait>(typeof(PortraitRepository));
        }

        public PortraitRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return MasterRepository.Project.Settings.Portraits.ExportOffset;
        }
    }
}
