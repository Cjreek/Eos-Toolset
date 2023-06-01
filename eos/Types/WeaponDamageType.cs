using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum WeaponDamageType
    {
        Piercing = 1,
        Bludgeoning = 2,
        Slashing = 3,
        [DisplayName("Slashing + Piercing")]
        SlashingPiercing = 4,
        [DisplayName("Piercing + Bludgeoning")]
        PiercingBludgeoning = 5,
    }
}
