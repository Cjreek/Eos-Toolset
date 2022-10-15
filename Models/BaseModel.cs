using Eos.Nwn;
using Eos.Nwn.Tlk;
using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using static Eos.Models.JsonUtils;

namespace Eos.Models
{
    public class BaseModelReference
    {
        public BaseModel? ReferenceObject { get; set; }
        public String? ReferenceProperty { get; set; }
        public String? ReferenceName { get; set; }
    }

    public class BaseModel : INotifyPropertyChanged
    {
        private bool _clearingReferences = false;
        private Dictionary<(BaseModel refObject, String refProperty), BaseModelReference> referenceDict = new Dictionary<(BaseModel refObject, String refProperty), BaseModelReference>();

        public Guid ID { get; set; }
        public bool IsReadonly { get; set; } = false;
        public Guid? Overrides { get; set; } = null;
        public int? Index { get; set; }
        public String? Icon { get; set; }
        public IEnumerable<BaseModelReference> References => referenceDict.Values;
        public int ReferenceCount => referenceDict.Count;

        protected void Set<T>(ref T? reference, T? value, [CallerMemberName] String refProperty = "") where T: BaseModel
        {
            if (reference != value)
            {
                reference?.RemoveReference(this, refProperty);
                reference = value;
                reference?.AddReference(this, refProperty);
            }
        }

        public void AddReference(BaseModel refObject, [CallerMemberName] String refProperty = "")
        {
            referenceDict[(refObject, refProperty)] = new BaseModelReference()
            {
                ReferenceObject = refObject,
                ReferenceProperty = refProperty,
                ReferenceName = refObject.GetLabel(),
            };
        }

        public void RemoveReference(BaseModel refObject, [CallerMemberName] String refProperty = "")
        {
            referenceDict.Remove((refObject, refProperty));
        }

        public BaseModel()
        {
            SetDefaultValues();
        }

        protected virtual void SetDefaultValues()
        {

        }

        protected virtual String GetLabel()
        {
            return "";
        }

        protected JsonObject? CreateJsonRef(BaseModel? model)
        {
            if (model == null) return null;

            var result = new JsonObject();
            result.Add("ID", model.ID.ToString());
            result.Add("Label", model.GetLabel());
            return result;
        }

        public JsonObject? ToJsonRef()
        {
            return CreateJsonRef(this);
        }

        public virtual JsonObject ToJson()
        {
            var result = new JsonObject();
            result.Add("ID", this.ID.ToString());
            result.Add("Index", this.Index);
            result.Add("Overrides", this.Overrides != null ? this.Overrides.ToString() : null);
            return result;
        }

        public static T CreateFromJson<T>(JsonObject json) where T : BaseModel, new()
        {
            T result = new T();
            result.FromJson(json);
            return result;
        }

        public virtual void FromJson(JsonObject json)
        {
            this.ID = ParseGuid(json["ID"]?.GetValue<String>());
            this.Index = json["Index"]?.GetValue<int?>();
            this.Overrides = ParseNullableGuid(json["Overrides"]?.GetValue<String>());
        }

        public M? Resolve<M>(M? modelRef, VirtualModelRepository<M> repository) where M : BaseModel, new()
        {
            if (_clearingReferences) return null;
            return repository.Resolve(modelRef);
        }

        public virtual void ResolveReferences()
        {

        }

        public void ClearReferences()
        {
            _clearingReferences = true;
            try
            {
                ResolveReferences();
            }
            finally
            {
                _clearingReferences = false;
            }
        }

        public virtual BaseModel? Copy()
        {
            var result = (BaseModel?)this.GetType()?.GetConstructor(new Type[] { })?.Invoke(new object[] { });
            if (result != null)
            {
                result.FromJson(this.ToJson());
                result.ResolveReferences();
                result.ID = Guid.Empty;
                return result;
            }

            return null;
        }

        public virtual BaseModel? Override()
        {
            var result = Copy();
            if (result != null) 
                result.Overrides = this.ID;
            return result;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
