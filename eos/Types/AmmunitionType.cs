using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum AmmunitionType
    {
        Arrow = 1, 
        Bolt = 2,
        Bullet = 3,
        Dart = 4,
        Shuriken = 5,
        [DisplayName("Throwing Axe")]
        ThrowingAxe = 6,
    }
}
