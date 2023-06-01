using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class BonusFeatsTable : BaseTable<BonusFeatsTableItem>
    {
        protected override string GetTypeName()
        {
            return "Bonus Feats Table";
        }

        protected override void SetDefaultValues()
        {
            Name = "CLS_BFEAT_NEW";
        }

        protected override void InitializeData()
        {
            for (int i = 0; i < GetMaximumItems(); i++)
            {
                Add(new BonusFeatsTableItem()
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
