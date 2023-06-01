using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class SpellSlotTableItem : TableItem
    {
        public int Level { get; set; }
        public int? SpellLevel0 { get; set; }
        public int? SpellLevel1 { get; set; }
        public int? SpellLevel2 { get; set; }
        public int? SpellLevel3 { get; set; }
        public int? SpellLevel4 { get; set; }
        public int? SpellLevel5 { get; set; }
        public int? SpellLevel6 { get; set; }
        public int? SpellLevel7 { get; set; }
        public int? SpellLevel8 { get; set; }
        public int? SpellLevel9 { get; set; }

        public SpellSlotTableItem() : base()
        {
        }

        public SpellSlotTableItem(SpellSlotTable parentTable) : base(parentTable)
        {
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Level = json["Level"]?.GetValue<int>() ?? 0;
            this.SpellLevel0 = json["SpellLevel0"]?.GetValue<int>();
            this.SpellLevel1 = json["SpellLevel1"]?.GetValue<int>();
            this.SpellLevel2 = json["SpellLevel2"]?.GetValue<int>();
            this.SpellLevel3 = json["SpellLevel3"]?.GetValue<int>();
            this.SpellLevel4 = json["SpellLevel4"]?.GetValue<int>();
            this.SpellLevel5 = json["SpellLevel5"]?.GetValue<int>();
            this.SpellLevel6 = json["SpellLevel6"]?.GetValue<int>();
            this.SpellLevel7 = json["SpellLevel7"]?.GetValue<int>();
            this.SpellLevel8 = json["SpellLevel8"]?.GetValue<int>();
            this.SpellLevel9 = json["SpellLevel9"]?.GetValue<int>();
        }

        public override JsonObject ToJson()
        {
            var json = base.ToJson();
            json.Add("Level", this.Level);
            json.Add("SpellLevel0", this.SpellLevel0);
            json.Add("SpellLevel1", this.SpellLevel1);
            json.Add("SpellLevel2", this.SpellLevel2);
            json.Add("SpellLevel3", this.SpellLevel3);
            json.Add("SpellLevel4", this.SpellLevel4);
            json.Add("SpellLevel5", this.SpellLevel5);
            json.Add("SpellLevel6", this.SpellLevel6);
            json.Add("SpellLevel7", this.SpellLevel7);
            json.Add("SpellLevel8", this.SpellLevel8);
            json.Add("SpellLevel9", this.SpellLevel9);

            return json;
        }
    }
}
