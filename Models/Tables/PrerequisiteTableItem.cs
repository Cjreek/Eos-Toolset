using Eos.Repositories;
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
        private RequirementType requirementType;
        private object? _requirementParam1;

        public RequirementType RequirementType
        {
            get { return requirementType; }
            set
            {
                if (requirementType != value)
                {
                    requirementType = value;
                    RequirementParam1 = null;
                    RequirementParam2 = null;
                    NotifyPropertyChanged();
                }
            }
        }

        public object? RequirementParam1
        {
            get { return _requirementParam1; }
            set 
            { 
                if (_requirementParam1 != value)
                {
                    if ((_requirementParam1 is BaseModel oldValue) && (ParentTable != null))
                        oldValue.RemoveReference(ParentTable);
                    _requirementParam1 = value;
                    if ((_requirementParam1 is BaseModel newValue) && (ParentTable != null))
                        newValue.AddReference(ParentTable);
                }
            }
        }
        public object? RequirementParam2 { get; set; }

        private object? JsonToParam(JsonNode? node)
        {
            if (node == null) return null;
            if (node.AsValue().TryGetValue<JsonElement>(out JsonElement element))
            {
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
            }
            else
            {
                if (node.AsValue().TryGetValue<String>(out String? strValue))
                    return strValue;
                if (node.AsValue().TryGetValue<int>(out int intValue))
                    return intValue;
                if (node.AsValue().TryGetValue<double>(out double doubleValue))
                    return doubleValue;
            }

            return null;
        }

        public override void ResolveReferences()
        {
            switch (this.RequirementType)
            {
                case RequirementType.CLASSOR:
                case RequirementType.CLASSNOT:
                    this.RequirementParam1 = MasterRepository.Classes.Resolve((CharacterClass?)RequirementParam1);
                    break;

                case RequirementType.FEAT:
                case RequirementType.FEATOR:
                    this.RequirementParam1 = MasterRepository.Feats.Resolve((Feat?)RequirementParam1);
                    break;

                case RequirementType.RACE:
                    this.RequirementParam1 = MasterRepository.Races.Resolve((Race?)RequirementParam1);
                    break;

                case RequirementType.SKILL:
                    this.RequirementParam1 = MasterRepository.Skills.Resolve((Skill?)RequirementParam1);
                    break;
            }
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
                return JsonValue.Create<int>(intValue);
            if (param is String strValue)
                return JsonValue.Create<String>(strValue);

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

        public override bool IsValid()
        {
            return (RequirementParam1 != null);
        }
    }
}
