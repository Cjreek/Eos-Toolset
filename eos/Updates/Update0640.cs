using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Updates
{
    internal class Update0640 : Update
    {
        public override int Version => 640;
        public override bool ForceGameDataUpdate => true; // Import appearances and base items

        public override void Apply(EosProject project)
        {
            // Fix Appearance project Settings
            if (project.Settings.Appearances.ExportOffset == 47)
                project.Settings.Appearances.ExportOffset = 1000;
        }
    }
}
