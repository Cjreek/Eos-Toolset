using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models
{
    public class Feat : BaseModel
    {
        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public TLKStringSet Description { get; set; } = new TLKStringSet();
        public IntPtr Icon { get; set; }
        public int? MinAttackBonus { get; set; }
        public int? MinStr { get; set; }
        public int? MinDex { get; set; }
        public int? MinInt { get; set; }
        public int? MinWis { get; set; }
        public int? MinCon { get; set; }
        public int? MinCha { get; set; }
        public int? MinSpellLevel { get; set; }
        public Feat? RequiredFeat1 { get; set; }
        public Feat? RequiredFeat2 { get; set; }
        public bool UseableByAllClasses { get; set; } = true;
        public AICategory Category { get; set; }
        public Spell? OnUseEffect { get; set; }
        public Feat? SuccessorFeat { get; set; }
        public double? CRModifier { get; set; } = 1;
        public int? UsesPerDay { get; set; } = 0;
        public Feat? MasterFeat { get; set; }
        public bool? TargetSelf { get; set; } = false;
        public Feat? RequiredFeatSelection1 { get; set; }
        public Feat? RequiredFeatSelection2 { get; set; }
        public Feat? RequiredFeatSelection3 { get; set; }
        public Feat? RequiredFeatSelection4 { get; set; }
        public Feat? RequiredFeatSelection5 { get; set; }
        public Skill? RequiredSkill1 { get; set; }
        public int? RequiredSkill1Minimum { get; set; }
        public Skill? RequiredSkill2 { get; set; }
        public int? RequiredSkill2Minimum { get; set; }
        public FeatCategory ToolsetCategory { get; set; } = FeatCategory.Other;
        public bool? IsHostile { get; set; } = false;
        public int? MinLevel { get; set; }
        public CharacterClass? MinLevelClass { get; set; }
        public int? MaxLevel { get; set; }
        public int? MinFortitudeSave { get; set; }
        public bool RequiresEpic { get; set; } = false;
        public bool UseActionQueue { get; set; } = true;
    }
}
