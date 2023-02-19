using Eos.Models.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    internal class BonusFeatTableViewModel : DataDetailViewModel<BonusFeatsTable>
    {
        public BonusFeatTableViewModel() : base()
        {
        }

        public BonusFeatTableViewModel(BonusFeatsTable bonusFeatTable) : base(bonusFeatTable)
        {
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
