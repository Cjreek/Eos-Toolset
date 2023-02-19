using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    [Flags]
    public enum MetaMagicType
    {
        Empower = 0x01,
        Extend = 0x02,
        Maximize = 0x04,
        Quicken = 0x08,
        Silent = 0x10,
        Still = 0x20
    }
}
