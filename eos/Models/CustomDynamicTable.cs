using Eos.Models.Tables;
using Eos.Nwn.Tlk;
using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Eos.Models
{
    public class CustomDynamicTable : BaseModel
    {
        private string _name = "";
        private DataTypeDefinition dataTypeDefinition = new DataTypeDefinition(Guid.Empty, String.Empty, null);
        public DataTypeDefinition DataTypeDefinition
        {
            get
            {
                if (dataTypeDefinition.ID != ID)
                {
                    dataTypeDefinition = new DataTypeDefinition(ID, Name, this, true);
                    dataTypeDefinition.ToJson = o => ((CustomDynamicTableInstance?)o)?.ToJsonRef();
                    dataTypeDefinition.To2DA = (o, lower, _) => lower ? ((CustomDynamicTableInstance?)o)?.Name?.ToLower() : ((CustomDynamicTableInstance?)o)?.Name;
                    dataTypeDefinition.FromJson = json => JsonUtils.CreateRefFromJson<CustomDynamicTableInstance>((JsonObject?)json);
                }
                return dataTypeDefinition;
            }
        }

        public ModelRepository<CustomDynamicTableInstance> InstanceRepository { get; set; }

        public CustomDynamicTable() : base()
        {
            InstanceRepository = MasterRepository.Project.CustomDynamicTableRepositories[this];
            InstanceRepository.CollectionChanged += InstanceRepository_CollectionChanged;
        }

        ~CustomDynamicTable()
        {
            InstanceRepository.CollectionChanged -= InstanceRepository_CollectionChanged;
        }

        private void InstanceRepository_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Suddenly works ??
            //if ((e.Action == NotifyCollectionChangedAction.Reset) || (e.Action == NotifyCollectionChangedAction.Move))
            //    NotifyPropertyChanged(nameof(InstanceRepository));
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String ResourceName { get; set; } = "";

        protected override void SetDefaultValues()
        {
            Name = "New Dynamic Table";
            ResourceName = "newdyntable";
        }

        public override String GetLabel()
        {
            return Name;
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name = json["Name"]?.GetValue<string>() ?? "";
            this.ResourceName = json["ResourceName"]?.GetValue<string>() ?? "";
        }

        public override JsonObject ToJson()
        {
            var customDynTableJson = base.ToJson();
            customDynTableJson.Add("Name", this.Name);
            customDynTableJson.Add("ResourceName", this.ResourceName);

            return customDynTableJson;
        }
    }
}
