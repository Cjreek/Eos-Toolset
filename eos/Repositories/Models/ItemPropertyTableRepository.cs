using Eos.Models;
using Eos.Models.Tables;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class ItemPropertyTableRepository : ModelRepository<ItemPropertyTable>
    {
        private Dictionary<string, ItemPropertyTable> _tableNameLookup = new Dictionary<string, ItemPropertyTable>();

        static ItemPropertyTableRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<ItemPropertyTable>(typeof(ItemPropertyTableRepository));
        }

        public ItemPropertyTableRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public bool Contains(string name)
        {
            return _tableNameLookup.ContainsKey(name);
        }

        public override void Add(ItemPropertyTable? model)
        {
            if (model == null) return;

            if (!_tableNameLookup.ContainsKey(model.Name))
            {
                base.Add(model);
                _tableNameLookup.Add(model.Name, model);
            }
        }

        public override void Remove(ItemPropertyTable item)
        {
            base.Remove(item);
            _tableNameLookup.Remove(item.Name);
        }

        public override void Clear()
        {
            base.Clear();
            _tableNameLookup.Clear();
        }
    }
}
