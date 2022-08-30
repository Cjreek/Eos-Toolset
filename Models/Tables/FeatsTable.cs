using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class FeatsTable : BaseTable<FeatsTableItem>
    {
        protected override int GetMaximumItems()
        {
            return Int16.MaxValue;
        }
    }
}
