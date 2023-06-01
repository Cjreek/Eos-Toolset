using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum VFXShakeType
    {
        [DisplayName("None")]
        None = 0,
        [DisplayName("Single Bump")]
        SingleBump = 1,
        [DisplayName("Multiple Bumps")]
        MultipleBumps = 2,
    }
}
