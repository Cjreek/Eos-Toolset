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
    public class Ammunition : BaseModel
    {
        private RangedDamageType? _damageType;
        private String _name = "";

        public String Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String? Model { get; set; }
        public String? ShotSound { get; set; }
        public String? ImpactSound { get; set; }
        public AmmunitionType AmmunitionType { get; set; }
        public RangedDamageType? RangedDamageType
        {
            get { return _damageType; }
            set { Set(ref _damageType, value); }
        }

        protected override string GetTypeName()
        {
            return "Ammunition";
        }

        protected override void SetDefaultValues()
        {
            Name = "New Ammunition Type";
            Name = "New Ammunition Type";
        }

        public override String GetLabel()
        {
            return Name;
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            RangedDamageType = Resolve(RangedDamageType, MasterRepository.RangedDamageTypes);
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name = json["Name"]?.GetValue<String>() ?? "";
            this.Model = json["Model"]?.GetValue<String>() ?? "";
            this.ShotSound = json["ShotSound"]?.GetValue<String>() ?? "";
            this.ImpactSound = json["ImpactSound"]?.GetValue<String>() ?? "";
            this.AmmunitionType = JsonToEnum<AmmunitionType>(json["AmmunitionType"]) ?? AmmunitionType.Arrow;
            this.RangedDamageType = CreateRefFromJson<RangedDamageType>(json["RangedDamageType"]?.AsObject());
        }

        public override JsonObject ToJson()
        {
            var ammunitionJson = base.ToJson();
            ammunitionJson.Add("Name", this.Name);
            ammunitionJson.Add("Model", this.Model);
            ammunitionJson.Add("ShotSound", this.ShotSound);
            ammunitionJson.Add("ImpactSound", this.ImpactSound);
            ammunitionJson.Add("AmmunitionType", EnumToJson(this.AmmunitionType));
            ammunitionJson.Add("RangedDamageType", CreateJsonRef(this.RangedDamageType));

            return ammunitionJson;
        }
    }
}
