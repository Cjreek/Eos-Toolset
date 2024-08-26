using Eos.Nwn.Tlk;
using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Updates
{
    internal class Update0760 : Update
    {
        public override int Version => 760;
        public override DateTime GameDataMinimumBuildDate => new DateTime(2024, 08, 05); // 6273521c
        public override bool ForceGameDataUpdate => true; // Import appearances and base items

        public override void Apply(EosProject project)
        {
            // add standard column data to overriden project data that's missing new columns
            foreach (var dmg in project.DamageTypes)
            {
                if (dmg == null) continue;

                if (dmg.IsOverride)
                {
                    var standardDmg = MasterRepository.Standard.DamageTypes.GetByID(dmg.Overrides ?? Guid.Empty);
                    if (standardDmg != null)
                    {
                        dmg.RangedDamageType = standardDmg.RangedDamageType; 
                        dmg.MeleeImpactVFX = standardDmg.MeleeImpactVFX;
                        dmg.RangedImpactVFX = standardDmg.RangedImpactVFX;
                    }
                }
            }

            foreach (var iptable in project.ItemPropertyTables)
            {
                if (iptable == null) continue;

                if ((iptable.IsOverride) && (iptable.Overrides == Guid.Parse("e40120bb-55e9-484b-977a-c74008067c41"))) // IPRP_SAVEELEMENT
                {
                    var standardIpTable = MasterRepository.Standard.ItemPropertyTables.GetByID(iptable.Overrides ?? Guid.Empty);
                    if ((standardIpTable != null) && (iptable.CustomColumn02.Column == ""))
                    {
                        iptable.CustomColumn02.Label = standardIpTable.CustomColumn02.Label;
                        iptable.CustomColumn02.Column = standardIpTable.CustomColumn02.Column;
                        iptable.CustomColumn02.DataType = standardIpTable.CustomColumn02.DataType;
                    }
                }
            }
        }
    }
}
