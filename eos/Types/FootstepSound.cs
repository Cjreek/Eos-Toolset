using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum FootstepSound
    {
        None = -1,
        Normal = 0,
        Large = 1,
        Dragon = 2,
        Soft = 3,
        Hoof = 4,
        [DisplayName("Hoof (Large)")]
        HoofLarge = 5,
        Beetle = 6,
        Spider = 7,
        Skeleton = 8,
        [DisplayName("Winged (Leather)")]
        WingedLeather = 9,
        [DisplayName("Winged (Feather)")]
        WingedFeather = 10,
        Lizard = 11,
        Seagull = 13,
        Shark = 14,
        [DisplayName("Water (Normal)")]
        WaterNormal = 15,
        [DisplayName("Water (Large)")]
        WaterLarge = 16,
        Horse = 17,
    }
}
