using Eos.Models.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    internal class StatGainTableViewModel : DataDetailViewModel<StatGainTable>
    {
        public StatGainTableViewModel() : base()
        {
        }

        public StatGainTableViewModel(StatGainTable statGainTable) : base(statGainTable)
        {
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
