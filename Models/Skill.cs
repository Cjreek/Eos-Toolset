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
    public class Skill : BaseModel
    {
        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public TLKStringSet Description { get; set; } = new TLKStringSet();
        public bool CanUseUntrained { get; set; }
        public AbilityType KeyAbility { get; set; }
        public bool UseArmorPenalty { get; set; }
        public bool AllClassesCanUse { get; set; }
        public AICategory? AIBehaviour { get; set; }
        public bool IsHostile { get; set; }
        public bool HideFromLevelUp { get; set; } = false;

        protected override TLKStringSet? GetTlkDisplayName()
        {
            var modelOverride = (Skill?)MasterRepository.Project.GetOverride(this);
            return modelOverride?.Name ?? this.Name;
        }

        public override String GetLabel()
        {
            return Name;
        }

        protected override void SetDefaultValues()
        {
            Name[MasterRepository.Project.DefaultLanguage].Text = "New Skill";
            Name[MasterRepository.Project.DefaultLanguage].TextF = "New Skill";
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name.FromJson(json["Name"]?.AsObject());
            this.Description.FromJson(json["Description"]?.AsObject());
            this.Icon = json["Icon"]?.GetValue<String>();
            this.CanUseUntrained = json["CanUseUntrained"]?.GetValue<bool>() ?? true;
            this.KeyAbility = JsonToEnum<AbilityType>(json["KeyAbility"]) ?? AbilityType.DEX;
            this.UseArmorPenalty = json["UseArmorPenalty"]?.GetValue<bool>() ?? false;
            this.AllClassesCanUse = json["AllClassesCanUse"]?.GetValue<bool>() ?? true;
            this.AIBehaviour = JsonToEnum<AICategory>(json["AIBehaviour"]);
            this.IsHostile = json["IsHostile"]?.GetValue<bool>() ?? false;
            this.HideFromLevelUp = json["HideFromLevelUp"]?.GetValue<bool>() ?? false;
        }

        public override JsonObject ToJson()
        {
            var skillJson = base.ToJson();
            skillJson.Add("Name", this.Name.ToJson());
            skillJson.Add("Description", this.Description.ToJson());
            skillJson.Add("Icon", this.Icon);
            skillJson.Add("CanUseUntrained", this.CanUseUntrained);
            skillJson.Add("KeyAbility", EnumToJson(this.KeyAbility));
            skillJson.Add("UseArmorPenalty", this.UseArmorPenalty);
            skillJson.Add("AllClassesCanUse", this.AllClassesCanUse);
            skillJson.Add("AIBehaviour", EnumToJson(this.AIBehaviour));
            skillJson.Add("IsHostile", this.IsHostile);
            skillJson.Add("HideFromLevelUp", this.HideFromLevelUp);

            return skillJson;
        }
    }
}
