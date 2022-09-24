using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum SoundsetType
    {
        [DisplayName("Player")]
        Player,
        [DisplayName("Henchman")]
        Henchman,
        [DisplayName("NPC (full)")]
        NPCFull,
        [DisplayName("NPC (partial)")]
        NPCPart,
        [DisplayName("Monster")]
        Monster,
    }
}
