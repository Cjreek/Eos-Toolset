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
    public class Poison : BaseModel
    {
        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public int SaveDC { get; set; } = 10;
        public int HandleDC { get; set; } = 0; // Unused?
        public int InitialAbilityDamageDiceCount { get; set; } = 1;
        public int InitialAbilityDamageDice { get; set; } = 4;
        public AbilityType? InitialAbilityDamageType { get; set; } = AbilityType.CON;
        public String? InitialEffectScript { get; set; }
        public int SecondaryAbilityDamageDiceCount { get; set; } = 1;
        public int SecondaryAbilityDamageDice { get; set; } = 4;
        public AbilityType? SecondaryAbilityDamageType { get; set; } = AbilityType.CON;
        public String? SecondaryEffectScript { get; set; }
        public double Cost { get; set; } = 1.0; // Unused?
        public bool OnHitApplied { get; set; } = false; // Unused?
        public String? ImpactVFX { get; set; }

        protected override void Initialize()
        {
            base.Initialize();
            Name = new TLKStringSet(() => NotifyPropertyChanged(nameof(Name)));
        }

        protected override TLKStringSet? GetTlkDisplayName()
        {
            var modelOverride = (Poison?)MasterRepository.Project.GetOverride(this);
            return modelOverride?.Name ?? this.Name;
        }

        public override String GetLabel()
        {
            return Name;
        }

        protected override void SetDefaultValues()
        {
            Name[MasterRepository.Project.DefaultLanguage].Text = "New Poison";
            Name[MasterRepository.Project.DefaultLanguage].TextF = "New Poison";
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name.FromJson(json["Name"]?.AsObject());
            this.SaveDC = json["SaveDC"]?.GetValue<int>() ?? 10;
            this.HandleDC = json["HandleDC"]?.GetValue<int>() ?? 0;
            this.InitialAbilityDamageDiceCount = json["InitialAbilityDamageDiceCount"]?.GetValue<int>() ?? 1;
            this.InitialAbilityDamageDice = json["InitialAbilityDamageDice"]?.GetValue<int>() ?? 4;
            this.InitialAbilityDamageType = JsonToEnum<AbilityType>(json["InitialAbilityDamageType"]);
            this.InitialEffectScript = json["InitialEffectScript"]?.GetValue<String>();
            this.SecondaryAbilityDamageDiceCount = json["SecondaryAbilityDamageDiceCount"]?.GetValue<int>() ?? 1;
            this.SecondaryAbilityDamageDice = json["SecondaryAbilityDamageDice"]?.GetValue<int>() ?? 4;
            this.SecondaryAbilityDamageType = JsonToEnum<AbilityType>(json["SecondaryAbilityDamageType"]);
            this.SecondaryEffectScript = json["SecondaryEffectScript"]?.GetValue<String>();
            this.Cost = json["Cost"]?.GetValue<double>() ?? 1.0;
            this.OnHitApplied = json["OnHitApplied"]?.GetValue<bool>() ?? false;
            this.ImpactVFX = json["ImpactVFX"]?.GetValue<String>();
        }

        public override JsonObject ToJson()
        {
            var poisonJson = base.ToJson();
            poisonJson.Add("Name", this.Name.ToJson());
            poisonJson.Add("SaveDC", this.SaveDC);
            poisonJson.Add("HandleDC", this.HandleDC);
            poisonJson.Add("InitialAbilityDamageDiceCount", this.InitialAbilityDamageDiceCount);
            poisonJson.Add("InitialAbilityDamageDice", this.InitialAbilityDamageDice);
            poisonJson.Add("InitialAbilityDamageType", EnumToJson(this.InitialAbilityDamageType));
            poisonJson.Add("InitialEffectScript", this.InitialEffectScript);
            poisonJson.Add("SecondaryAbilityDamageDiceCount", this.SecondaryAbilityDamageDiceCount);
            poisonJson.Add("SecondaryAbilityDamageDice", this.SecondaryAbilityDamageDice);
            poisonJson.Add("SecondaryAbilityDamageType", EnumToJson(this.SecondaryAbilityDamageType));
            poisonJson.Add("SecondaryEffectScript", this.SecondaryEffectScript);
            poisonJson.Add("Cost", this.Cost);
            poisonJson.Add("OnHitApplied", this.OnHitApplied);
            poisonJson.Add("ImpactVFX", this.ImpactVFX);

            return poisonJson;
        }
    }
}
