using Eos.Models.Tables;
using Eos.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    public class BonusFeatTableViewModel : DataDetailViewModel<BonusFeatsTable>
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
