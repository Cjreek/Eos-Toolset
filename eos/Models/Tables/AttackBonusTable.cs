using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class AttackBonusTable : BaseTable<AttackBonusTableItem>
    {
        protected override string GetTypeName()
        {
            return "Attack Bonus Table";
        }

        protected override void SetDefaultValues()
        {
            Name = "CLS_ATK_NEW";
        }

        protected override void InitializeData()
        {
            for (int i = 0; i < GetMaximumItems(); i++)
            {
                Add(new AttackBonusTableItem()
                {
                    Level = i + 1,
                    AttackBonus = 0,
                });
            }
        }

        protected override int GetMaximumItems()
        {
            return 60;
        }
    }
}
