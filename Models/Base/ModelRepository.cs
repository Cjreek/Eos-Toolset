using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Eos.Models;

namespace Eos.Models.Base
{
    public class ModelRepository<T> : Repository<T> where T : BaseModel, new()
    {
        private Dictionary<Guid, T?> modelLookup = new Dictionary<Guid, T?>();
        private bool isReadonly;

        public ModelRepository(bool isReadonly)
        {
            this.isReadonly = isReadonly;
        }

        public T? GetByID(Guid id)
        {
            modelLookup.TryGetValue(id, out T? result);
            return result;
        }

        public bool Contains(Guid id)
        {
            return modelLookup.ContainsKey(id);
        }

        public override void Add(T model)
        {
            if (model.ID == Guid.Empty)
                model.ID = Guid.NewGuid();

            model.IsReadonly = isReadonly;

            if (!modelLookup.ContainsKey(model.ID))
            {
                base.Add(model);
                modelLookup.Add(model.ID, model);
            }
        }
    }
}
