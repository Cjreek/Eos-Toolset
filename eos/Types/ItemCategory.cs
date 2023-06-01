using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum ItemCategory
    {
        None = 0,
        Melee = 1,
        Ranged = 2,
        Shield = 3,
        Armor = 4,
        Helmet = 5,
        Ammo = 6,
        Thrown = 7,
        Staves = 8,
        Potion = 9,
        Scroll = 10,
        [DisplayName("Thieves' Tools")]
        ThievesTools = 11,
        Misc = 12,
        Wands = 13,
        Rods = 14,
        Traps = 15,
        [DisplayName("Misc (Unequipable)")]
        MiscUnequipable = 16,
        Container = 17,
        Healers = 19,
        Torch = 20,
    }
}
