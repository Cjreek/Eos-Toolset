using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

using static Eos.Models.JsonUtils;

namespace Eos.Models.Tables
{
    public class PrerequisiteTableItem : TableItem
    {
        private RequirementType requirementType;
        private CharacterClass? _param1Class;
        private Feat? _param1Feat;
        private Race? _param1Race;
        private Skill? _param1Skill;
        private string? _param1String;
        private int? _param1Int;
        private int? _param1Save;

        public RequirementType RequirementType
        {
            get { return requirementType; }
            set
            {
                if ((requirementType != value) && (value != RequirementType.UNDEFINED))
                {
                    requirementType = value;
                    ClearParams();
                    NotifyPropertyChanged();
                }
            }
        }

        public object? RequirementParam1
        {
            get 
            { 
                switch (requirementType)
                {
                    case RequirementType.SAVE:
                        return Param1Save;

                    case RequirementType.ARCSPELL:
                    case RequirementType.BAB:
                    case RequirementType.SPELL:
                        return Param1Int;

                    case RequirementType.CLASSNOT:
                    case RequirementType.CLASSOR:
                        return Param1Class;

                    case RequirementType.FEAT:
                    case RequirementType.FEATOR:
                        return Param1Feat;

                    case RequirementType.RACE:
                        return Param1Race;

                    case RequirementType.SKILL:
                        return Param1Skill;

                    case RequirementType.VAR:
                        return Param1String;
                }

                return null;
            }
        }

        public int? RequirementParam2 { get; set; }

        public CharacterClass? Param1Class
        {
            get { return _param1Class; }
            set
            {
                if (_param1Class != value)
                {
                    if ((_param1Class != null) && (ParentTable != null))
                        _param1Class.RemoveReference(ParentTable);

                    _param1Class = value;

                    if ((_param1Class != null) && (ParentTable != null))
                        _param1Class.AddReference(ParentTable);

                    NotifyPropertyChanged();
                }
            }
        }

        public Feat? Param1Feat
        {
            get { return _param1Feat; }
            set
            {
                if (_param1Feat != value)
                {
                    if ((_param1Feat != null) && (ParentTable != null))
                        _param1Feat.RemoveReference(ParentTable);

                    _param1Feat = value;

                    if ((_param1Feat != null) && (ParentTable != null))
                        _param1Feat.AddReference(ParentTable);

                    NotifyPropertyChanged();
                }
            }
        }

        public Race? Param1Race
        {
            get { return _param1Race; }
            set
            {
                if (_param1Race != value)
                {
                    if ((_param1Race != null) && (ParentTable != null))
                        _param1Race.RemoveReference(ParentTable);

                    _param1Race = value;

                    if ((_param1Race != null) && (ParentTable != null))
                        _param1Race.AddReference(ParentTable);

                    NotifyPropertyChanged();
                }
            }
        }
        public Skill? Param1Skill
        {
            get { return _param1Skill; }
            set
            {
                if (_param1Skill != value)
                {
                    if ((_param1Skill != null) && (ParentTable != null))
                        _param1Skill.RemoveReference(ParentTable);

                    _param1Skill = value;

                    if ((_param1Skill != null) && (ParentTable != null))
                        _param1Skill.AddReference(ParentTable);

                    NotifyPropertyChanged();
                }
            }
        }

        public string? Param1String
        {
            get { return _param1String; }
            set
            {
                if (_param1String != value)
                {
                    _param1String = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int? Param1Save
        {
            get { return _param1Save; }
            set
            {
                if (_param1Save != value)
                {
                    _param1Save = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int? Param1Int
        {
            get { return _param1Int; }
            set
            {
                if (_param1Int != value)
                {
                    _param1Int = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public void ClearParams()
        {
            Param1Class = null;
            Param1Skill = null;
            Param1String = null;
            Param1Int = null;
            Param1Save = null;
            Param1Feat = null;
            Param1Race = null;
            RequirementParam2 = null;
        }

        public override void ResolveReferences()
        {
            switch (this.RequirementType)
            {
                case RequirementType.CLASSOR:
                case RequirementType.CLASSNOT:
                    this.Param1Class = MasterRepository.Classes.Resolve(Param1Class);
                    break;

                case RequirementType.FEAT:
                case RequirementType.FEATOR:
                    this.Param1Feat = MasterRepository.Feats.Resolve(Param1Feat);
                    break;

                case RequirementType.RACE:
                    this.Param1Race = MasterRepository.Races.Resolve(Param1Race);
                    break;

                case RequirementType.SKILL:
                    this.Param1Skill = MasterRepository.Skills.Resolve(Param1Skill);
                    break;
            }
        }

        public override void FromJson(JsonObject json)
        {
            this.RequirementType = JsonToEnum<RequirementType>(json["RequirementType"]) ?? RequirementType.VAR;

            this.Param1Class = CreateRefFromJson<CharacterClass>(json["Param1Class"]?.AsObject());
            this.Param1Feat = CreateRefFromJson<Feat>(json["Param1Feat"]?.AsObject());
            this.Param1Race = CreateRefFromJson<Race>(json["Param1Race"]?.AsObject());
            this.Param1Skill = CreateRefFromJson<Skill>(json["Param1Skill"]?.AsObject());
            this.Param1String = json["Param1String"]?.GetValue<string>();
            this.Param1Int = json["Param1Int"]?.GetValue<int>();
            this.Param1Save = json["Param1Save"]?.GetValue<int>();

            this.RequirementParam2 = json["RequirementParam2"]?.GetValue<int>();
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
            var json = new JsonObject
            {
                { "RequirementType", EnumToJson(RequirementType) },
                { "Param1Class", Param1Class?.ToJsonRef() },
                { "Param1Feat", Param1Feat?.ToJsonRef() },
                { "Param1Race", Param1Race?.ToJsonRef() },
                { "Param1Skill", Param1Skill?.ToJsonRef() },
                { "Param1String", Param1String },
                { "Param1Int", Param1Int },
                { "Param1Save", Param1Save },

                { "RequirementParam2", RequirementParam2 }
            };

            return json;
        }

        public override bool IsValid()
        {
            return (RequirementParam1 != null);
        }
    }
}
