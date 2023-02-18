using Eos.Nwn.Tlk;
using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using static Eos.Models.JsonUtils;

namespace Eos.Models
{
    public class Disease : BaseModel
    {
        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public int FirstSaveDC { get; set; } = 15;
        public int SecondSaveDC { get; set; } = 10;
        public int IncubationHours { get; set; } = 1;
        public int AbilityDamage1DiceCount { get; set; } = 1;
        public int AbilityDamage1Dice { get; set; } = 4;
        public AbilityType? AbilityDamage1Type { get; set; } = AbilityType.CON;
        public int AbilityDamage2DiceCount { get; set; } = 0;
        public int AbilityDamage2Dice { get; set; } = 0;
        public AbilityType? AbilityDamage2Type { get; set; } = null;
        public int AbilityDamage3DiceCount { get; set; } = 0;
        public int AbilityDamage3Dice { get; set; } = 0;
        public AbilityType? AbilityDamage3Type { get; set; } = null;
        public String? IncubationEndScript { get; set; }
        public String? DailyEffectScript { get; set; }

        protected override void Initialize()
        {
            base.Initialize();
            Name = new TLKStringSet(() => NotifyPropertyChanged(nameof(Name)));
        }

        protected override TLKStringSet? GetTlkDisplayName()
        {
            var modelOverride = (Disease?)MasterRepository.Project.GetOverride(this);
            return modelOverride?.Name ?? this.Name;
        }

        public override String GetLabel()
        {
            return Name;
        }

        protected override void SetDefaultValues()
        {
            Name[MasterRepository.Project.DefaultLanguage].Text = "New Disease";
            Name[MasterRepository.Project.DefaultLanguage].TextF = "New Disease";
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name.FromJson(json["Name"]?.AsObject());
            this.FirstSaveDC = json["FirstSaveDC"]?.GetValue<int>() ?? 15;
            this.SecondSaveDC = json["SecondSaveDC"]?.GetValue<int>() ?? 10;
            this.IncubationHours = json["IncubationHours"]?.GetValue<int>() ?? 1;
            this.AbilityDamage1DiceCount = json["AbilityDamage1DiceCount"]?.GetValue<int>() ?? 1;
            this.AbilityDamage1Dice = json["AbilityDamage1Dice"]?.GetValue<int>() ?? 4;
            this.AbilityDamage1Type = JsonToEnum<AbilityType>(json["AbilityDamage1Type"]);
            this.AbilityDamage2DiceCount = json["AbilityDamage2DiceCount"]?.GetValue<int>() ?? 0;
            this.AbilityDamage2Dice = json["AbilityDamage2Dice"]?.GetValue<int>() ?? 0;
            this.AbilityDamage2Type = JsonToEnum<AbilityType>(json["AbilityDamage2Type"]);
            this.AbilityDamage3DiceCount = json["AbilityDamage3DiceCount"]?.GetValue<int>() ?? 0;
            this.AbilityDamage3Dice = json["AbilityDamage3Dice"]?.GetValue<int>() ?? 0;
            this.AbilityDamage3Type = JsonToEnum<AbilityType>(json["AbilityDamage3Type"]);
            this.IncubationEndScript = json["IncubationEndScript"]?.GetValue<String>();
            this.DailyEffectScript = json["DailyEffectScript"]?.GetValue<String>();
        }

        public override JsonObject ToJson()
        {
            var diseaseJson = base.ToJson();
            diseaseJson.Add("Name", this.Name.ToJson());
            diseaseJson.Add("FirstSaveDC", this.FirstSaveDC);
            diseaseJson.Add("SecondSaveDC", this.SecondSaveDC);
            diseaseJson.Add("IncubationHours", this.IncubationHours);
            diseaseJson.Add("AbilityDamage1DiceCount", this.AbilityDamage1DiceCount);
            diseaseJson.Add("AbilityDamage1Dice", this.AbilityDamage1Dice);
            diseaseJson.Add("AbilityDamage1Type", EnumToJson(this.AbilityDamage1Type));
            diseaseJson.Add("AbilityDamage2DiceCount", this.AbilityDamage1DiceCount);
            diseaseJson.Add("AbilityDamage2Dice", this.AbilityDamage1Dice);
            diseaseJson.Add("AbilityDamage2Type", EnumToJson(this.AbilityDamage1Type));
            diseaseJson.Add("AbilityDamage3DiceCount", this.AbilityDamage1DiceCount);
            diseaseJson.Add("AbilityDamage3Dice", this.AbilityDamage1Dice);
            diseaseJson.Add("AbilityDamage3Type", EnumToJson(this.AbilityDamage1Type));
            diseaseJson.Add("IncubationEndScript", this.IncubationEndScript);
            diseaseJson.Add("DailyEffectScript", this.DailyEffectScript);

            return diseaseJson;
        }
    }
}
