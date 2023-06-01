using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum WeaponWieldType
    {
        Standard = 0,
        [DisplayName("Not Wieldable")]
        NotWieldable = 1,
        [DisplayName("Two-Handed Weapon")]
        TwoHanded = 4,
        Bow = 5,
        Crossbow = 6,
        Shield = 7,
        [DisplayName("Double-Sided Weapon")]
        DoubleSided = 8,
        [DisplayName("Creature Weapon")]
        CreatureWeapon = 9,
        Sling = 10,
        [DisplayName("Throwing Weapon")]
        ThrowingWeapon = 11,
    }
}
