using Eos.Models.Tables;
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
        public int HitDie { get; set; }
        public AttackBonusTable? AttackBonusTable { get; set; }
        public FeatsTable? Feats { get; set; }
        public SavingThrowTable? SavingThrows { get; set; }
        public SkillsTable? Skills { get; set; }
        public BonusFeatsTable? BonusFeats { get; set; }
        public SpellSlotTable? SpellSlots { get; set; }
        public KnownSpellsTable? KnownSpells { get; set; }
        public bool Playable { get; set; }
        public IntPtr SpellCasterType { get; set; } // Std. Arkan/Divine oder Custom
}
}
