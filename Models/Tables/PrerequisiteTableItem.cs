using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class PrerequisiteTableItem : TableItem
    {
        public RequirementType RequirementType { get; set; }
        public object? RequirementParam1 { get; set; }
        public object? RequirementParam2 { get; set; }
    }
}
