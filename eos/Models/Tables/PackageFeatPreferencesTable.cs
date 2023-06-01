using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class PackageFeatPreferencesTable : BaseTable<PackageFeatPreferencesTableItem>
    {
        protected override string GetTypeName()
        {
            return "Feat Preference Table";
        }

        protected override void SetDefaultValues()
        {
            Name = "PACKFT";
        }

        protected override int GetMaximumItems()
        {
            return int.MaxValue;
        }
    }
}
