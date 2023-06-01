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

        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public DamageTypeGroup? Group
        {
            get { return _group; }
            set { Set(ref _group, value); }
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
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name.FromJson(json["Name"]?.AsObject());
            this.Group = CreateRefFromJson<DamageTypeGroup>(json["Group"]?.AsObject());
        }

        public override JsonObject ToJson()
        {
            var damageTypeJson = base.ToJson();
            damageTypeJson.Add("Name", this.Name.ToJson());
            damageTypeJson.Add("Group", CreateJsonRef(this.Group));

            return damageTypeJson;
        }
    }
}
