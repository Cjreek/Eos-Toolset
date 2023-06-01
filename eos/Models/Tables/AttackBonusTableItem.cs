using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class AttackBonusTableItem : TableItem
    {
        public int Level { get; set; } = 1;
        public int AttackBonus { get; set; } = 0;

        public AttackBonusTableItem() : base()
        {
        }

        public AttackBonusTableItem(AttackBonusTable parentTable) : base(parentTable)
        {
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Level = json["Level"]?.GetValue<int>() ?? 1;
            this.AttackBonus = json["AttackBonus"]?.GetValue<int>() ?? 0;
        }

        public override JsonObject ToJson()
        {
            var json = base.ToJson();
            json.Add("Level", this.Level);
            json.Add("AttackBonus", this.AttackBonus);

            return json;
        }
    }
}
