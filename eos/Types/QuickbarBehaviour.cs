using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum QuickbarBehaviour
    {
        Default = 0,
        [DisplayName("Select Spell")]
        SelectSpell = 1,
        [DisplayName("Select Spell (Target: Self)")]
        SelectSpellTargetSelf = 2,
    }
}
