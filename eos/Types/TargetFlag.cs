using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    [Flags]
    public enum TargetFlag
    {
        [DisplayName("Harms enemies")]
        HarmsEnemies = 0x01,
        [DisplayName("Harms allies")]
        HarmsAllies = 0x02,
        [DisplayName("Helps allies")]
        HelpsAllies = 0x04,
        [DisplayName("Ignores self")]
        IgnoresSelf = 0x08,
        [DisplayName("Origin on self")]
        OriginOnSelf = 0x10,
        [DisplayName("Suppress with target")]
        SuppressWithTarget = 0x20
    }
}
