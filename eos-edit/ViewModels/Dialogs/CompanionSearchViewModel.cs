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
    internal class CompanionSearchViewModel : ModelSearchViewModel<Companion>
    {
        public CompanionSearchViewModel(VirtualModelRepository<Companion> repository) : base(repository)
        {
        }

        protected override TLKStringSet? GetModelText(Companion? model)
        {
            return model?.Name;
        }

        protected override string GetWindowTitle()
        {
            return "Search Companion";
        }
    }
}
