using Eos.Models;
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
    public class CustomObjectInstanceViewModel : DataDetailViewModel<CustomObjectInstance>
    {
        public CustomObjectInstanceViewModel() : base()
        {
        }

        public CustomObjectInstanceViewModel(CustomObjectInstance instance) : base(instance)
        {
        }

        protected override HashSet<String> GetHeaderSourceFields()
        {
            return new HashSet<String>()
            {
                "Name"
            };
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
