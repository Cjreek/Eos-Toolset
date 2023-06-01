using Eos.Models.Tables;
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
    public class Appearance : BaseModel
    {
        private AppearanceSoundset? _appearanceSoundset;

        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public string RaceModel { get; set; } = "";
        public string? EnvironmentMap { get; set; }
        public BloodColor BloodColor { get; set; } = BloodColor.R;
        public ModelType ModelType { get; set; } = ModelType.S;
        public bool CanHaveWings { get; set; } = false;
        public bool CanHaveTails { get; set; } = false;
        public double? WeaponScale { get; set; } = 1.0; // ModelType != S
        public double? WingTailScale { get; set; } = 1.0;
        public double? HelmetScaleMale { get; set; } = 1.15; // ModelType P
        public double? HelmetScaleFemale { get; set; } = 0.95; // ModelType P
        public MovementRate MovementRate { get; set; } = MovementRate.NORM;
        public double WalkAnimationDistance { get; set; } = 1.0;
        public double RunAnimationDistance { get; set; } = 3.0;
        public double PersonalSpaceRadius { get; set; } = 0.3;
        public double CreaturePersonalSpaceRadius { get; set; } = 0.5;
        public double CameraHeight { get; set; } = 1.5;
        public double HitDistance { get; set; } = 0.3;
        public double PreferredAttackDistance { get; set; } = 2.0;
        public TargetHeight TargetHeight { get; set; } = TargetHeight.H;
        public bool AbortAttackAnimationOnParry { get; set; } = true;
        public bool HasLegs { get; set; } = true;
        public bool HasArms { get; set; } = true;
        public string? Portrait { get; set; } = "";
        public SizeCategory SizeCategory { get; set; } = SizeCategory.Medium;
        public PerceptionDistance PerceptionRange { get; set; } = PerceptionDistance.Medium;
        public FootstepSound FootstepSound { get; set; } = FootstepSound.Normal;
        public AppearanceSoundset? AppearanceSoundset
        {
            get { return _appearanceSoundset; }
            set { Set(ref _appearanceSoundset, value); }
        }
        public bool HeadTracking { get; set; } = true;
        public int HorizontalHeadTrackingRange { get; set; } = 60;
        public int VerticalHeadTrackingRange { get; set; } = 30;
        public string? ModelHeadNodeName { get; set; } = "head_g";
        public BodyBag BodyBag { get; set; } = BodyBag.Default;
        public bool Targetable { get; set; } = true;

        protected override TLKStringSet? GetTlkDisplayName()
        {
            var modelOverride = (Appearance?)MasterRepository.Project.GetOverride(this);
            return modelOverride?.Name ?? this.Name;
        }

        public override String GetLabel()
        {
            return Name;
        }

        protected override void SetDefaultValues()
        {
            Name[MasterRepository.Project.DefaultLanguage].Text = "New Appearance";
            Name[MasterRepository.Project.DefaultLanguage].TextF = "New Appearance";
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            AppearanceSoundset = Resolve(AppearanceSoundset, MasterRepository.AppearanceSoundsets);
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name.FromJson(json["Name"]?.AsObject());
            this.RaceModel = json["RaceModel"]?.GetValue<string>() ?? "";
            this.EnvironmentMap = json["EnvironmentMap"]?.GetValue<string>();
            this.BloodColor = JsonToEnum<BloodColor>(json["BloodColor"]) ?? BloodColor.R;
            this.ModelType = JsonToEnum<ModelType>(json["ModelType"]) ?? ModelType.S;
            this.CanHaveWings = json["CanHaveWings"]?.GetValue<bool>() ?? false;
            this.CanHaveTails = json["CanHaveTails"]?.GetValue<bool>() ?? false;
            this.WeaponScale = json["WeaponScale"]?.GetValue<double>() ?? 1.0; 
            this.WingTailScale = json["WingTailScale"]?.GetValue<double>() ?? 1.0;
            this.HelmetScaleMale = json["HelmetScaleMale"]?.GetValue<double>() ?? 1.15;
            this.HelmetScaleFemale = json["HelmetScaleFemale"]?.GetValue<double>() ?? 0.95;
            this.MovementRate = JsonToEnum<MovementRate>(json["MovementRate"]) ?? MovementRate.NORM;
            this.WalkAnimationDistance = json["WalkAnimationDistance"]?.GetValue<double>() ?? 1.0;
            this.RunAnimationDistance = json["RunAnimationDistance"]?.GetValue<double>() ?? 3.0;
            this.PersonalSpaceRadius = json["PersonalSpaceRadius"]?.GetValue<double>() ?? 0.3;
            this.CreaturePersonalSpaceRadius = json["CreaturePersonalSpaceRadius"]?.GetValue<double>() ?? 0.5;
            this.CameraHeight = json["CameraHeight"]?.GetValue<double>() ?? 1.5;
            this.HitDistance = json["HitDistance"]?.GetValue<double>() ?? 0.3;
            this.PreferredAttackDistance = json["PreferredAttackDistance"]?.GetValue<double>() ?? 2.0;
            this.TargetHeight = JsonToEnum<TargetHeight>(json["TargetHeight"]) ?? TargetHeight.H;
            this.AbortAttackAnimationOnParry = json["AbortAttackAnimationOnParry"]?.GetValue<bool>() ?? true;
            this.HasLegs = json["HasLegs"]?.GetValue<bool>() ?? true;
            this.HasArms = json["HasArms"]?.GetValue<bool>() ?? true;
            this.Portrait = json["Portrait"]?.GetValue<string>() ?? "";
            this.SizeCategory = JsonToEnum<SizeCategory>(json["SizeCategory"]) ?? SizeCategory.Medium;
            this.PerceptionRange = JsonToEnum<PerceptionDistance>(json["PerceptionRange"]) ?? PerceptionDistance.Medium;
            this.FootstepSound = JsonToEnum<FootstepSound>(json["FootstepSound"]) ?? FootstepSound.Normal;
            this.AppearanceSoundset = CreateRefFromJson<AppearanceSoundset>(json["AppearanceSoundset"]?.AsObject()); 
            this.HeadTracking = json["HeadTracking"]?.GetValue<bool>() ?? true;
            this.HorizontalHeadTrackingRange = json["HorizontalHeadTrackingRange"]?.GetValue<int>() ?? 60;
            this.VerticalHeadTrackingRange = json["VerticalHeadTrackingRange"]?.GetValue<int>() ?? 30;
            this.ModelHeadNodeName = json["ModelHeadNodeName"]?.GetValue<string>() ?? "head_g";
            this.BodyBag = JsonToEnum<BodyBag>(json["BodyBag"]) ?? BodyBag.Default;
            this.Targetable = json["Targetable"]?.GetValue<bool>() ?? true;
        }

        public override JsonObject ToJson()
        {
            var appearanceJson = base.ToJson();
            appearanceJson.Add("Name", this.Name.ToJson());
            appearanceJson.Add("RaceModel", this.RaceModel);
            appearanceJson.Add("EnvironmentMap", this.EnvironmentMap);
            appearanceJson.Add("BloodColor", EnumToJson(this.BloodColor));
            appearanceJson.Add("ModelType", EnumToJson(this.ModelType));
            appearanceJson.Add("CanHaveWings", this.CanHaveWings);
            appearanceJson.Add("CanHaveTails", this.CanHaveTails);
            appearanceJson.Add("WeaponScale", this.WeaponScale);
            appearanceJson.Add("WingTailScale", this.WingTailScale);
            appearanceJson.Add("HelmetScaleMale", this.HelmetScaleMale);
            appearanceJson.Add("HelmetScaleFemale", this.HelmetScaleFemale);
            appearanceJson.Add("MovementRate", EnumToJson(this.MovementRate));
            appearanceJson.Add("WalkAnimationDistance", this.WalkAnimationDistance);
            appearanceJson.Add("RunAnimationDistance", this.RunAnimationDistance);
            appearanceJson.Add("PersonalSpaceRadius", this.PersonalSpaceRadius);
            appearanceJson.Add("CreaturePersonalSpaceRadius", this.CreaturePersonalSpaceRadius);
            appearanceJson.Add("CameraHeight", this.CameraHeight);
            appearanceJson.Add("HitDistance", this.HitDistance);
            appearanceJson.Add("PreferredAttackDistance", this.PreferredAttackDistance);
            appearanceJson.Add("TargetHeight", EnumToJson(this.TargetHeight));
            appearanceJson.Add("AbortAttackAnimationOnParry", this.AbortAttackAnimationOnParry);
            appearanceJson.Add("HasLegs", this.HasLegs);
            appearanceJson.Add("HasArms", this.HasArms);
            appearanceJson.Add("Portrait", this.Portrait);
            appearanceJson.Add("SizeCategory", EnumToJson(this.SizeCategory));
            appearanceJson.Add("PerceptionRange", EnumToJson(this.PerceptionRange));
            appearanceJson.Add("FootstepSound", EnumToJson(this.FootstepSound));
            appearanceJson.Add("AppearanceSoundset", CreateJsonRef(this.AppearanceSoundset));
            appearanceJson.Add("HeadTracking", this.HeadTracking);
            appearanceJson.Add("HorizontalHeadTrackingRange", this.HorizontalHeadTrackingRange);
            appearanceJson.Add("VerticalHeadTrackingRange", this.VerticalHeadTrackingRange);
            appearanceJson.Add("ModelHeadNodeName", this.ModelHeadNodeName);
            appearanceJson.Add("BodyBag", EnumToJson(this.BodyBag));
            appearanceJson.Add("Targetable", this.Targetable);

            return appearanceJson;
        }
    }
}
