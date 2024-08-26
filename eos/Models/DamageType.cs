using Eos.Nwn.Tlk;
using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using static Eos.Models.JsonUtils;

namespace Eos.Models
{
    public class DamageType : BaseModel
    {
        private DamageTypeGroup? _group;
        private RangedDamageType? _rangedDamageType;
        public VisualEffect? _meleeVFX;
        public VisualEffect? _rangedVFX;

        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public DamageTypeGroup? Group
        {
            get { return _group; }
            set { Set(ref _group, value); }
        }

        public RangedDamageType? RangedDamageType
        {
            get { return _rangedDamageType; }
            set { Set(ref _rangedDamageType, value); }
        }

        public VisualEffect? MeleeImpactVFX
        {
            get { return _meleeVFX; }
            set { Set(ref _meleeVFX, value); }
        }

        public VisualEffect? RangedImpactVFX
        {
            get { return _rangedVFX; }
            set { Set(ref _rangedVFX, value); }
        }

        protected override void Initialize()
        {
            base.Initialize();
            Name = new TLKStringSet(() => NotifyPropertyChanged(nameof(Name)));
        }

        protected override TLKStringSet? GetTlkDisplayName()
        {
            var modelOverride = (DamageType?)MasterRepository.Project.GetOverride(this);
            return modelOverride?.Name ?? this.Name;
        }

        public override String GetLabel()
        {
            return Name;
        }

        protected override void SetDefaultValues()
        {
            Name[MasterRepository.Project.DefaultLanguage].Text = "New Damage Type";
            Name[MasterRepository.Project.DefaultLanguage].TextF = "New Damage Type";
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            Group = Resolve(Group, MasterRepository.DamageTypeGroups);
            RangedDamageType = Resolve(RangedDamageType, MasterRepository.RangedDamageTypes);
            MeleeImpactVFX = Resolve(MeleeImpactVFX, MasterRepository.VisualEffects);
            RangedImpactVFX = Resolve(RangedImpactVFX, MasterRepository.VisualEffects);
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name.FromJson(json["Name"]?.AsObject());
            this.Group = CreateRefFromJson<DamageTypeGroup>(json["Group"]?.AsObject());
            this.RangedDamageType = CreateRefFromJson<RangedDamageType>(json["RangedDamageType"]?.AsObject());
            this.MeleeImpactVFX = CreateRefFromJson<VisualEffect>(json["MeleeImpactVFX"]?.AsObject());
            this.RangedImpactVFX = CreateRefFromJson<VisualEffect>(json["RangedImpactVFX"]?.AsObject());
        }

        public override JsonObject ToJson()
        {
            var damageTypeJson = base.ToJson();
            damageTypeJson.Add("Name", this.Name.ToJson());
            damageTypeJson.Add("Group", CreateJsonRef(this.Group));
            damageTypeJson.Add("RangedDamageType", CreateJsonRef(this.RangedDamageType));
            damageTypeJson.Add("MeleeImpactVFX", CreateJsonRef(this.MeleeImpactVFX));
            damageTypeJson.Add("RangedImpactVFX", CreateJsonRef(this.RangedImpactVFX));

            return damageTypeJson;
        }
    }
}
