using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class BonusFeatsTableItem : TableItem
    {
        public int Level { get; set; } = 1;
        public int BonusFeatCount { get; set; } = 0;

        public BonusFeatsTableItem() : base()
        {
        }

        public BonusFeatsTableItem(BonusFeatsTable parentTable) : base(parentTable)
        {
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Level = json["Level"]?.GetValue<int>() ?? 1;
            this.BonusFeatCount = json["BonusFeatCount"]?.GetValue<int>() ?? 0;
        }

        public override JsonObject ToJson()
        {
            var json = base.ToJson();
            json.Add("Level", this.Level);
            json.Add("BonusFeatCount", this.BonusFeatCount);

            return json;
        }
    }
}
