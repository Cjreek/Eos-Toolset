using Eos.Nwn.Tlk;
using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Eos.Models
{
    public class Race : BaseModel
    {
        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public TLKStringSet NamePlural { get; set; } = new TLKStringSet();
        public TLKStringSet Adjective { get; set; } = new TLKStringSet();
        public TLKStringSet Description { get; set; } = new TLKStringSet();
        public IntPtr Appearance { get; set; }
        public int StrAdjustment { get; set; } = 0;
        public int DexAdjustment { get; set; } = 0;
        public int IntAdjustment { get; set; } = 0;
        public int ChaAdjustment { get; set; } = 0;
        public int WisAdjustment { get; set; } = 0;
        public int ConAdjustment { get; set; } = 0;
        public CharacterClass? FavoredClass { get; set; }
        public TLKStringSet Biography { get; set; } = new TLKStringSet();
        public bool? Playable { get; set; } = false;
        public int? DefaultAge { get; set; }
        public CharacterClass? ToolsetDefaultClass { get; set; }
        public double? CRModifier { get; set; } = 1;
        public IntPtr NameGenTableA { get; set; }
        public IntPtr NameGenTableB { get; set; }
        public int FirstLevelExtraFeats { get; set; } = 0;
        public int ExtraSkillPointsPerLevel { get; set; } = 0;
        public int? FirstLevelSkillPointsMultiplier { get; set; } = 4;
        public int? FirstLevelAbilityPoints { get; set; } = 30;
        public int? FeatEveryNthLevel { get; set; } = 3;
        public int? FeatEveryNthLevelCount { get; set; } = 1;
        public AbilityType SkillPointModifierAbility { get; set; } = AbilityType.INT;
        public List<Feat> Feats { get; } = new List<Feat>();

        protected override String GetLabel()
        {
            return Name;
        }

        public override void ResolveReferences()
        {
            FavoredClass = Resolve(FavoredClass, MasterRepository.Classes);
            ToolsetDefaultClass = Resolve(ToolsetDefaultClass, MasterRepository.Classes);
            for (int i = Feats.Count-1; i >= 0; i--)
            {
                var resolvedFeat = Resolve(Feats[i], MasterRepository.Feats);
                if (resolvedFeat != null)
                    Feats[i] = resolvedFeat;
                else
                    Feats.RemoveAt(i);
            }
        }

        public override JsonObject ToJson()
        {
            var raceJson = new JsonObject();
            raceJson.Add("ID", this.ID.ToString());
            raceJson.Add("Index", this.Index);
            raceJson.Add("Name", this.Name.ToJson());
            raceJson.Add("NamePlural", this.NamePlural.ToJson());
            raceJson.Add("Adjective", this.Adjective.ToJson());
            raceJson.Add("Description", this.Description.ToJson());
            raceJson.Add("Icon", this.Icon);
            raceJson.Add("Appearance", null);
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
            raceJson.Add("NameGenTableA", null);
            raceJson.Add("NameGenTableB", null);
            raceJson.Add("FirstLevelExtraFeats", this.FirstLevelExtraFeats);
            raceJson.Add("ExtraSkillPointsPerLevel", this.ExtraSkillPointsPerLevel);
            raceJson.Add("FirstLevelSkillPointsMultiplier", this.FirstLevelSkillPointsMultiplier);
            raceJson.Add("FirstLevelAbilityPoints", this.FirstLevelAbilityPoints);
            raceJson.Add("FeatEveryNthLevel", this.FeatEveryNthLevel);
            raceJson.Add("FeatEveryNthLevelCount", this.FeatEveryNthLevelCount);
            raceJson.Add("SkillPointModifierAbility", EnumToJson(this.SkillPointModifierAbility));

            var featArr = new JsonArray();
            for (int i=0; i < this.Feats.Count; i++)
                featArr.Add(CreateJsonRef(this.Feats[i]));
            raceJson.Add("Feats", featArr);

            return raceJson;
        }

        public override void FromJson(JsonObject json)
        {
            this.ID = ParseGuid(json["ID"]?.GetValue<String>());
            this.Index = json["Index"]?.GetValue<int?>();
            this.Name.FromJson(json["Name"]?.AsObject());
            this.NamePlural.FromJson(json["NamePlural"]?.AsObject());
            this.Adjective.FromJson(json["Adjective"]?.AsObject());
            this.Description.FromJson(json["Description"]?.AsObject());
            this.Icon = json["Icon"]?.GetValue<String>();
            this.Appearance = IntPtr.Zero; // !
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
            this.NameGenTableA = IntPtr.Zero; // !
            this.NameGenTableB = IntPtr.Zero; // !
            this.FirstLevelExtraFeats = json["FirstLevelExtraFeats"]?.GetValue<int>() ?? 0;
            this.ExtraSkillPointsPerLevel = json["ExtraSkillPointsPerLevel"]?.GetValue<int>() ?? 0;
            this.FirstLevelSkillPointsMultiplier = json["FirstLevelSkillPointsMultiplier"]?.GetValue<int>();
            this.FirstLevelAbilityPoints = json["FirstLevelAbilityPoints"]?.GetValue<int>();
            this.FeatEveryNthLevel = json["FeatEveryNthLevel"]?.GetValue<int>();
            this.FeatEveryNthLevelCount = json["FeatEveryNthLevelCount"]?.GetValue<int>();
            this.SkillPointModifierAbility = JsonToEnum<AbilityType>(json["SkillPointModifierAbility"]) ?? AbilityType.INT;

            this.Feats.Clear();
            var featArr = json["Feats"]?.AsArray() ?? new JsonArray();
            for (int i=0; i < featArr.Count; i++)
            {
                var feat = CreateRefFromJson<Feat>(featArr[i]?.AsObject());
                if (feat != null)
                    this.Feats.Add(feat);
            }
        }
    }
}
