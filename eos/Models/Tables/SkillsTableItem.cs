using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using static Eos.Models.JsonUtils;

namespace Eos.Models.Tables
{
    public class SkillsTableItem : TableItem
    {
        private Skill? _skill;

        public Skill? Skill
        {
            get { return _skill; }
            set { Set(ref _skill, value); }
        }

        public bool IsClassSkill { get; set; }

        public SkillsTableItem() : base()
        {
        }

        public SkillsTableItem(SkillsTable parentTable) : base(parentTable)
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
            this.IsClassSkill = json["IsClassSkill"]?.GetValue<bool>() ?? false;
        }

        public override JsonObject ToJson()
        {
            var json = base.ToJson();
            json.Add("Skill", this.Skill?.ToJsonRef());
            json.Add("IsClassSkill", this.IsClassSkill);

            return json;
        }

        public override bool IsValid()
        {
            return (Skill != null);
        }
    }
}
