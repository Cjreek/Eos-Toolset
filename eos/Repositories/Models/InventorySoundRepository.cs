using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class InventorySoundRepository : ModelRepository<InventorySound>
    {
        static InventorySoundRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<InventorySound>(typeof(InventorySoundRepository));
        }

        public InventorySoundRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return MasterRepository.Project.Settings.InventorySounds.ExportOffset;
        }
    }
}
