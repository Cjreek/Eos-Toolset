using Eos.Models;
using Eos.Nwn.Tlk;
using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels.Dialogs
{
    public class RangedDamageTypeSearchViewModel : ModelSearchViewModel<RangedDamageType>
    {
        public RangedDamageTypeSearchViewModel(VirtualModelRepository<RangedDamageType> repository) : base(repository)
        {
        }

        protected override TLKStringSet? GetModelText(RangedDamageType? model)
        {
            var tlk = new TLKStringSet();
            foreach (var lang in Enum.GetValues<TLKLanguage>())
                tlk[lang].Text = model?.Name ?? "";
            return tlk;
        }

        protected override string GetWindowTitle()
        {
            return "Search Ranged Damage Type";
        }
    }
}
