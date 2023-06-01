using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum VisualEffectType
    {
        [DisplayName("Fire and Forget / Impact")]
        F = 1,
        [DisplayName("Duration")]
        D = 2,
        [DisplayName("Projectile")]
        P = 3,
        [DisplayName("Beam")]
        B = 4
    }
}
