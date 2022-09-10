using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using static Eos.Models.JsonUtils;

namespace Eos.Models.Tables
{
    public class PrerequisiteTableItem : TableItem
    {
        public RequirementType RequirementType { get; set; }
        public object? RequirementParam1 { get; set; }
        public object? RequirementParam2 { get; set; }

        private object? JsonToParam(JsonNode? node)
        {
            if (node == null) return null;
            var element = node.GetValue<JsonElement>();
            
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt32(out int intValue))
                        return intValue;
                    else
                        return element.GetDouble();
            }

            return null;
        }

        public override void FromJson(JsonObject json)
        {
            this.RequirementType = JsonToEnum<RequirementType>(json["RequirementType"]) ?? RequirementType.VAR;
            switch (this.RequirementType)
            {
                case RequirementType.CLASSOR:
                case RequirementType.CLASSNOT:
                    this.RequirementParam1 = CreateRefFromJson<CharacterClass>(json["RequirementParam1"]?.AsObject());
                    break;

                case RequirementType.FEAT:
                case RequirementType.FEATOR:
                    this.RequirementParam1 = CreateRefFromJson<Feat>(json["RequirementParam1"]?.AsObject());
                    break;

                case RequirementType.RACE:
                    this.RequirementParam1 = CreateRefFromJson<Race>(json["RequirementParam1"]?.AsObject());
                    break;

                case RequirementType.SKILL:
                    this.RequirementParam1 = CreateRefFromJson<Skill>(json["RequirementParam1"]?.AsObject());
                    break;

                default:
                    this.RequirementParam1 = JsonToParam(json["RequirementParam1"]);
                    break;
            }
            this.RequirementParam2 = JsonToParam(json["RequirementParam2"]);
        }

        private JsonNode? ParamToJson(object? param)
        {
            if (param == null)
                return null;
            if (param is int intValue)
                return JsonValue.Create(intValue);
            if (param is String strValue)
                return JsonValue.Create(strValue);

            return null;
        }

        public override JsonObject ToJson()
        {
            var json = new JsonObject();
            json.Add("RequirementType", EnumToJson(this.RequirementType));
            switch (this.RequirementType)
            {
                case RequirementType.CLASSOR:
                case RequirementType.CLASSNOT:
                    json.Add("RequirementParam1", ((CharacterClass?)this.RequirementParam1)?.ToJsonRef());
                    break;

                case RequirementType.FEAT:
                case RequirementType.FEATOR:
                    json.Add("RequirementParam1", ((Feat?)this.RequirementParam1)?.ToJsonRef());
                    break;

                case RequirementType.RACE:
                    json.Add("RequirementParam1", ((Race?)this.RequirementParam1)?.ToJsonRef());
                    break;

                case RequirementType.SKILL:
                    json.Add("RequirementParam1", ((Skill?)this.RequirementParam1)?.ToJsonRef());
                    break;

                default:
                    json.Add("RequirementParam1", ParamToJson(this.RequirementParam1));
                    break;
            }
            json.Add("RequirementParam2", ParamToJson(this.RequirementParam2));

            return json;
        }
    }
}
