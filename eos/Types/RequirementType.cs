using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum RequirementType
    {
        [IgnoreEnumValue]
        UNDEFINED,
        ARCSPELL,
        BAB,
        CLASSOR,
        CLASSNOT,
        FEAT,
        FEATOR,
        RACE,
        SAVE,
        SKILL,
        SPELL,
        VAR
    }
}
