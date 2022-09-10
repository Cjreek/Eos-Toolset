using Eos.Nwn;
using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Eos.Models
{
    public class BaseModel : INotifyPropertyChanged
    {
        public Guid ID { get; set; }
        public bool IsReadonly { get; set; } = false;
        public int? Index { get; set; }
        public String? Icon { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

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
            return new JsonObject();
        }

        public static T CreateFromJson<T>(JsonObject json) where T : BaseModel, new()
        {
            T result = new T();
            result.FromJson(json);
            return result;
        }

        public virtual void FromJson(JsonObject json)
        {
            
        }

        public static M? Resolve<M>(M? modelRef, VirtualModelRepository<M> repository) where M : BaseModel, new()
        {
            if (modelRef == null) return null;
            return repository.GetByID(modelRef.ID);
        }

        public virtual void ResolveReferences()
        {

        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
