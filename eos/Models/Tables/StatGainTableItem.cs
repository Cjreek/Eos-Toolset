using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class StatGainTableItem : TableItem
    {
        public int Level { get; set; }
        public int? Strength { get; set; }
        public int? Dexterity { get; set; }
        public int? Constitution { get; set; }
        public int? Wisdom { get; set; }
        public int? Intelligence { get; set; }
        public int? Charisma { get; set; }
        public int? NaturalAC { get; set; }

        public StatGainTableItem() : base()
        {
        }

        public StatGainTableItem(StatGainTable parentTable) : base(parentTable)
        {
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Level = json["Level"]?.GetValue<int>() ?? 0;
            this.Strength = json["Strength"]?.GetValue<int>();
            this.Dexterity = json["Dexterity"]?.GetValue<int>();
            this.Constitution = json["Constitution"]?.GetValue<int>();
            this.Wisdom = json["Wisdom"]?.GetValue<int>();
            this.Intelligence = json["Intelligence"]?.GetValue<int>();
            this.Charisma = json["Charisma"]?.GetValue<int>();
            this.NaturalAC = json["NaturalAC"]?.GetValue<int>();
        }

        public override JsonObject ToJson()
        {
            var json = base.ToJson();
            json.Add("Level", this.Level);
            json.Add("Strength", this.Strength);
            json.Add("Dexterity", this.Dexterity);
            json.Add("Constitution", this.Constitution);
            json.Add("Wisdom", this.Wisdom);
            json.Add("Intelligence", this.Intelligence);
            json.Add("Charisma", this.Charisma);
            json.Add("NaturalAC", this.NaturalAC);

            return json;
        }
    }
}
