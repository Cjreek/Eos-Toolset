using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum BloodColor
    {
        [DisplayName("Red")]
        R,
        [DisplayName("Green")]
        G,
        [DisplayName("White")]
        W,
        [DisplayName("Yellow")]
        Y,
        [DisplayName("None")]
        N
    }
}
