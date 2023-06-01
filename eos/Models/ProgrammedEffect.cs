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
    public class ProgrammedEffect : BaseModel
    {
        public string Name { get; set; } = "";
        public ProgrammedEffectType Type { get; set; }

        // Type == 1 (Skin Overlay)
        public string T1ModelName { get; set; } = "";
        public ArmorType T1ArmorType { get; set; }
        public VisualEffect? T1OnHitVFX { get; set; }
        public VisualEffect? T1OnHitVFXSmall { get; set; }

        // Type == 2 (Environment Mapping)
        public string T2EnvironmentMap { get; set; } = "";

        // Type == 3 (Glow Effect)
        public uint T3GlowColor { get; set; }

        // Type == 4 (Lighting)
        public string T4LightModelAnimation { get; set; } = "";
        public double T4AnimationSpeed { get; set; }
        public bool T4CastShadows { get; set; }
        public int T4Priority { get; set; }
        public bool T4RemoveCloseToOtherLights { get; set; }
        public bool T4RemoveAllOtherLights { get; set; }
        public string T4LightModel { get; set; } = "";

        // Type == 5 (Alpha Transparency)
        public double T5OpacityFrom { get; set; }
        public uint T5TransparencyColor { get; set; }
        public bool T5TransparencyColorKeepRed { get; set; }
        public bool T5TransparencyColorKeepGreen { get; set; }
        public bool T5TransparencyColorKeepBlue { get; set; }
        public int T5FadeInterval { get; set; }
        public double T5OpacityTo { get; set; }

        // Type == 6 (Pulsing Aura)
        public uint T6Color1 { get; set; }
        public uint T6Color2 { get; set; }
        public int T6FadeDuration { get; set; }

        // Type == 7 (Beam)
        public string T7BeamModel { get; set; } = "";
        public string T7BeamAnimation { get; set; } = "";

        // Type == 10 (MIRV projectile)
        public string T10ProjectileModel { get; set; } = "";
        public Spell? T10Spell { get; set; }
        public ProjectileOrientation T10Orientation { get; set; }
        public ProjectilePath T10ProjectilePath { get; set; }
        public ProjectileTravelTime T10TravelTime { get; set; }

        // Type == 11 (Variant MIRV projectile)
        public string T11ProjectileModel { get; set; } = "";
        public string T11FireSound { get; set; } = "";
        public string T11ImpactSound { get; set; } = "";
        public ProjectilePath T11ProjectilePath { get; set; }

        // Type == 12 (Spellcasting failure)
        public string T12ModelNode { get; set; } = "";
        public string T12EffectModel { get; set; } = "";


        protected override String GetTypeName()
        {
            return "Programmed Effect";
        }

        public override String GetLabel()
        {
            return Name;
        }

        protected override void SetDefaultValues()
        {
            Name = "New Programmed Effect";
            Type = ProgrammedEffectType.SkinOverlay;
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            switch (this.Type)
            {
                case ProgrammedEffectType.SkinOverlay:
                    T1OnHitVFX = Resolve(T1OnHitVFX, MasterRepository.VisualEffects);
                    T1OnHitVFXSmall = Resolve(T1OnHitVFXSmall, MasterRepository.VisualEffects);
                    break;

                case ProgrammedEffectType.MIRV:
                    T10Spell = Resolve(T10Spell, MasterRepository.Spells);
                    break;
            }
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name = json["Name"]?.GetValue<String>() ?? "";
            this.Type = JsonToEnum<ProgrammedEffectType>(json["Type"]) ?? ProgrammedEffectType.Invalid;

            switch (this.Type)
            {
                case ProgrammedEffectType.SkinOverlay:
                    this.T1ModelName = json["ModelName"]?.GetValue<String>() ?? "";
                    this.T1ArmorType = JsonToEnum<ArmorType>(json["ArmorType"]) ?? ArmorType.Leather;
                    this.T1OnHitVFX = CreateRefFromJson<VisualEffect>(json["OnHitVFX"]?.AsObject());
                    this.T1OnHitVFXSmall = CreateRefFromJson<VisualEffect>(json["OnHitVFXSmall"]?.AsObject());
                    break;

                case ProgrammedEffectType.EnvironmentMapping:
                    this.T2EnvironmentMap = json["EnvironmentMap"]?.GetValue<String>() ?? "";
                    break;

                case ProgrammedEffectType.GlowEffect:
                    this.T3GlowColor = json["GlowColor"]?.GetValue<uint>() ?? 0;
                    break;

                case ProgrammedEffectType.Lighting:
                    this.T4LightModel = json["LightModel"]?.GetValue<String>() ?? "";
                    this.T4LightModelAnimation = json["LightModelAnimation"]?.GetValue<String>() ?? "";
                    this.T4AnimationSpeed = json["AnimationSpeed"]?.GetValue<double>() ?? 1.0;
                    this.T4CastShadows = json["CastShadows"]?.GetValue<bool>() ?? false;
                    this.T4Priority = json["Priority"]?.GetValue<int>() ?? 20;
                    this.T4RemoveCloseToOtherLights = json["RemoveCloseToOtherLights"]?.GetValue<bool>() ?? false;
                    this.T4RemoveAllOtherLights = json["RemoveAllOtherLights"]?.GetValue<bool>() ?? false;
                    break;

                case ProgrammedEffectType.AlphaTransparency:
                    this.T5OpacityFrom = json["OpacityFrom"]?.GetValue<double>() ?? 0.0;
                    this.T5OpacityTo = json["OpacityTo"]?.GetValue<double>() ?? 1.0;
                    this.T5TransparencyColor = json["TransparencyColor"]?.GetValue<uint>() ?? 0;
                    this.T5TransparencyColorKeepRed = json["T5TransparencyColorKeepRed"]?.GetValue<bool>() ?? false;
                    this.T5TransparencyColorKeepGreen = json["T5TransparencyColorKeepGreen"]?.GetValue<bool>() ?? false;
                    this.T5TransparencyColorKeepBlue = json["T5TransparencyColorKeepBlue"]?.GetValue<bool>() ?? false;
                    this.T5FadeInterval = json["FadeInterval"]?.GetValue<int>() ?? 1000;
                    break;

                case ProgrammedEffectType.PulsingAura:
                    this.T6Color1 = json["Color1"]?.GetValue<uint>() ?? 0;
                    this.T6Color2 = json["Color2"]?.GetValue<uint>() ?? 0;
                    this.T6FadeDuration = json["FadeDuration"]?.GetValue<int>() ?? 0;
                    break;

                case ProgrammedEffectType.Beam:
                    this.T7BeamModel = json["BeamModel"]?.GetValue<String>() ?? "";
                    this.T7BeamAnimation = json["BeamAnimation"]?.GetValue<String>() ?? "";
                    break;

                case ProgrammedEffectType.MIRV:
                    this.T10ProjectileModel = json["ProjectileModel"]?.GetValue<String>() ?? "";
                    this.T10Spell = CreateRefFromJson<Spell>(json["Spell"]?.AsObject());
                    this.T10Orientation = JsonToEnum<ProjectileOrientation>(json["Orientation"]) ?? ProjectileOrientation.None;
                    this.T10ProjectilePath = JsonToEnum<ProjectilePath>(json["ProjectilePath"]) ?? ProjectilePath.Default;
                    this.T10TravelTime = JsonToEnum<ProjectileTravelTime>(json["TravelTime"]) ?? ProjectileTravelTime.Log; 
                    break;

                case ProgrammedEffectType.VariantMIRV:
                    this.T11ProjectileModel = json["ProjectileModel"]?.GetValue<String>() ?? "";
                    this.T11ProjectilePath = JsonToEnum<ProjectilePath>(json["ProjectilePath"]) ?? ProjectilePath.Default;
                    this.T11FireSound = json["FireSound"]?.GetValue<String>() ?? "";
                    this.T11ImpactSound = json["ImpactSound"]?.GetValue<String>() ?? "";
                    break;

                case ProgrammedEffectType.SpellCastFailure:
                    this.T12ModelNode = json["ModelNode"]?.GetValue<String>() ?? "";
                    this.T12EffectModel = json["EffectModel"]?.GetValue<String>() ?? "";
                    break;
            }
        }

        public override JsonObject ToJson()
        {
            var progFXJson = base.ToJson();
            progFXJson.Add("Name", this.Name);
            progFXJson.Add("Type", EnumToJson(this.Type));
            switch (this.Type)
            {
                case ProgrammedEffectType.SkinOverlay:
                    progFXJson.Add("ModelName", this.T1ModelName);
                    progFXJson.Add("ArmorType", EnumToJson(this.T1ArmorType));
                    progFXJson.Add("OnHitVFX", CreateJsonRef(this.T1OnHitVFX));
                    progFXJson.Add("OnHitVFXSmall", CreateJsonRef(this.T1OnHitVFXSmall));
                    break;

                case ProgrammedEffectType.EnvironmentMapping:
                    progFXJson.Add("EnvironmentMap", this.T2EnvironmentMap);
                    break;

                case ProgrammedEffectType.GlowEffect:
                    progFXJson.Add("GlowColor", this.T3GlowColor);
                    break;

                case ProgrammedEffectType.Lighting:
                    progFXJson.Add("LightModel", this.T4LightModel);
                    progFXJson.Add("LightModelAnimation", this.T4LightModelAnimation);
                    progFXJson.Add("AnimationSpeed", this.T4AnimationSpeed);
                    progFXJson.Add("CastShadows", this.T4CastShadows);
                    progFXJson.Add("Priority", this.T4Priority);
                    progFXJson.Add("RemoveCloseToOtherLights", this.T4RemoveCloseToOtherLights);
                    progFXJson.Add("RemoveAllOtherLights", this.T4RemoveAllOtherLights);
                    break;

                case ProgrammedEffectType.AlphaTransparency:
                    progFXJson.Add("OpacityFrom", this.T5OpacityFrom);
                    progFXJson.Add("OpacityTo", this.T5OpacityTo);
                    progFXJson.Add("TransparencyColor", this.T5TransparencyColor);
                    progFXJson.Add("T5TransparencyColorKeepRed", this.T5TransparencyColorKeepRed);
                    progFXJson.Add("T5TransparencyColorKeepGreen", this.T5TransparencyColorKeepGreen);
                    progFXJson.Add("T5TransparencyColorKeepBlue", this.T5TransparencyColorKeepBlue);
                    progFXJson.Add("FadeInterval", this.T5FadeInterval);
                    break;

                case ProgrammedEffectType.PulsingAura:
                    progFXJson.Add("Color1", this.T6Color1);
                    progFXJson.Add("Color2", this.T6Color2);
                    progFXJson.Add("FadeDuration", this.T6FadeDuration);
                    break;

                case ProgrammedEffectType.Beam:
                    progFXJson.Add("BeamModel", this.T7BeamModel);
                    progFXJson.Add("BeamAnimation", this.T7BeamAnimation);
                    break;

                case ProgrammedEffectType.MIRV:
                    progFXJson.Add("ProjectileModel", this.T10ProjectileModel);
                    progFXJson.Add("Spell", CreateJsonRef(this.T10Spell));
                    progFXJson.Add("Orientation", EnumToJson(this.T10Orientation));
                    progFXJson.Add("ProjectilePath", EnumToJson(this.T10ProjectilePath));
                    progFXJson.Add("TravelTime", EnumToJson(this.T10TravelTime));
                    break;

                case ProgrammedEffectType.VariantMIRV:
                    progFXJson.Add("ProjectileModel", this.T11ProjectileModel);
                    progFXJson.Add("ProjectilePath", EnumToJson(this.T11ProjectilePath));
                    progFXJson.Add("FireSound", this.T11FireSound);
                    progFXJson.Add("ImpactSound", this.T11ImpactSound);
                    break;

                case ProgrammedEffectType.SpellCastFailure:
                    progFXJson.Add("ModelNode", this.T12ModelNode);
                    progFXJson.Add("EffectModel", this.T12EffectModel);
                    break;
            }

            return progFXJson;
        }
    }
}
