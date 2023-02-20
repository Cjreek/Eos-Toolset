using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class SkillsTable : BaseTable<SkillsTableItem>
    {
        protected override void SetDefaultValues()
        {
            Name = "CLS_SKILL_NEW";
        }
    }
}
