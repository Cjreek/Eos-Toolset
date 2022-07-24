using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models
{
    internal class Race
    {
        public string? Name { get; set; }
        public string? NamePlural { get; set; }
        public string? Adjective { get; set; }
        public string? AdjectiveLower { get; set; }
        public string? Description { get; set; }
        public IntPtr Icon { get; set; }
        public IntPtr Appearance { get; set; }
        public int? StrAdjustment { get; set; }
        public int? DexAdjustment { get; set; }
        public int? IntAdjustment { get; set; }
        public int? ChaAdjustment { get; set; }
        public int? WisAdjustment { get; set; }
        public int? ConAdjustment { get; set; }
        public IntPtr FavoredClass { get; set; }
        public string? Biography { get; set; }
        public bool? Playable { get; set; }
        public int? DefaultAge { get; set; }
        public IntPtr ToolsetDefaultClass { get; set; }
        public double? CRModifier { get; set; }
        public IntPtr NameGenTableA { get; set; }
        public IntPtr NameGenTableB { get; set; }
        public int? FirstLevelExtraFeats { get; set; }
        public int? ExtraSkillPointsPerLevel { get; set; }
        public int? FirstLevelSkillPointsMultiplier { get; set; }
        public int? FirstLevelAbilityPoints { get; set; }
        public int? FeatEveryNthLevel { get; set; }
        public int? FeatEveryNthLevelCount { get; set; }
        public AbilityType SkillPointModifierAbility { get; set; } = AbilityType.Intelligence;
    }
}
