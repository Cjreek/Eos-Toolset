using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    internal enum FeatCategory
    {
        [DisplayName("Combat")]
        Combat = 1,
        [DisplayName("Active Combat")]
        CombatActive = 2,
        [DisplayName("Defensive")]
        Defensive = 3,
        [DisplayName("Magical")]
        Magical = 4,
        [DisplayName("Class or Racial")]
        ClassOrRacial = 5,
        [DisplayName("Other")]
        Other = 6
    }
}
