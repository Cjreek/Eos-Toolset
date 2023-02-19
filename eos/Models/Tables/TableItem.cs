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
        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual void ResolveReferences()
        {

        }

        public virtual void FromJson(JsonObject json)
        {

        }

        public virtual JsonObject ToJson()
        {
            return new JsonObject();
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

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
