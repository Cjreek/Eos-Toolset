using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models
{
    public class Skill : BaseModel
    {
        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public TLKStringSet Description { get; set; } = new TLKStringSet();
        public IntPtr Icon { get; set; }
        public bool CanUseUntrained { get; set; }
        public AbilityType KeyAbility { get; set; }
        public bool UseArmorPenalty { get; set; }
        public bool AllClassesCanUse { get; set; }
        public AICategory AIBehaviour { get; set; }
        public bool IsHostile { get; set; }
    }
}
