using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models
{
    public class Poison : BaseModel
    {
        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public int SaveDC { get; set; } = 10;
        public int HandleDC { get; set; } = 0; // Unused?
        public int InitialAbilityDamageDiceCount { get; set; } = 1;
        public int InitialAbilityDamageDice { get; set; } = 4;
        public AbilityType? InitialAbilityDamageType { get; set; } = AbilityType.CON;
        public IntPtr InitialEffectScript { get; set; }
        public int SecondaryAbilityDamageDiceCount { get; set; } = 1;
        public int SecondaryAbilityDamageDice { get; set; } = 4;
        public AbilityType? SecondaryAbilityDamageType { get; set; } = AbilityType.CON;
        public IntPtr SecondaryEffectScript { get; set; }
        public double Cost { get; set; } = 1.0; // Unused?
        public bool OnHitApplied { get; set; } = false; // Unused?
        public IntPtr ImpactVFX { get; set; }
    }
}
