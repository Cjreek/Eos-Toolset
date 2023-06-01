using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class ItemPropertySetRepository : ModelRepository<ItemPropertySet>
    {
        static ItemPropertySetRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<ItemPropertySet>(typeof(ItemPropertySetRepository));
        }

        public ItemPropertySetRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return MasterRepository.Project.Settings.ItemPropertySets.ExportOffset;
        }
    }
}
