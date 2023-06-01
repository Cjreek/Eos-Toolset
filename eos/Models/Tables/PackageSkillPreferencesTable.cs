using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class PackageSkillPreferencesTable : BaseTable<PackageSkillPreferencesTableItem>
    {
        protected override string GetTypeName()
        {
            return "Skill Preference Table";
        }

        protected override void SetDefaultValues()
        {
            Name = "PACKSK";
        }

        protected override int GetMaximumItems()
        {
            return int.MaxValue;
        }
    }
}
