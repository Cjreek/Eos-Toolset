using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class PackageSpellPreferencesTable : BaseTable<PackageSpellPreferencesTableItem>
    {
        protected override string GetTypeName()
        {
            return "Spell Preference Table";
        }

        protected override void SetDefaultValues()
        {
            Name = "PACKSP";
        }

        protected override int GetMaximumItems()
        {
            return int.MaxValue;
        }
    }
}
