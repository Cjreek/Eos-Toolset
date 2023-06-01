using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class BaseItemRepository : ModelRepository<BaseItem>
    {
        static BaseItemRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<BaseItem>(typeof(BaseItemRepository));
        }

        public BaseItemRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return MasterRepository.Project.Settings.BaseItems.ExportOffset;
        }
    }
}
