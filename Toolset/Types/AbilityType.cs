using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum AbilityType
    {
        [DisplayName("Strength")]
        STR = 0,
        [DisplayName("Dexterity")]
        DEX = 1,
        [DisplayName("Intelligence")]
        INT = 3,
        [DisplayName("Charisma")]
        CHA = 5,
        [DisplayName("Wisdom")]
        WIS = 4,
        [DisplayName("Constitution")]
        CON = 2,
    }
}
