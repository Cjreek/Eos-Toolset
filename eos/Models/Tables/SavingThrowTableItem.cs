using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class SavingThrowTableItem : TableItem
    {
        public int Level { get; set; }
        public int FortitudeSave { get; set; }
        public int ReflexSave { get; set; }
        public int WillpowerSave { get; set; }

        public override void FromJson(JsonObject json)
        {
            this.Level = json["Level"]?.GetValue<int>() ?? 1;
            this.FortitudeSave = json["FortitudeSave"]?.GetValue<int>() ?? 0;
            this.ReflexSave = json["ReflexSave"]?.GetValue<int>() ?? 0;
            this.WillpowerSave = json["WillpowerSave"]?.GetValue<int>() ?? 0;
        }

        public override JsonObject ToJson()
        {
            var json = new JsonObject();
            json.Add("Level", this.Level);
            json.Add("FortitudeSave", this.FortitudeSave);
            json.Add("ReflexSave", this.ReflexSave);
            json.Add("WillpowerSave", this.WillpowerSave);

            return json;
        }
    }
}
