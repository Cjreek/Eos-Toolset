using Eos.Models;
using Eos.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    public class AppearanceSoundsetViewModel : DataDetailViewModel<AppearanceSoundset>
    {
        public AppearanceSoundsetViewModel() : base()
        {
        }

        public AppearanceSoundsetViewModel(AppearanceSoundset appearanceSoundset) : base(appearanceSoundset)
        {
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
