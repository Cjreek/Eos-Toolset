using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using static Eos.Models.JsonUtils;

namespace Eos.Models.Tables
{
    public class CustomEnumItem : TableItem
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public String Label { get; set; } = "";
        public String Value { get; set; } = "";

        public CustomEnumItem() : base()
        {
        }

        public CustomEnumItem(CustomEnum parentTable) : base(parentTable)
        {
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.ID = ParseGuid(json["ID"]?.GetValue<String>());
            this.Label = json["Label"]?.GetValue<String>() ?? "";
            this.Value = json["Value"]?.GetValue<String>() ?? "";
        }

        public override JsonObject ToJson()
        {
            var json = base.ToJson();
            json.Add("ID", this.ID.ToString());
            json.Add("Label", this.Label);
            json.Add("Value", this.Value);

            return json;
        }
    }
}
