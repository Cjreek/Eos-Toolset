using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using static Eos.Models.JsonUtils;

namespace Eos.Models.Tables
{
    public class PackageSkillPreferencesTableItem : TableItem
    {
        private Skill? _skill;

        public Skill? Skill
        {
            get { return _skill; }
            set { Set(ref _skill, value); }
        }

        public PackageSkillPreferencesTableItem() : base()
        {
        }

        public PackageSkillPreferencesTableItem(PackageSkillPreferencesTable parentTable) : base(parentTable)
        {
        }

        public override void ResolveReferences()
        {
            Skill = MasterRepository.Skills.Resolve(Skill);
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Skill = CreateRefFromJson<Skill>(json["Skill"]?.AsObject());
        }

        public override JsonObject ToJson()
        {
            var json = base.ToJson();
            json.Add("Skill", this.Skill?.ToJsonRef());

            return json;
        }

        public override bool IsValid()
        {
            return (Skill != null);
        }
    }
}
