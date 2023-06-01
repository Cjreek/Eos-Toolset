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
    internal class AppearanceSoundsetSearchViewModel : ModelSearchViewModel<AppearanceSoundset>
    {
        public AppearanceSoundsetSearchViewModel(VirtualModelRepository<AppearanceSoundset> repository) : base(repository)
        {
        }

        protected override TLKStringSet? GetModelText(AppearanceSoundset? model)
        {
            var tlk = new TLKStringSet();
            foreach (var lang in Enum.GetValues<TLKLanguage>())
                tlk[lang].Text = model?.Name ?? "";
            return tlk;
        }

        protected override string GetWindowTitle()
        {
            return "Search Appearance Soundset";
        }
    }
}
