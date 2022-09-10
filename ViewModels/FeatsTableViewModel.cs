using Eos.Models.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    internal class FeatsTableViewModel : DataDetailViewModel<FeatsTable>
    {
        public FeatsTableViewModel() : base()
        {
        }

        public FeatsTableViewModel(FeatsTable featsTable) : base(featsTable)
        {
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
