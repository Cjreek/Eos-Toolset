using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Eos.Models;
using System.IO;
using System.Text.Json.Nodes;
using Eos.Models.Tables;
using System.Text.Json;

namespace Eos.Repositories
{
    public class ModelRepository<T> : Repository<T>, IRepository where T : BaseModel, new()
    {
        private Dictionary<Guid, T?> modelLookup = new Dictionary<Guid, T?>();
        private Dictionary<int, T?> modelIndexLookup = new Dictionary<int, T?>();
        private Dictionary<Guid, Guid> overrideLookup = new Dictionary<Guid, Guid>();
        private bool isReadonly;

        public ModelExtension Extensions { get; } = new ModelExtension() { Name = typeof(T).Name };

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

        public virtual int GetCustomDataStartIndex()
        {
            return -1;
        }

        public int? Get2DAIndex(T? model, bool returnCustomDataIndex = true)
        {
            if (model == null) return null;

            if (!model.IsReadonly)
            {
                if (returnCustomDataIndex && (model.Overrides == null))
                    return GetCustomDataStartIndex() + (model.Index ?? 0);
                else
                {
                    if (model.Overrides != null)
                        return MasterRepository.Standard.GetByID(model.GetType(), model.Overrides ?? Guid.Empty)?.Index;
                    else
                        return model.Index;
                }
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

        public override void Add(T? model)
        {
            if (model == null) return;

            if (model.ID == Guid.Empty)
                model.ID = Guid.NewGuid();

            model.Extensions = Extensions;
            model.IsReadonly = isReadonly;

            if (!modelLookup.ContainsKey(model.ID))
            {
                base.Add(model);

                if (model.Overrides != null)
                    overrideLookup.Add(model.Overrides ?? Guid.Empty, model.ID);

                modelLookup.Add(model.ID, model);
                if (model.Index != null && model.Index != -1)
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

        public int GetNextFreeIndex(int startIndex = 0)
        {
            while (modelIndexLookup.ContainsKey(startIndex)) startIndex++;
            return startIndex;
        }

        public void LoadFromFile(string filename)
        {
            Clear();
            if (File.Exists(filename))
            {
                var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                try
                {
                    if (JsonNode.Parse(fs) is JsonObject jsonRepository)
                    {
                        var extensions = jsonRepository["Extensions"]?.AsArray();
                        if (extensions != null)
                        {
                            for (int i = 0; i < extensions.Count; i++)
                            {
                                if (extensions[i] is JsonObject jsonObj)
                                {
                                    var prop = new CustomObjectProperty();
                                    prop.FromJson(jsonObj);
                                    prop.ResolveReferences();
                                    Extensions.Add(prop);
                                }
                            }
                        }

                        var items = jsonRepository["Items"]?.AsArray();
                        if (items != null)
                        {
                            for (int i = 0; i < items.Count; i++)
                            {
                                if (items[i] is JsonObject jsonObj)
                                {
                                    var newModel = BaseModel.CreateFromJson<T>(jsonObj, Extensions);
                                    Add(newModel);
                                }
                            }
                        }

                        int nextIndex = GetNextFreeIndex(0);
                        for (int i=0; i < internalList.Count; i++)
                        {
                            var model = internalList[i];
                            if (model != null && model.Index == null)
                            {
                                model.Index = nextIndex;
                                modelIndexLookup.Add(model.Index ?? 0, model);
                                nextIndex = GetNextFreeIndex(nextIndex);
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

        public void SaveToFile(string filename, bool writeIndented = false)
        {
            var jsonRepo = new JsonObject();

            var extensionArr = new JsonArray();
            foreach (var item in Extensions.Items)
            {
                if (item == null) continue;
                extensionArr.Add(item.ToJson());
            }
            jsonRepo.Add("Extensions", extensionArr);

            var jsonArr = new JsonArray();
            foreach (var entity in this)
            {
                if (entity != null)
                    jsonArr.Add(entity.ToJson());
            }
            jsonRepo.Add("Items", jsonArr);

            var serializeOptions = new JsonSerializerOptions();
            serializeOptions.WriteIndented = writeIndented;
            File.WriteAllText(filename, jsonRepo.ToJsonString(serializeOptions));
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

        public int? GetBase2DAIndex(BaseModel model, bool returnCustomDataIndex = true)
        {
            if (model is T specificModel)
                return Get2DAIndex(specificModel, returnCustomDataIndex);
            return null;
        }

        IEnumerable<BaseModel?> IRepository.GetItems()
        {
            return internalList.Cast<BaseModel>();
        }
    }
}