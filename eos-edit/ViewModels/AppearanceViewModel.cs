using Eos.Models;
using Eos.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    public class AppearanceViewModel : DataDetailViewModel<Appearance>
    {
        public AppearanceViewModel() : base()
        {
        }

        public AppearanceViewModel(Appearance appearance) : base(appearance)
        {
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
