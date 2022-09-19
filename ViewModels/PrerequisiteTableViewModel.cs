using Eos.Models.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    internal class PrerequisiteTableViewModel : DataDetailViewModel<PrerequisiteTable>
    {
        public PrerequisiteTableViewModel() : base()
        {
        }

        public PrerequisiteTableViewModel(PrerequisiteTable requTable) : base(requTable)
        {
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
