using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models
{
    public class Spell : BaseModel
    {
        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public TLKStringSet Description { get; set; } = new TLKStringSet();
        public IntPtr Icon { get; set; }
        public SpellSchool School { get; set; } = SpellSchool.G;
        public SpellRange Range { get; set; } = SpellRange.P;
        public SpellComponent Components { get; set; } = SpellComponent.V | SpellComponent.S;
        public MetaMagicType AvailableMetaMagic { get; set; } = (MetaMagicType)0;
        public SpellTarget TargetTypes { get; set; } = SpellTarget.Self;
        public IntPtr ImpactScript { get; set; }
        public int ConjurationTime { get; set; } = 1500;
        public SpellConjureAnimation ConjuringAnimation { get; set; } = SpellConjureAnimation.Hand;
        public IntPtr ConjurationHeadEffect { get; set; }
        public IntPtr ConjurationHandEffect { get; set; }
        public IntPtr ConjurationGroundEffect { get; set; }
        public IntPtr ConjurationSound { get; set; }
        public IntPtr ConjurationMaleSound { get; set; }
        public IntPtr ConjurationFemaleSound { get; set; }
        public SpellCastAnimation CastingAnimation { get; set; } = SpellCastAnimation.Out;
        public int CastTime { get; set; } = 1000;
        public IntPtr CastingHeadEffect { get; set; }
        public IntPtr CastingHandEffect { get; set; }
        public IntPtr CastingGroundEffect { get; set; }
        public IntPtr CastingSound { get; set; }
        public bool HasProjectile { get; set; } = false;
        public IntPtr ProjectileModel { get; set; }
        public ProjectileType ProjectileType { get; set; } = ProjectileType.Homing;
        public ProjectileSource ProjectileSpawnPoint { get; set; } = ProjectileSource.Hand;
        public IntPtr ProjectileSound { get; set; }
        public ProjectileOrientation ProjectileOrientation { get; set; } = ProjectileOrientation.Path;
        public Spell? SubSpell1 { get; set; }
        public Spell? SubSpell2 { get; set; }
        public Spell? SubSpell3 { get; set; }
        public Spell? SubSpell4 { get; set; }
        public Spell? SubSpell5 { get; set; }
        public Spell? SubSpell6 { get; set; }
        public Spell? SubSpell7 { get; set; }
        public Spell? SubSpell8 { get; set; }
        public AICategory? Category { get; set; }
        public Spell? ParentSpell { get; set; }
        public SpellType Type { get; set; } = SpellType.Spell;
        //public Feat? FeatReference { get; set; } // Deprecated? Determine dynamically
        public Spell? CounterSpell1 { get; set; }
        public Spell? CounterSpell2 { get; set; }
    }
}
