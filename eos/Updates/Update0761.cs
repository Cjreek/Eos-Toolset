using Eos.Nwn.Tlk;
using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Updates
{
    internal class Update0761 : Update
    {
        public override int Version => 761;
        public override DateTime GameDataMinimumBuildDate => new DateTime(2024, 08, 05); // 6273521c
        public override bool ForceGameDataUpdate => true; // Reimport base items to import creature armor correctly

        public override void Apply(EosProject project)
        {
            // Nothing, just needs to update game data
        }
    }
}
