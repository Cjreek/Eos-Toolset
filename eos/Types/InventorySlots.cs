using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    [Flags]
    public enum InventorySlots
    {
        Helmet    = 0x000001,
        Armor     = 0x000002,
        Boots     = 0x000004,
        Gloves    = 0x000008,
        MainHand  = 0x000010,
        OffHand   = 0x000020,
        Cloak     = 0x000040,
        [DisplayName("Left Ring")]
        LeftRing  = 0x000080,
        [DisplayName("Right Ring")]
        RightRing = 0x000100,
        Amulet    = 0x000200,
        Belt      = 0x000400,
        Arrow     = 0x000800,
        Bullet    = 0x001000,
        Bolt      = 0x002000,
        [DisplayName("Left Claw")]
        LeftClaw  = 0x004000,
        [DisplayName("Right Claw")]
        RightClaw = 0x008000,
        Bite      = 0x010000,
        [DisplayName("Creature Armor")]
        CArmor    = 0x020000,
    }
}
