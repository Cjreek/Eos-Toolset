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
        private Dictionary<Guid, Guid> overrideLookup = new Dictionary<Guid, Guid>();
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

        protected virtual int GetCustomDataStartIndex()
        {
            throw new NotImplementedException();
        }

        public int? Get2DAIndex(T? model, bool returnCustomDataIndex = true)
        {
            if (model == null) return null;

            if ((model.Index ?? -1) < 0)
            {
                var result = -1;
                for (int i = 0; i < internalList.Count; i++)
                {
                    if (internalList[i]?.Overrides == null)
                        result++;

                    if (internalList[i] == model)
                        break;
                }

                if (returnCustomDataIndex)
                    return GetCustomDataStartIndex() + result;
                else
                    return result;
            }

            return model.Index;
        }

        public bool HasOverride(BaseModel model)
        {
            return overrideLookup.ContainsKey(model.ID);
        }

        public BaseModel? GetOverride(BaseModel model)
        {
            if (overrideLookup.ContainsKey(model.ID))
                return GetByID(overrideLookup[model.ID]);

            return null;
        }

        public override void Add(T model)
        {
            if (model.ID == Guid.Empty)
                model.ID = Guid.NewGuid();

            model.IsReadonly = isReadonly;

            if (!modelLookup.ContainsKey(model.ID))
            {
                base.Add(model);

                if (model.Overrides != null)
                    overrideLookup.Add(model.Overrides ?? Guid.Empty, model.ID);

                modelLookup.Add(model.ID, model);
                if (model.Index != null)
                    modelIndexLookup.Add(model.Index ?? 0, model);
            }
        }

        public override void Clear()
        {
            base.Clear();
            modelLookup.Clear();
            modelIndexLookup.Clear();
            overrideLookup.Clear();
        }

        public override void Remove(T item)
        {
            if (item.Index != null && modelIndexLookup.ContainsKey(item.Index ?? 0))
                modelIndexLookup.Remove(item.Index ?? 0);
            if (modelLookup.ContainsKey(item.ID))
                modelLookup.Remove(item.ID);
            if (overrideLookup.ContainsKey(item.Overrides ?? Guid.Empty))
                overrideLookup.Remove(item.Overrides ?? Guid.Empty);

            item.ClearReferences();
            base.Remove(item);

            // Set all remaining references to null
            foreach (var reference in item.References)
                reference.ReferenceObject?.ResolveReferences();
        }

        public void LoadFromFile(string filename)
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

        public void SaveToFile(string filename)
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
                Add(notNullModel);
            else
                throw new InvalidCastException();
        }

        public void RemoveBase(BaseModel model)
        {
            if (model is T specificModel)
                Remove(specificModel);
        }

        public BaseModel? GetBaseByID(Guid id)
        {
            modelLookup.TryGetValue(id, out T? result);
            return result;
        }
    }
}
