using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    [Flags]
    public enum SpellTarget
    {
        Self = 0x01,
        Creature = 0x02,
        Area = 0x04,
        Item = 0x08,
        Door = 0x10,
        Placeable = 0x20,
        Trigger = 0x40
    }
}
