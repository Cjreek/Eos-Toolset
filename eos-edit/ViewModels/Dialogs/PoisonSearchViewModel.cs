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
    internal class PoisonSearchViewModel : ModelSearchViewModel<Poison>
    {
        public PoisonSearchViewModel(VirtualModelRepository<Poison> repository) : base(repository)
        {
        }

        protected override TLKStringSet? GetModelText(Poison? model)
        {
            return model?.Name;
        }

        protected override string GetWindowTitle()
        {
            return "Search poison";
        }
    }
}
