using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class ItemPropertyRepository : ModelRepository<ItemProperty>
    {
        static ItemPropertyRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<ItemProperty>(typeof(ItemPropertyRepository));
        }

        public ItemPropertyRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return MasterRepository.Project.Settings.ItemProperties.ExportOffset;
        }
    }
}
