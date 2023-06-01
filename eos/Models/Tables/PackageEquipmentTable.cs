using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class PackageEquipmentTable : BaseTable<PackageEquipmentTableItem>
    {
        protected override string GetTypeName()
        {
            return "Equipment Table";
        }

        protected override void SetDefaultValues()
        {
            Name = "PACKEQ";
        }

        protected override int GetMaximumItems()
        {
            return int.MaxValue;
        }
    }
}
