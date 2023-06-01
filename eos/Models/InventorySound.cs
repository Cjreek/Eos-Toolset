using Eos.Nwn.Tlk;
using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Eos.Models
{
    public class InventorySound : BaseModel
    {
        public string Name { get; set; } = "";
        public string Sound { get; set; } = "";

        public override String GetLabel()
        {
            return Name;
        }

        protected override string GetTypeName()
        {
            return "Inventory Sound";
        }

        protected override void SetDefaultValues()
        {
            Name = "New Inventory Sound";
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name = json["Name"]?.GetValue<string>() ?? "";
            this.Sound = json["Sound"]?.GetValue<string>() ?? "";
        }

        public override JsonObject ToJson()
        {
            var inventorySoundJson = base.ToJson();
            inventorySoundJson.Add("Name", this.Name);
            inventorySoundJson.Add("Sound", this.Sound);

            return inventorySoundJson;
        }
    }
}
