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
    internal class WeaponSoundSearchViewModel : ModelSearchViewModel<WeaponSound>
    {
        public WeaponSoundSearchViewModel(VirtualModelRepository<WeaponSound> repository) : base(repository)
        {
        }

        protected override TLKStringSet? GetModelText(WeaponSound? model)
        {
            var tlk = new TLKStringSet();
            foreach (var lang in Enum.GetValues<TLKLanguage>())
                tlk[lang].Text = model?.Name ?? "";
            return tlk;
        }

        protected override string GetWindowTitle()
        {
            return "Search Weapon Sound";
        }
    }
}
