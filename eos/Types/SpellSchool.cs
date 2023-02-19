using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum SpellSchool
    {
        [DisplayName("General")]
        G,
        [DisplayName("Abjuration")]
        A,
        [DisplayName("Conjuration")]
        C,
        [DisplayName("Divination")]
        D,
        [DisplayName("Enchantment")]
        E,
        [DisplayName("Evocation")]
        V,
        [DisplayName("Illusion")]
        I,
        [DisplayName("Necromancy")]
        N,
        [DisplayName("Transmutation")]
        T
    }
}
