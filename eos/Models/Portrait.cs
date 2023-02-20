using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Eos.Models
{
    public class Portrait : BaseModel
    {
        public string ResRef { get; set; } = "";

        public override String GetLabel()
        {
            return ResRef;
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.ResRef = json["ResRef"]?.GetValue<String>() ?? "";
        }

        public override JsonObject ToJson()
        {
            var portraitJson = base.ToJson();
            portraitJson.Add("ResRef", this.ResRef);

            return portraitJson;
        }
    }
}
