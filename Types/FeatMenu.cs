using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum FeatMenu
    {
        [DisplayName("None")]
        NoMenuEntry = 0,
        [DisplayName("Radial Menu")]
        ClassRadialMenu = 1,
        [DisplayName("Epic Spell Menu")]
        EpicSpellMenu = 2,
    }
}
