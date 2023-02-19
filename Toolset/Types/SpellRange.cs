using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum SpellRange
    {
        [DisplayName("Personal (0m)")]
        P,
        [DisplayName("Touch (2.25m)")]
        T,
        [DisplayName("Short (8m)")]
        S,
        [DisplayName("Medium (20m)")]
        M,
        [DisplayName("Long (40m)")]
        L
    }
}
