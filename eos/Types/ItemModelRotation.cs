using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum ItemModelRotation
    {
        None = 0,
        [DisplayName("Y-Axis (90°)")]
        AxisY = 1,
        [DisplayName("X-Axis (90°)")]
        AxisX = 2,
    }
}
