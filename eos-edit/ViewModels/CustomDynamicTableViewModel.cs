using Eos.Models;
using Eos.Models.Tables;
using Eos.Types;
using Eos.ViewModels.Base;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    public class CustomDynamicTableViewModel : DataDetailViewModel<CustomDynamicTable>
    {
        public CustomDynamicTableViewModel() : base()
        {
        }

        public CustomDynamicTableViewModel(CustomDynamicTable customDynamicTable) : base(customDynamicTable)
        {
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
