using Eos.Models.Tables;
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
    public class VisualEffect : BaseModel
    {
        private ProgrammedEffect? _impactProgFX;
        private ProgrammedEffect? _durationProgFX;
        private ProgrammedEffect? _cessationProgFX;

        public String Name { get; set; } = "";
        public VisualEffectType Type { get; set; }
        public bool OrientWithGround { get; set; }
        public string? ImpactHeadEffect { get; set; }
        public string? ImpactImpactEffect { get; set; }
        public string? ImpactRootSmallEffect { get; set; }
        public string? ImpactRootMediumEffect { get; set; }
        public string? ImpactRootLargeEffect { get; set; }
        public string? ImpactRootHugeEffect { get; set; }
        public ProgrammedEffect? ImpactProgFX
        {
            get { return _impactProgFX; }
            set { Set(ref _impactProgFX, value); }
        }
        public string? ImpactSound { get; set; }
        public ProgrammedEffect? DurationProgFX
        {
            get { return _durationProgFX; }
            set { Set(ref _durationProgFX, value); }
        }
        public string? DurationSound { get; set; }
        public ProgrammedEffect? CessationProgFX
        {
            get { return _cessationProgFX; }
            set { Set(ref _cessationProgFX, value); }
        }
        public string? CessationSound { get; set; }
        public string? CessationHeadEffect { get; set; }
        public string? CessationImpactEffect { get; set; }
        public string? CessationRootSmallEffect { get; set; }
        public string? CessationRootMediumEffect { get; set; }
        public string? CessationRootLargeEffect { get; set; }
        public string? CessationRootHugeEffect { get; set; }
        public VFXShakeType ShakeType { get; set; }
        public double? ShakeDelay { get; set; }
        public double? ShakeDuration { get; set; }
        public string? LowViolenceModel { get; set; }
        public string? LowQualityModel { get; set; }
        public bool OrientWithObject { get; set; }

        public override String GetLabel()
        {
            return Name;
        }

        protected override string GetTypeName()
        {
            return "Visual Effect";
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            ImpactProgFX = Resolve(ImpactProgFX, MasterRepository.ProgrammedEffects);
            DurationProgFX = Resolve(DurationProgFX, MasterRepository.ProgrammedEffects);
            CessationProgFX = Resolve(CessationProgFX, MasterRepository.ProgrammedEffects);
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name = json["Name"]?.GetValue<String>() ?? "";
            this.Type = JsonToEnum<VisualEffectType>(json["Type"]) ?? VisualEffectType.F;
            this.OrientWithGround = json["OrientWithGround"]?.GetValue<bool>() ?? false;
            this.ImpactHeadEffect = json["ImpactHeadEffect"]?.GetValue<string>();
            this.ImpactImpactEffect = json["ImpactImpactEffect"]?.GetValue<string>();
            this.ImpactRootSmallEffect = json["ImpactRootSmallEffect"]?.GetValue<string>();
            this.ImpactRootMediumEffect = json["ImpactRootMediumEffect"]?.GetValue<string>();
            this.ImpactRootLargeEffect = json["ImpactRootLargeEffect"]?.GetValue<string>();
            this.ImpactRootHugeEffect = json["ImpactRootHugeEffect"]?.GetValue<string>();
            this.ImpactProgFX = CreateRefFromJson<ProgrammedEffect>(json["ImpactProgFX"]?.AsObject());
            this.ImpactSound = json["ImpactSound"]?.GetValue<string>();
            this.DurationProgFX = CreateRefFromJson<ProgrammedEffect>(json["DurationProgFX"]?.AsObject());
            this.DurationSound = json["DurationSound"]?.GetValue<string>();
            this.CessationProgFX = CreateRefFromJson<ProgrammedEffect>(json["CessationProgFX"]?.AsObject());
            this.CessationSound = json["CessationSound"]?.GetValue<string>();
            this.CessationHeadEffect = json["CessationHeadEffect"]?.GetValue<string>();
            this.CessationImpactEffect = json["CessationImpactEffect"]?.GetValue<string>();
            this.CessationRootSmallEffect = json["CessationRootSmallEffect"]?.GetValue<string>();
            this.CessationRootMediumEffect = json["CessationRootMediumEffect"]?.GetValue<string>();
            this.CessationRootLargeEffect = json["CessationRootLargeEffect"]?.GetValue<string>();
            this.CessationRootHugeEffect = json["CessationRootHugeEffect"]?.GetValue<string>();
            this.ShakeType = JsonToEnum<VFXShakeType>(json["ShakeType"]) ?? VFXShakeType.None;
            this.ShakeDelay = json["ShakeDelay"]?.GetValue<double>();
            this.ShakeDuration = json["ShakeDuration"]?.GetValue<double>();
            this.LowViolenceModel = json["LowViolenceModel"]?.GetValue<string>();
            this.LowQualityModel = json["LowQualityModel"]?.GetValue<string>();
            this.OrientWithObject = json["OrientWithObject"]?.GetValue<bool>() ?? false;
        }

        public override JsonObject ToJson()
        {
            var vfxJson = base.ToJson();
            vfxJson.Add("Name", this.Name);
            vfxJson.Add("Type", EnumToJson(this.Type));
            vfxJson.Add("OrientWithGround", this.OrientWithGround);
            vfxJson.Add("ImpactHeadEffect", this.ImpactHeadEffect);
            vfxJson.Add("ImpactImpactEffect", this.ImpactImpactEffect);
            vfxJson.Add("ImpactRootSmallEffect", this.ImpactRootSmallEffect);
            vfxJson.Add("ImpactRootMediumEffect", this.ImpactRootMediumEffect);
            vfxJson.Add("ImpactRootLargeEffect", this.ImpactRootLargeEffect);
            vfxJson.Add("ImpactRootHugeEffect", this.ImpactRootHugeEffect);
            vfxJson.Add("ImpactProgFX", CreateJsonRef(this.ImpactProgFX));
            vfxJson.Add("ImpactSound", this.ImpactSound);
            vfxJson.Add("DurationProgFX", CreateJsonRef(this.DurationProgFX));
            vfxJson.Add("DurationSound", this.DurationSound);
            vfxJson.Add("CessationProgFX", CreateJsonRef(this.CessationProgFX));
            vfxJson.Add("CessationSound", this.CessationSound);
            vfxJson.Add("CessationHeadEffect", this.CessationHeadEffect);
            vfxJson.Add("CessationImpactEffect", this.CessationImpactEffect);
            vfxJson.Add("CessationRootSmallEffect", this.CessationRootSmallEffect);
            vfxJson.Add("CessationRootMediumEffect", this.CessationRootMediumEffect);
            vfxJson.Add("CessationRootLargeEffect", this.CessationRootLargeEffect);
            vfxJson.Add("CessationRootHugeEffect", this.CessationRootHugeEffect);
            vfxJson.Add("ShakeType", EnumToJson(this.ShakeType));
            vfxJson.Add("ShakeDelay", this.ShakeDelay);
            vfxJson.Add("ShakeDuration", this.ShakeDuration);
            vfxJson.Add("LowViolenceModel", this.LowViolenceModel);
            vfxJson.Add("LowQualityModel", this.LowQualityModel);
            vfxJson.Add("OrientWithObject", this.OrientWithObject);

            return vfxJson;
        }
    }
}
