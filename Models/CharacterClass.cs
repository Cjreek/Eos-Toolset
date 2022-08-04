using Eos.Models.Tables;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models
{
    internal class CharacterClass : BaseModel
    {
        public String Name { get; set; } = "";
        public string NamePlural { get; set; } = "";
        public string Description { get; set; } = "";
        public IntPtr Icon { get; set; }
        public int HitDie { get; set; } = 8;
        public AttackBonusTable? AttackBonusTable { get; set; }
        public FeatsTable? Feats { get; set; }
        public SavingThrowTable? SavingThrows { get; set; }
        public SkillsTable? Skills { get; set; }
        public BonusFeatsTable? BonusFeats { get; set; }
        public SpellSlotTable? SpellSlots { get; set; }
        public KnownSpellsTable? KnownSpells { get; set; }
        public bool Playable { get; set; } = true;
        public IntPtr SpellCasterType { get; set; } // Std. Arkan/Divine oder Custom
        public int DefaultStr { get; set; } = 10;
        public int DefaultDex { get; set; } = 10;
        public int DefaultCon { get; set; } = 10;
        public int DefaultWis { get; set; } = 10;
        public int DefaultInt { get; set; } = 10;
        public int DefaultCha { get; set; } = 10;
        public AbilityType PrimaryAbility { get; set; } = AbilityType.STR;
        public Alignment AllowedAlignments { get; set; } = Alignments.All;
        public IntPtr Requirements { get; set; }
        public int MaxLevel { get; set; } = 0;
        public bool MulticlassXPPenalty { get; set; } = true;
        public int ArcaneCasterLevelMod { get; set; } = 0;
        public int DivineCasterLevelMod { get; set; } = 0;
        public int PreEpicMaxLevel { get; set; } = -1;
        public IntPtr Package { get; set; }
        public IntPtr StatGainTable { get; set; }
        public bool MemorizesSpells { get; set; } = true;
        public bool SpellbookRestricted { get; set; } = true;
        public bool PicksDomain { get; set; } = false;
        public bool PicksSchool { get; set; } = false;
        public bool CanLearnFromScrolls { get; set; } = false;
        public bool IsArcaneCaster { get; set; } = true;
        public bool HasSpellFailure { get; set; } = true;
        public AbilityType SpellcastingAbility { get; set; }
        public IntPtr Spellbook { get; set; }
        public double CasterLevelMultiplier { get; set; } = 1.0;
        public int MinCastingLevel { get; set; }
        public int MinAssociateLevel { get; set; }
        public bool CanCastSpontaneously { get; set; } = false;
    }
}
