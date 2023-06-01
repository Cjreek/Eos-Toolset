using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum MovementRate
    {
        [DisplayName("Immobile")]
        NOMOVE,
        [DisplayName("Very Slow")]
        VSLOW,
        [DisplayName("Slow")]
        SLOW,
        [DisplayName("Normal")]
        NORM,
        [DisplayName("Fast")]
        FAST,
        [DisplayName("Very Fast")]
        VFAST,
        [DisplayName("DM Fast")]
        DFAST
    }
}
