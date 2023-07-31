using Eos.Models.Tables;
using Eos.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    public class SavingThrowTableViewModel : DataDetailViewModel<SavingThrowTable>
    {
        public SavingThrowTableViewModel() : base()
        {
        }

        public SavingThrowTableViewModel(SavingThrowTable savesTable) : base(savesTable)
        {
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
