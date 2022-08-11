using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    [Flags]
    public enum SpellComponent
    {
        [DisplayName("Verbal")]
        V = 1,
        [DisplayName("Somatic")]
        S = 2
    }
}
