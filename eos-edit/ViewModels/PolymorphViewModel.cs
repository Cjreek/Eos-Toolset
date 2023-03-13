using Eos.Models;
using Eos.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    internal class PolymorphViewModel : DataDetailViewModel<Polymorph>
    {
        public PolymorphViewModel() : base()
        {
        }

        public PolymorphViewModel(Polymorph polymorph) : base(polymorph)
        {

        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
