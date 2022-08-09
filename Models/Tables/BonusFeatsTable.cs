using Eos.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class BonusFeatsTable : BaseTable<BonusFeatsTableModel>
    {
        protected override void InitializeData()
        {
            for (int i = 0; i < GetMaximumItems(); i++)
            {
                Add(new BonusFeatsTableModel()
                {
                    Level = i + 1,
                    BonusFeatCount = 0,
                });
            }
        }

        protected override int GetMaximumItems()
        {
            return 60;
        }
    }
}
