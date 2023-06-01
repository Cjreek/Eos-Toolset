using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum StorePanelType
    {
        Armor = 0,
        Weapons = 1,
        [DisplayName("Potions & Scrolls")]
        PotionsScrolls = 2,
        [DisplayName("Wands & Magic Items")]
        WandsMagicItems = 3,
        [DisplayName("Miscellaneous")]
        Misc = 4,
    }
}
