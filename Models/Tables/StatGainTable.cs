using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class StatGainTable : BaseTable<StatGainTableItem>
    {
        protected override void SetDefaultValues()
        {
            Name = "NEW_STATGN_TBL";
        }

        protected override void InitializeData()
        {
            for (int i = 0; i < GetMaximumItems(); i++)
            {
                Add(new StatGainTableItem()
                {
                    Level = i + 1,
                    Strength = 0,
                    Dexterity = 0,
                    Constitution = 0,
                    Wisdom = 0,
                    Intelligence = 0,
                    Charisma = 0,
                    NaturalAC = 0,
                });
            }
        }

        protected override int GetMaximumItems()
        {
            return 60;
        }
    }
}
