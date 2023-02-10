using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

using static Eos.Models.JsonUtils;

namespace Eos.Models
{
    public class AreaEffect : BaseModel
    {
        public String Name { get; set; } = "";
        public AreaEffectShape Shape { get; set; }
        public double? Radius { get; set; }
        public double? Width { get; set; }
        public double? Length { get; set; }
        public String? OnEnterScript { get; set; }
        public String? OnExitScript { get; set; }
        public String? OnHeartbeatScript { get; set; }
        public bool OrientWithGround { get; set; }
        public IntPtr VisualEffect { get; set; }
        public String? Model1 { get; set; }
        public String? LowQualityModel1 { get; set; }
        public int? Model1Amount { get; set; }
        public int? Model1Duration { get; set; }
        public double? Model1EdgeWeight { get; set; }
        public String? Model2 { get; set; }
        public String? LowQualityModel2 { get; set; }
        public int? Model2Amount { get; set; }
        public int? Model2Duration { get; set; }
        public double? Model2EdgeWeight { get; set; }
        public String? Model3 { get; set; }
        public String? LowQualityModel3 { get; set; }
        public int? Model3Amount { get; set; }
        public int? Model3Duration { get; set; }
        public double? Model3EdgeWeight { get; set; }
        public String? ImpactSound { get; set; }
        public String? LoopSound { get; set; }
        public String? CessationSound { get; set; }
        public String? RandomSound { get; set; }
        public double? RandomSoundChance { get; set; }

        public override String GetLabel()
        {
            return Name;
        }

        protected override void SetDefaultValues()
        {
            Name = "VFX_PER_NEW";
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name = json["Name"]?.GetValue<String>() ?? "";
            this.Shape = JsonToEnum<AreaEffectShape>(json["Shape"]) ?? AreaEffectShape.C;
            this.Radius = json["Radius"]?.GetValue<double>();
            this.Width = json["Width"]?.GetValue<double>();
            this.Length = json["Length"]?.GetValue<double>();
            this.OnEnterScript = json["OnEnterScript"]?.GetValue<String>();
            this.OnExitScript = json["OnExitScript"]?.GetValue<String>();
            this.OnHeartbeatScript = json["OnHeartbeatScript"]?.GetValue<String>();
            this.OrientWithGround = json["OrientWithGround"]?.GetValue<bool>() ?? false;
            //this.VisualEffect = json["VisualEffect"]?.GetValue<int>();
            this.Model1 = json["Model1"]?.GetValue<String>();
            this.LowQualityModel1 = json["LowQualityModel1"]?.GetValue<String>();
            this.Model1Amount = json["Model1Amount"]?.GetValue<int>();
            this.Model1Duration = json["Model1Duration"]?.GetValue<int>();
            this.Model1EdgeWeight = json["Model1EdgeWeight"]?.GetValue<double>();
            this.Model2 = json["Model2"]?.GetValue<String>();
            this.LowQualityModel2 = json["LowQualityModel2"]?.GetValue<String>();
            this.Model2Amount = json["Model2Amount"]?.GetValue<int>();
            this.Model2Duration = json["Model2Duration"]?.GetValue<int>();
            this.Model2EdgeWeight = json["Model2EdgeWeight"]?.GetValue<double>();
            this.Model3 = json["Model3"]?.GetValue<String>();
            this.LowQualityModel3 = json["LowQualityModel3"]?.GetValue<String>();
            this.Model3Amount = json["Model3Amount"]?.GetValue<int>();
            this.Model3Duration = json["Model3Duration"]?.GetValue<int>();
            this.Model3EdgeWeight = json["Model3EdgeWeight"]?.GetValue<double>();
            this.ImpactSound = json["ImpactSound"]?.GetValue<String>();
            this.LoopSound = json["LoopSound"]?.GetValue<String>();
            this.CessationSound = json["CessationSound"]?.GetValue<String>();
            this.RandomSound = json["RandomSound"]?.GetValue<String>();
            this.RandomSoundChance = json["RandomSoundChance"]?.GetValue<double>();
        }

        public override JsonObject ToJson()
        {
            var aoeJson = base.ToJson();
            aoeJson.Add("Name", this.Name);
            aoeJson.Add("Shape", EnumToJson(this.Shape));
            aoeJson.Add("Radius", this.Radius);
            aoeJson.Add("Width", this.Width);
            aoeJson.Add("Length", this.Length);
            aoeJson.Add("OnEnterScript", this.OnEnterScript);
            aoeJson.Add("OnExitScript", this.OnExitScript);
            aoeJson.Add("OnHeartbeatScript", this.OnHeartbeatScript);
            aoeJson.Add("OrientWithGround", this.OrientWithGround);
            aoeJson.Add("VisualEffect", null); // !
            aoeJson.Add("Model1", this.Model1);
            aoeJson.Add("LowQualityModel1", this.LowQualityModel1);
            aoeJson.Add("Model1Amount", this.Model1Amount);
            aoeJson.Add("Model1Duration", this.Model1Duration);
            aoeJson.Add("Model1EdgeWeight", this.Model1EdgeWeight);
            aoeJson.Add("Model2", this.Model2);
            aoeJson.Add("LowQualityModel2", this.LowQualityModel2);
            aoeJson.Add("Model2Amount", this.Model2Amount);
            aoeJson.Add("Model2Duration", this.Model2Duration);
            aoeJson.Add("Model2EdgeWeight", this.Model2EdgeWeight);
            aoeJson.Add("Model3", this.Model3);
            aoeJson.Add("LowQualityModel3", this.LowQualityModel3);
            aoeJson.Add("Model3Amount", this.Model3Amount);
            aoeJson.Add("Model3Duration", this.Model3Duration);
            aoeJson.Add("Model3EdgeWeight", this.Model3EdgeWeight);
            aoeJson.Add("ImpactSound", this.ImpactSound);
            aoeJson.Add("LoopSound", this.LoopSound);
            aoeJson.Add("CessationSound", this.CessationSound);
            aoeJson.Add("RandomSound", this.RandomSound);
            aoeJson.Add("RandomSoundChance", this.RandomSoundChance);

            return aoeJson;
        }
    }
}
