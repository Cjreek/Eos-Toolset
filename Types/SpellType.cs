using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum SpellType
    {
        [DisplayName("Spell")]
        Spell = 1,
        [DisplayName("Creature Power")]
        CreaturePower = 2,
        [DisplayName("Feat")]
        Feat = 3,
        [DisplayName("Other")]
        Other = 4
    }
}
