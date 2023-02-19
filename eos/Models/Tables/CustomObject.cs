using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class CustomObject : BaseTable<CustomObjectProperty>
    {
        private DataTypeDefinition dataTypeDefinition = new DataTypeDefinition(Guid.Empty, String.Empty, null);

        public DataTypeDefinition DataTypeDefinition
        {
            get
            {
                if (dataTypeDefinition.ID != ID)
                {
                    dataTypeDefinition = new DataTypeDefinition(ID, Name, this, true);
                    dataTypeDefinition.ToJson = o => ((CustomObjectInstance?)o)?.ToJsonRef();
                    dataTypeDefinition.To2DA = o => MasterRepository.Project.CustomObjectRepositories[this].Get2DAIndex((CustomObjectInstance?)o);
                    dataTypeDefinition.FromJson = json => JsonUtils.CreateRefFromJson<CustomObjectInstance>((JsonObject?)json);
                }

                return dataTypeDefinition;
            }
        }

        public event EventHandler? OnChanged;

        public String ResourceName { get; set; } = "";

        protected override void SetDefaultValues()
        {
            Name = "NewObject";
        }

        public override JsonObject ToJson()
        {
            var json = base.ToJson();
            json.Add("ResourceName", this.ResourceName);
            return json;
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.ResourceName = json["ResourceName"]?.GetValue<String>() ?? "";
        }

        protected override void Changed()
        {
            OnChanged?.Invoke(this, new EventArgs());
        }
    }
}
