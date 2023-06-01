using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum AlphaChannelUsageType
    {
        [DisplayName("Alpha Transparency")]
        Alpha = 0,
        [DisplayName("Environment Mapping")]
        Environment = 1,
    }
}
