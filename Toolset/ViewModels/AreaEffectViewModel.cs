using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Eos.ViewModels
{
    internal class AreaEffectViewModel : DataDetailViewModel<AreaEffect>
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
