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
    public class SpellSearchViewModel : ModelSearchViewModel<Spell>
    {
        public SpellSearchViewModel(VirtualModelRepository<Spell> repository) : base(repository)
        {
        }

        protected override TLKStringSet? GetModelText(Spell? model)
        {
            return model?.Name;
        }

        protected override string GetWindowTitle()
        {
            return "Search Spell";
        }
    }
}
