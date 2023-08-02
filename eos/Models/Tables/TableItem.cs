using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class TableItem : INotifyPropertyChanged
    {
        public BaseModel? ParentTable { get; set; }
        public string SourceLabel { get; set; } = "";
        public event PropertyChangedEventHandler? PropertyChanged;

        public TableItem()
        {

        }

        public TableItem(BaseModel? parentTable)
        {
            ParentTable = parentTable;
        }

        public virtual void ResolveReferences()
        {

        }

        public virtual void FromJson(JsonObject json)
        {
            this.SourceLabel = json["SourceLabel"]?.GetValue<string>() ?? "";
        }

        public virtual JsonObject ToJson()
        {
            var item = new JsonObject();
            if (this.SourceLabel != "")
                item.Add("SourceLabel", this.SourceLabel);

            return item;
        }

        protected void Set<T>(ref T? reference, T? value, [CallerMemberName] String refProperty = "") where T : BaseModel
        {
            if ((reference != value) && (ParentTable != null))
            {
                reference?.RemoveReference(ParentTable, refProperty);
                reference = value;
                reference?.AddReference(ParentTable, refProperty);
                NotifyPropertyChanged(refProperty);
            }
        }

        public virtual bool IsValid()
        {
            return true;
        }

        protected BaseModel? ResolveByType(Type modelType, Guid id)
        {
            return MasterRepository.Standard.GetByID(modelType, id) ?? MasterRepository.Project.GetByID(modelType, id);
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "") // TODOX: protected // !
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
