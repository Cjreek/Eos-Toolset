using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class FeatsTableModel
    {
        public Feat? Feat { get; set; }
        public FeatListType FeatList { get; set; } = FeatListType.GeneralFeat;
        public int GrantedOnLevel { get; set; } = -1;
        public FeatMenu Menu { get; set; } = FeatMenu.NoMenuEntry;
    }
}
