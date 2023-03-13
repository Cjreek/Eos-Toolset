using Eos.Models.Tables;
using Eos.Types;
using Eos.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    internal class AttackBonusTableViewModel : DataDetailViewModel<AttackBonusTable>
    {
        public AttackBonusTableViewModel() : base()
        {
        }

        public AttackBonusTableViewModel(AttackBonusTable abTable) : base(abTable)
        {
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
