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
        public Skill? Skill { get; set; }
        public bool IsClassSkill { get; set; }

        public override void ResolveReferences()
        {
            Skill = BaseModel.Resolve(Skill, MasterRepository.Skills);
        }

        public override void FromJson(JsonObject json)
        {
            this.Skill = CreateRefFromJson<Skill>(json["Skill"]?.AsObject());
            this.IsClassSkill = json["IsClassSkill"]?.GetValue<bool>() ?? false;
        }

        public override JsonObject ToJson()
        {
            var json = new JsonObject();
            json.Add("Skill", this.Skill?.ToJsonRef());
            json.Add("IsClassSkill", this.IsClassSkill);

            return json;
        }
    }
}
