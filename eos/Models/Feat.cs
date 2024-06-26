﻿using Eos.Nwn.Tlk;
using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using static Eos.Models.JsonUtils;

namespace Eos.Models
{
    public class Feat : BaseModel
    {
        private FeatCategory _toolsetCategory = FeatCategory.Other;
        private Feat? _requiredFeat1;
        private Feat? _requiredFeat2;
        private Spell? _onUseEffect;
        private Feat? _successorFeat;
        private MasterFeat? _masterFeat;
        private Feat? _requiredFeatSelection1;
        private Feat? _requiredFeatSelection2;
        private Feat? _requiredFeatSelection3;
        private Feat? _requiredFeatSelection4;
        private Feat? _requiredFeatSelection5;
        private Skill? _requiredSkill1;
        private Skill? _requiredSkill2;
        private CharacterClass? _minLevelClass;

        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public TLKStringSet Description { get; set; } = new TLKStringSet();
        public int? MinAttackBonus { get; set; }
        public int? MinStr { get; set; }
        public int? MinDex { get; set; }
        public int? MinInt { get; set; }
        public int? MinWis { get; set; }
        public int? MinCon { get; set; }
        public int? MinCha { get; set; }
        public int? MinSpellLevel { get; set; }
        public Feat? RequiredFeat1
        {
            get { return _requiredFeat1; }
            set { Set(ref _requiredFeat1, value); }
        }
        public Feat? RequiredFeat2
        {
            get { return _requiredFeat2; }
            set { Set(ref _requiredFeat2, value); }
        }
        public bool UseableByAllClasses { get; set; } = true;
        public AICategory? Category { get; set; }
        public Spell? OnUseEffect
        {
            get { return _onUseEffect; }
            set { Set(ref _onUseEffect, value); }
        }
        public Feat? SuccessorFeat
        {
            get { return _successorFeat; }
            set { Set(ref _successorFeat, value); }
        }
        public double? CRModifier { get; set; } = 1;
        public int? UsesPerDay { get; set; } = 0;
        public MasterFeat? MasterFeat
        {
            get { return _masterFeat; }
            set { Set(ref _masterFeat, value); }
        }
        public bool? TargetSelf { get; set; } = false;
        public Feat? RequiredFeatSelection1
        {
            get { return _requiredFeatSelection1; }
            set { Set(ref _requiredFeatSelection1, value); }
        }
        public Feat? RequiredFeatSelection2
        {
            get { return _requiredFeatSelection2; }
            set { Set(ref _requiredFeatSelection2, value); }
        }
        public Feat? RequiredFeatSelection3
        {
            get { return _requiredFeatSelection3; }
            set { Set(ref _requiredFeatSelection3, value); }
        }
        public Feat? RequiredFeatSelection4
        {
            get { return _requiredFeatSelection4; }
            set { Set(ref _requiredFeatSelection4, value); }
        }
        public Feat? RequiredFeatSelection5
        {
            get { return _requiredFeatSelection5; }
            set { Set(ref _requiredFeatSelection5, value); }
        }
        public Skill? RequiredSkill1
        {
            get { return _requiredSkill1; }
            set { Set(ref _requiredSkill1, value); }
        }
        public int? RequiredSkill1Minimum { get; set; }
        public Skill? RequiredSkill2
        {
            get { return _requiredSkill2; }
            set { Set(ref _requiredSkill2, value); }
        }
        public int? RequiredSkill2Minimum { get; set; }
        public FeatCategory ToolsetCategory
        {
            get { return _toolsetCategory; }
            set
            {
                if (_toolsetCategory != value)
                {
                    _toolsetCategory = value;
                    NotifyPropertyChangedWithRepository();
                }
            }
        }
        public bool? IsHostile { get; set; } = false;
        public int? MinLevel { get; set; }
        public CharacterClass? MinLevelClass
        {
            get { return _minLevelClass; }
            set { Set(ref _minLevelClass, value); }
        }
        public int? MaxLevel { get; set; }
        public int? MinFortitudeSave { get; set; }
        public bool RequiresEpic { get; set; } = false;
        public bool UseActionQueue { get; set; } = true;

        protected override void Initialize()
        {
            base.Initialize();
            Name = new TLKStringSet(() => NotifyPropertyChanged(nameof(Name)));
        }

        protected override TLKStringSet? GetTlkDisplayName()
        {
            var modelOverride = (Feat?)MasterRepository.Project.GetOverride(this);
            return modelOverride?.Name ?? this.Name;
        }

        public override String GetLabel()
        {
            return Name;
        }

        protected override void SetDefaultValues()
        {
            Name[MasterRepository.Project.DefaultLanguage].Text = "New Feat";
            Name[MasterRepository.Project.DefaultLanguage].TextF = "New Feat";
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            RequiredFeat1 = Resolve(RequiredFeat1, MasterRepository.Feats);
            RequiredFeat2 = Resolve(RequiredFeat2, MasterRepository.Feats);
            OnUseEffect = Resolve(OnUseEffect, MasterRepository.Spells);
            SuccessorFeat = Resolve(SuccessorFeat, MasterRepository.Feats);
            MasterFeat = Resolve(MasterFeat, MasterRepository.MasterFeats);
            RequiredFeatSelection1 = Resolve(RequiredFeatSelection1, MasterRepository.Feats);
            RequiredFeatSelection2 = Resolve(RequiredFeatSelection2, MasterRepository.Feats);
            RequiredFeatSelection3 = Resolve(RequiredFeatSelection3, MasterRepository.Feats);
            RequiredFeatSelection4 = Resolve(RequiredFeatSelection4, MasterRepository.Feats);
            RequiredFeatSelection5 = Resolve(RequiredFeatSelection5, MasterRepository.Feats);
            RequiredSkill1 = Resolve(RequiredSkill1, MasterRepository.Skills);
            RequiredSkill2 = Resolve(RequiredSkill2, MasterRepository.Skills);
            MinLevelClass = Resolve(MinLevelClass, MasterRepository.Classes);
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name.FromJson(json["Name"]?.AsObject());
            this.Description.FromJson(json["Description"]?.AsObject());
            this.Icon = json["Icon"]?.GetValue<String>();
            this.MinAttackBonus = json["MinAttackBonus"]?.GetValue<int>();
            this.MinStr = json["MinStr"]?.GetValue<int>();
            this.MinDex = json["MinDex"]?.GetValue<int>();
            this.MinInt = json["MinInt"]?.GetValue<int>();
            this.MinWis = json["MinWis"]?.GetValue<int>();
            this.MinCon = json["MinCon"]?.GetValue<int>();
            this.MinCha = json["MinCha"]?.GetValue<int>();
            this.MinSpellLevel = json["MinSpellLevel"]?.GetValue<int>();
            this.RequiredFeat1 = CreateRefFromJson<Feat>(json["RequiredFeat1"]?.AsObject());
            this.RequiredFeat2 = CreateRefFromJson<Feat>(json["RequiredFeat2"]?.AsObject());
            this.UseableByAllClasses = json["UseableByAllClasses"]?.GetValue<bool>() ?? true;
            this.Category = JsonToEnum<AICategory>(json["Category"]);
            this.OnUseEffect = CreateRefFromJson<Spell>(json["OnUseEffect"]?.AsObject());
            this.SuccessorFeat = CreateRefFromJson<Feat>(json["SuccessorFeat"]?.AsObject());
            this.CRModifier = json["CRModifier"]?.GetValue<double>();
            this.UsesPerDay = json["UsesPerDay"]?.GetValue<int>();
            this.MasterFeat = CreateRefFromJson<MasterFeat>(json["MasterFeat"]?.AsObject());
            this.TargetSelf = json["TargetSelf"]?.GetValue<bool>();
            this.RequiredFeatSelection1 = CreateRefFromJson<Feat>(json["RequiredFeatSelection1"]?.AsObject());
            this.RequiredFeatSelection2 = CreateRefFromJson<Feat>(json["RequiredFeatSelection2"]?.AsObject());
            this.RequiredFeatSelection3 = CreateRefFromJson<Feat>(json["RequiredFeatSelection3"]?.AsObject());
            this.RequiredFeatSelection4 = CreateRefFromJson<Feat>(json["RequiredFeatSelection4"]?.AsObject());
            this.RequiredFeatSelection5 = CreateRefFromJson<Feat>(json["RequiredFeatSelection5"]?.AsObject());
            this.RequiredSkill1 = CreateRefFromJson<Skill>(json["RequiredSkill1"]?.AsObject());
            this.RequiredSkill1Minimum = json["RequiredSkill1Minimum"]?.GetValue<int>();
            this.RequiredSkill2 = CreateRefFromJson<Skill>(json["RequiredSkill2"]?.AsObject());
            this.RequiredSkill2Minimum = json["RequiredSkill2Minimum"]?.GetValue<int>();
            this.ToolsetCategory = JsonToEnum<FeatCategory>(json["ToolsetCategory"]) ?? FeatCategory.Other;
            this.IsHostile = json["IsHostile"]?.GetValue<bool>();
            this.MinLevel = json["MinLevel"]?.GetValue<int>();
            this.MinLevelClass = CreateRefFromJson<CharacterClass>(json["MinLevelClass"]?.AsObject());
            this.MaxLevel = json["MaxLevel"]?.GetValue<int>();
            this.MinFortitudeSave = json["MinFortitudeSave"]?.GetValue<int>();
            this.RequiresEpic = json["RequiresEpic"]?.GetValue<bool>() ?? false;
            this.UseActionQueue = json["UseActionQueue"]?.GetValue<bool>() ?? false;
        }

        public override JsonObject ToJson()
        {
            var featJson = base.ToJson();
            featJson.Add("Name", this.Name.ToJson());
            featJson.Add("Description", this.Description.ToJson());
            featJson.Add("Icon", this.Icon);
            featJson.Add("MinAttackBonus", this.MinAttackBonus);
            featJson.Add("MinStr", this.MinStr);
            featJson.Add("MinDex", this.MinDex);
            featJson.Add("MinInt", this.MinInt);
            featJson.Add("MinWis", this.MinWis);
            featJson.Add("MinCon", this.MinCon);
            featJson.Add("MinCha", this.MinCha);
            featJson.Add("MinSpellLevel", this.MinSpellLevel);
            featJson.Add("RequiredFeat1", CreateJsonRef(this.RequiredFeat1));
            featJson.Add("RequiredFeat2", CreateJsonRef(this.RequiredFeat2));
            featJson.Add("UseableByAllClasses", this.UseableByAllClasses);
            featJson.Add("Category", EnumToJson(this.Category));
            featJson.Add("OnUseEffect", CreateJsonRef(this.OnUseEffect));
            featJson.Add("SuccessorFeat", CreateJsonRef(this.SuccessorFeat));
            featJson.Add("CRModifier", this.CRModifier);
            featJson.Add("UsesPerDay", this.UsesPerDay);
            featJson.Add("MasterFeat", CreateJsonRef(this.MasterFeat));
            featJson.Add("TargetSelf", this.TargetSelf);
            featJson.Add("RequiredFeatSelection1", CreateJsonRef(this.RequiredFeatSelection1));
            featJson.Add("RequiredFeatSelection2", CreateJsonRef(this.RequiredFeatSelection2));
            featJson.Add("RequiredFeatSelection3", CreateJsonRef(this.RequiredFeatSelection3));
            featJson.Add("RequiredFeatSelection4", CreateJsonRef(this.RequiredFeatSelection4));
            featJson.Add("RequiredFeatSelection5", CreateJsonRef(this.RequiredFeatSelection5));
            featJson.Add("RequiredSkill1", CreateJsonRef(this.RequiredSkill1));
            featJson.Add("RequiredSkill1Minimum", this.RequiredSkill1Minimum);
            featJson.Add("RequiredSkill2", CreateJsonRef(this.RequiredSkill2));
            featJson.Add("RequiredSkill2Minimum", this.RequiredSkill2Minimum);
            featJson.Add("ToolsetCategory", EnumToJson(ToolsetCategory));
            featJson.Add("IsHostile", this.IsHostile);
            featJson.Add("MinLevel", this.MinLevel);
            featJson.Add("MinLevelClass", CreateJsonRef(this.MinLevelClass));
            featJson.Add("MaxLevel", this.MaxLevel);
            featJson.Add("MinFortitudeSave", this.MinFortitudeSave);
            featJson.Add("RequiresEpic", this.RequiresEpic);
            featJson.Add("UseActionQueue", this.UseActionQueue);

            return featJson;
        }
    }
}
