using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models
{
    internal class Domain : BaseModel
    {
        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public TLKStringSet Description { get; set; } = new TLKStringSet();
        public IntPtr Icon { get; set; }
        public Spell? Level0Spell { get; set; }
        public Spell? Level1Spell { get; set; }
        public Spell? Level2Spell { get; set; }
        public Spell? Level3Spell { get; set; }
        public Spell? Level4Spell { get; set; }
        public Spell? Level5Spell { get; set; }
        public Spell? Level6Spell { get; set; }
        public Spell? Level7Spell { get; set; }
        public Spell? Level8Spell { get; set; }
        public Spell? Level9Spell { get; set; }
        public Feat? GrantedFeat { get; set; }
        public bool FeatIsActive { get; set; } = false;
    }
}
