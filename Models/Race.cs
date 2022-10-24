using Eos.Models.Tables;
using Eos.Nwn.Tlk;
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
    public class Race : BaseModel
    {
        private Appearance? _appearance;
        private CharacterClass? _favoredClass;
        private CharacterClass? _toolsetDefaultClass;
        private Feat? _favoredEnemyFeat;
        private RacialFeatsTable? _feats;

        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public TLKStringSet NamePlural { get; set; } = new TLKStringSet();
        public TLKStringSet Adjective { get; set; } = new TLKStringSet();
        public TLKStringSet Description { get; set; } = new TLKStringSet();
        public Appearance? Appearance
        {
            get { return _appearance; }
            set { Set(ref _appearance, value); }
        }
        public int StrAdjustment { get; set; } = 0;
        public int DexAdjustment { get; set; } = 0;
        public int IntAdjustment { get; set; } = 0;
        public int ChaAdjustment { get; set; } = 0;
        public int WisAdjustment { get; set; } = 0;
        public int ConAdjustment { get; set; } = 0;
        public CharacterClass? FavoredClass
        {
            get { return _favoredClass; }
            set { Set(ref _favoredClass, value); }
        }

        public TLKStringSet Biography { get; set; } = new TLKStringSet();
        public bool? Playable { get; set; } = false;
        public int? DefaultAge { get; set; }
        public CharacterClass? ToolsetDefaultClass
        {
            get { return _toolsetDefaultClass; }
            set { Set(ref _toolsetDefaultClass, value); }
        }
        public double? CRModifier { get; set; } = 1;
        public String? NameGenTableA { get; set; }
        public String? NameGenTableB { get; set; }
        public int FirstLevelExtraFeats { get; set; } = 0;
        public int ExtraSkillPointsPerLevel { get; set; } = 0;
        public int? FirstLevelSkillPointsMultiplier { get; set; } = 4;
        public int? FirstLevelAbilityPoints { get; set; } = 30;
        public int? FeatEveryNthLevel { get; set; } = 3;
        public int? FeatEveryNthLevelCount { get; set; } = 1;
        public AbilityType? SkillPointModifierAbility { get; set; } = AbilityType.INT;
        public Feat? FavoredEnemyFeat
        {
            get { return _favoredEnemyFeat; }
            set { Set(ref _favoredEnemyFeat, value); }
        }
        public RacialFeatsTable? Feats
        {
            get { return _feats; }
            set { Set(ref _feats, value); }
        }

        protected override String GetLabel()
        {
            return Name;
        }

        protected override void SetDefaultValues()
        {
            Name[MasterRepository.Project.DefaultLanguage].Text = "New Race";
            Name[MasterRepository.Project.DefaultLanguage].TextF = "New Race";
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            FavoredClass = Resolve(FavoredClass, MasterRepository.Classes);
            ToolsetDefaultClass = Resolve(ToolsetDefaultClass, MasterRepository.Classes);
            FavoredEnemyFeat = Resolve(FavoredEnemyFeat, MasterRepository.Feats);
            Feats = Resolve(Feats, MasterRepository.RacialFeatsTables);
            Appearance = Resolve(Appearance, MasterRepository.Appearances);
        }

        public override JsonObject ToJson()
        {
            var raceJson = base.ToJson();
            raceJson.Add("Name", this.Name.ToJson());
            raceJson.Add("NamePlural", this.NamePlural.ToJson());
            raceJson.Add("Adjective", this.Adjective.ToJson());
            raceJson.Add("Description", this.Description.ToJson());
            raceJson.Add("Icon", this.Icon);
            raceJson.Add("Appearance", CreateJsonRef(this.Appearance));
            raceJson.Add("StrAdjustment", this.StrAdjustment);
            raceJson.Add("DexAdjustment", this.DexAdjustment);
            raceJson.Add("IntAdjustment", this.IntAdjustment);
            raceJson.Add("ChaAdjustment", this.ChaAdjustment);
            raceJson.Add("WisAdjustment", this.WisAdjustment);
            raceJson.Add("ConAdjustment", this.ConAdjustment);
            raceJson.Add("FavoredClass", CreateJsonRef(this.FavoredClass));
            raceJson.Add("Biography", this.Biography.ToJson());
            raceJson.Add("Playable", this.Playable);
            raceJson.Add("DefaultAge", this.DefaultAge);
            raceJson.Add("ToolsetDefaultClass", CreateJsonRef(this.ToolsetDefaultClass));
            raceJson.Add("CRModifier", this.CRModifier);
            raceJson.Add("NameGenTableA", this.NameGenTableA);
            raceJson.Add("NameGenTableB", this.NameGenTableB);
            raceJson.Add("FirstLevelExtraFeats", this.FirstLevelExtraFeats);
            raceJson.Add("ExtraSkillPointsPerLevel", this.ExtraSkillPointsPerLevel);
            raceJson.Add("FirstLevelSkillPointsMultiplier", this.FirstLevelSkillPointsMultiplier);
            raceJson.Add("FirstLevelAbilityPoints", this.FirstLevelAbilityPoints);
            raceJson.Add("FeatEveryNthLevel", this.FeatEveryNthLevel);
            raceJson.Add("FeatEveryNthLevelCount", this.FeatEveryNthLevelCount);
            raceJson.Add("SkillPointModifierAbility", EnumToJson(this.SkillPointModifierAbility));
            raceJson.Add("FavoredEnemyFeat", CreateJsonRef(this.FavoredEnemyFeat));
            raceJson.Add("Feats", CreateJsonRef(this.Feats));

            return raceJson;
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name.FromJson(json["Name"]?.AsObject());
            this.NamePlural.FromJson(json["NamePlural"]?.AsObject());
            this.Adjective.FromJson(json["Adjective"]?.AsObject());
            this.Description.FromJson(json["Description"]?.AsObject());
            this.Icon = json["Icon"]?.GetValue<String>();
            this.Appearance = CreateRefFromJson<Appearance>(json["Appearance"]?.AsObject());
            this.StrAdjustment = json["StrAdjustment"]?.GetValue<int>() ?? 0;
            this.DexAdjustment = json["DexAdjustment"]?.GetValue<int>() ?? 0;
            this.IntAdjustment = json["IntAdjustment"]?.GetValue<int>() ?? 0;
            this.ChaAdjustment = json["ChaAdjustment"]?.GetValue<int>() ?? 0;
            this.WisAdjustment = json["WisAdjustment"]?.GetValue<int>() ?? 0;
            this.ConAdjustment = json["ConAdjustment"]?.GetValue<int>() ?? 0;
            this.FavoredClass = CreateRefFromJson<CharacterClass>(json["FavoredClass"]?.AsObject());
            this.Biography.FromJson(json["Biography"]?.AsObject());
            this.Playable = json["Playable"]?.GetValue<bool>() ?? false;
            this.DefaultAge = json["DefaultAge"]?.GetValue<int>();
            this.ToolsetDefaultClass = CreateRefFromJson<CharacterClass>(json["ToolsetDefaultClass"]?.AsObject());
            this.CRModifier = json["CRModifier"]?.GetValue<double>();
            this.NameGenTableA = json["NameGenTableA"]?.GetValue<String>();
            this.NameGenTableB = json["NameGenTableB"]?.GetValue<String>();
            this.FirstLevelExtraFeats = json["FirstLevelExtraFeats"]?.GetValue<int>() ?? 0;
            this.ExtraSkillPointsPerLevel = json["ExtraSkillPointsPerLevel"]?.GetValue<int>() ?? 0;
            this.FirstLevelSkillPointsMultiplier = json["FirstLevelSkillPointsMultiplier"]?.GetValue<int>();
            this.FirstLevelAbilityPoints = json["FirstLevelAbilityPoints"]?.GetValue<int>();
            this.FeatEveryNthLevel = json["FeatEveryNthLevel"]?.GetValue<int>();
            this.FeatEveryNthLevelCount = json["FeatEveryNthLevelCount"]?.GetValue<int>();
            this.SkillPointModifierAbility = JsonToEnum<AbilityType>(json["SkillPointModifierAbility"]) ?? AbilityType.INT;
            this.FavoredEnemyFeat = CreateRefFromJson<Feat>(json["FavoredEnemyFeat"]?.AsObject());
            this.Feats = CreateRefFromJson<RacialFeatsTable>(json["Feats"]?.AsObject());
        }
    }
}
