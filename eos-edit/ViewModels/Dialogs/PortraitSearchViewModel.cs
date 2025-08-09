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
    public class PortraitSearchViewModel : ModelSearchViewModel<Portrait>
    {
        public PortraitSearchViewModel(VirtualModelRepository<Portrait> repository) : base(repository)
        {
            IconHeight = 64;
            IconWidth = 32;
        }

        protected override TLKStringSet? GetModelText(Portrait? model)
        {
            var tlk = new TLKStringSet();
            foreach (var lang in Enum.GetValues<TLKLanguage>())
                tlk[lang].Text = model?.Name ?? "";
            return tlk;
        }

        protected override string GetWindowTitle()
        {
            return "Search Portrait";
        }
    }
}
