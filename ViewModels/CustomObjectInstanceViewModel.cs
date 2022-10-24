using Eos.Models;
using Eos.Models.Tables;
using Eos.Types;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    internal class CustomObjectInstanceViewModel : DataDetailViewModel<CustomObjectInstance>
    {
        public CustomObjectInstanceViewModel() : base()
        {
        }

        public CustomObjectInstanceViewModel(CustomObjectInstance instance) : base(instance)
        {
        }

        protected override string GetHeader()
        {
            return Data.Label;
        }
    }
}
