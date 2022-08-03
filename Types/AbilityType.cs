using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    internal enum AbilityType
    {
        [DisplayName("Strength")]
        STR,
        [DisplayName("Dexterity")]
        DEX,
        [DisplayName("Intelligence")]
        INT,
        [DisplayName("Charisma")]
        CHA,
        [DisplayName("Wisdom")]
        WIS,
        [DisplayName("Constitution")]
        CON
    }
}
