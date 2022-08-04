using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    [Flags]
    internal enum SpellComponent
    {
        [DisplayName("Verbal")]
        V,
        [DisplayName("Somatic")]
        S
    }
}
