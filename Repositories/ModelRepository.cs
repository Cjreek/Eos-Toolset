using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Eos.Models;
using System.IO;
using System.Text.Json.Nodes;

namespace Eos.Repositories
{
    public class ModelRepository<T> : Repository<T>, IRepository where T : BaseModel, new()
    {
        private Dictionary<Guid, T?> modelLookup = new Dictionary<Guid, T?>();
        private Dictionary<int, T?> modelIndexLookup = new Dictionary<int, T?>();
        private bool isReadonly;

        public ModelRepository(bool isReadonly) : base()
        {
            this.isReadonly = isReadonly;
        }

        public T? GetByID(Guid id)
        {
            modelLookup.TryGetValue(id, out T? result);
            return result;
        }

        public T? GetByIndex(int index)
        {
            modelIndexLookup.TryGetValue(index, out T? result);
            return result;
        }

        public bool Contains(Guid id)
        {
            return modelLookup.ContainsKey(id);
        }

        public bool Contains(int index)
        {
            return modelIndexLookup.ContainsKey(index);
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
                if (model.Index != null)
                    modelIndexLookup.Add(model.Index ?? 0, model);
            }
        }

        public override void Clear()
        {
            base.Clear();
            modelLookup.Clear();
        }

        public void Sort<U>(Func<T?,U> compareFunc)
        {
            var list = internalList.OrderBy(compareFunc).ToList();

            internalList.Clear();
            foreach (var item in list)
                internalList.Add(item);
        }

        public void LoadFromFile(String filename)
        {
            Clear();

            if (File.Exists(filename))
            {
                var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                try
                {
                    if (JsonNode.Parse(fs) is JsonArray jsonRepository)
                    {
                        for (int i = 0; i < jsonRepository.Count; i++)
                        {
                            if (jsonRepository[i] is JsonObject jsonObj)
                            {
                                var newModel = BaseModel.CreateFromJson<T>(jsonObj);
                                Add(newModel);
                            }
                        }
                    }
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        public void SaveToFile(String filename)
        {
            var jsonArr = new JsonArray();
            foreach (var entity in this)
            {
                if (entity != null)
                    jsonArr.Add(entity.ToJson());
            }
            File.WriteAllText(filename, jsonArr.ToJsonString());
        }

        public void ResolveReferences()
        {
            foreach (var item in internalList)
                item?.ResolveReferences();
        }

        public void AddBase(BaseModel model)
        {
            if (model is T notNullModel)
                Add((T)notNullModel);
            else
                throw new InvalidCastException();
        }
    }
}
