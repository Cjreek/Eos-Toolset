using Eos.Models;
using Eos.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    public class AreaEffectViewModel : DataDetailViewModel<AreaEffect>
    {
        public AreaEffectViewModel() : base()
        {
        }

        public AreaEffectViewModel(AreaEffect aoe) : base(aoe)
        {
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
