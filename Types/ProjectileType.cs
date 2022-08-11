using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum ProjectileType
    {
        [DisplayName("Homing")]
        Homing,
        [DisplayName("Ballistic")]
        Ballistic,
        [DisplayName("High Ballistic")]
        HighBallistic,
        [DisplayName("Burst")]
        Burst,
        [DisplayName("Accelerating")]
        Accelerating,
        [DisplayName("Spiral")]
        Spiral,
        [DisplayName("Linked")]
        Linked,
        [DisplayName("Bounce")]
        Bounce
    }
}
