using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Eos.Models
{
    public class VisualEffect : BaseModel
    {
        public String Name { get; set; } = "";

        public override String GetLabel()
        {
            return Name;
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name = json["Name"]?.GetValue<String>() ?? "";
        }

        public override JsonObject ToJson()
        {
            var vfxJson = base.ToJson();
            vfxJson.Add("Name", this.Name);

            return vfxJson;
        }
    }
}
