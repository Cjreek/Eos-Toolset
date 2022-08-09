using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models
{
    internal class Race : BaseModel
    {
        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public TLKStringSet NamePlural { get; set; } = new TLKStringSet();
        public TLKStringSet Adjective { get; set; } = new TLKStringSet();
        public TLKStringSet Description { get; set; } = new TLKStringSet();
        public IntPtr Icon { get; set; }
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
        public int? FirstLevelExtraFeats { get; set; } = 0;
        public int? ExtraSkillPointsPerLevel { get; set; } = 0;
        public int? FirstLevelSkillPointsMultiplier { get; set; } = 4;
        public int? FirstLevelAbilityPoints { get; set; } = 30;
        public int? FeatEveryNthLevel { get; set; } = 3;
        public int? FeatEveryNthLevelCount { get; set; } = 1;
        public AbilityType SkillPointModifierAbility { get; set; } = AbilityType.INT;
        public List<Feat> Feats { get; } = new List<Feat>();
    }
}
