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
    public class DamageTypeSearchViewModel : ModelSearchViewModel<DamageType>
    {
        public DamageTypeSearchViewModel(VirtualModelRepository<DamageType> repository) : base(repository)
        {
        }

        protected override TLKStringSet? GetModelText(DamageType? model)
        {
            return model?.Name;
        }

        protected override string GetWindowTitle()
        {
            return "Search Damage Type";
        }
    }
}
