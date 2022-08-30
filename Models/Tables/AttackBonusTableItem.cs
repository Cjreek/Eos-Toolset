using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class AttackBonusTableItem : TableItem
    {
        public int Level { get; set; } = 1;
        public int AttackBonus { get; set; } = 0;
    }
}
