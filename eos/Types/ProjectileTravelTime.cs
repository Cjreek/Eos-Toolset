using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum ProjectileTravelTime
    {
        [DisplayName("Logarithmic")]
        Log,
        [DisplayName("Linear")]
        Linear,
        [DisplayName("Linear (½ Speed)")]
        Linear2
    }
}
