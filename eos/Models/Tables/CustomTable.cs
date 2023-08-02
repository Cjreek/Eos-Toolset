using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Eos.Models.Tables
{
    public class CustomTable : BaseTable<CustomObjectProperty>
    {
        private DataTypeDefinition dataTypeDefinition = new DataTypeDefinition(Guid.Empty, String.Empty, null);

        public DataTypeDefinition DataTypeDefinition
        {
            get
            {
                if (dataTypeDefinition.ID != ID)
                {
                    dataTypeDefinition = new DataTypeDefinition(ID, Name, this, true);
                    dataTypeDefinition.ToJson = o => ((CustomTableInstance?)o)?.ToJsonRef();
                    dataTypeDefinition.To2DA = (o, lower, _) => lower ? ((CustomTableInstance?)o)?.Name?.ToLower() : ((CustomTableInstance?)o)?.Name;
                    dataTypeDefinition.FromJson = json => JsonUtils.CreateRefFromJson<CustomTableInstance>((JsonObject?)json);
                }
                return dataTypeDefinition;
            }
        }

        public ModelRepository<CustomTableInstance> InstanceRepository { get; set; }

        public event EventHandler? OnChanged;

        public String FileName { get; set; } = "";

        public CustomTable() : base()
        {
            InstanceRepository = MasterRepository.Project.CustomTableRepositories[this];
            InstanceRepository.CollectionChanged += InstanceRepository_CollectionChanged;
        }

        ~CustomTable()
        {
            InstanceRepository.CollectionChanged -= InstanceRepository_CollectionChanged;
        }

        private void InstanceRepository_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Delay, because otherwise bindings won't update immediately
            Task.Delay(5).ContinueWith(t => NotifyPropertyChanged(nameof(InstanceRepository)));

            // Suddenly works ??
            //if ((e.Action == NotifyCollectionChangedAction.Reset) || (e.Action == NotifyCollectionChangedAction.Move))
            //    NotifyPropertyChanged(nameof(InstanceRepository));
        }

        protected override void SetDefaultValues()
        {
            Name = "New Table";
        }

        public override JsonObject ToJson()
        {
            var json = base.ToJson();
            json.Add("FileName", FileName);
            return json;
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            FileName = json["FileName"]?.GetValue<String>() ?? "";
        }

        protected override void Changed()
        {
            OnChanged?.Invoke(this, new EventArgs());
        }
    }
}
