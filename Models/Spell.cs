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
        public TLKStringSet AlternativeCastMessage { get; set; } = new TLKStringSet();
        public SpellSchool School { get; set; } = SpellSchool.G;
        public SpellRange Range { get; set; } = SpellRange.P;
        public SpellComponent Components { get; set; } = SpellComponent.V | SpellComponent.S;
        public MetaMagicType AvailableMetaMagic { get; set; } = (MetaMagicType)0;
        public SpellTarget TargetTypes { get; set; } = SpellTarget.Self;
        public String? ImpactScript { get; set; }
        public int ConjurationTime { get; set; } = 1500;
        public SpellConjureAnimation? ConjuringAnimation { get; set; } = SpellConjureAnimation.Hand;
        public String? ConjurationHeadEffect { get; set; }
        public String? ConjurationHandEffect { get; set; }
        public String? ConjurationGroundEffect { get; set; }
        public String? ConjurationSound { get; set; }
        public String? ConjurationMaleSound { get; set; }
        public String? ConjurationFemaleSound { get; set; }
        public SpellCastAnimation? CastingAnimation { get; set; } = SpellCastAnimation.Out;
        public int CastTime { get; set; } = 1000;
        public String? CastingHeadEffect { get; set; }
        public String? CastingHandEffect { get; set; }
        public String? CastingGroundEffect { get; set; }
        public String? CastingSound { get; set; }
        public bool HasProjectile { get; set; } = false;
        public String? ProjectileModel { get; set; }
        public ProjectileType? ProjectileType { get; set; } = Types.ProjectileType.Homing;
        public ProjectileSource? ProjectileSpawnPoint { get; set; } = ProjectileSource.Hand;
        public String? ProjectileSound { get; set; }
        public ProjectileOrientation? ProjectileOrientation { get; set; } = Types.ProjectileOrientation.Path;
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
        public bool UseConcentration { get; set; } = true;
        public bool IsCastSpontaneously { get; set; } = false;
        public bool IsHostile { get; set; }
        public Spell? CounterSpell1 { get; set; }
        public Spell? CounterSpell2 { get; set; }

        public TargetShape? TargetShape { get; set; }
        public int? TargetSizeX { get; set; }
        public int? TargetSizeY { get; set; }
        public int? TargetingFlags { get; set; }
    }
}
