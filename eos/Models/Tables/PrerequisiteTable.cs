using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class PrerequisiteTable : BaseTable<PrerequisiteTableItem>
    {
        protected override string GetTypeName()
        {
            return "Prerequisites Table";
        }

        protected override void SetDefaultValues()
        {
            Name = "CLS_PRES_NEW";
        }
    }
}
