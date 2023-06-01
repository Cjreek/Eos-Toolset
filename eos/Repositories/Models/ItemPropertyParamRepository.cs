using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class ItemPropertyParamRepository : ModelRepository<ItemPropertyParam>
    {
        static ItemPropertyParamRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<ItemPropertyParam>(typeof(ItemPropertyParamRepository));
        }

        public ItemPropertyParamRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return MasterRepository.Project.Settings.ItemPropertyParams.ExportOffset;
        }
    }
}
