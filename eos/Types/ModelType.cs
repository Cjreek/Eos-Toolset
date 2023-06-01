using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum ModelType
    {
        [DisplayName("Parts")]
        P,
        [DisplayName("Simple")]
        S,
        [DisplayName("Full")]
        F,
        [DisplayName("Limited/Large")]
        L
    }
}
