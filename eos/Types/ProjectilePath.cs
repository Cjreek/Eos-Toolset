using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum ProjectilePath
    {
        [DisplayName("Default")]
        Default = 0,
        [DisplayName("Homing")]
        Homing = 1,
        [DisplayName("Ballistic")]
        Ballistic = 2,
        [DisplayName("High Ballistic")]
        HighBallistic = 3,
        [DisplayName("Burst Up")]
        BurstUp = 4,
        [DisplayName("Accelerating")]
        Accelerating = 5,
        [DisplayName("Spiral")]
        Spiral = 6,
        [DisplayName("Linked")]
        Linked = 7,
        [DisplayName("Bounce")]
        Bounce = 8,
        [DisplayName("Burst")]
        Burst = 9,
        [DisplayName("Linked Burst Up")]
        LinkedBurstUp = 10,
        [DisplayName("Triple Ballistic Hit")]
        TripleBallisticHit = 11,
        [DisplayName("Triple Ballistic Miss")]
        TripleBallisticMiss = 12,
        [DisplayName("Double Ballistic")]
        DoubleBallistic = 13,
    }
}
