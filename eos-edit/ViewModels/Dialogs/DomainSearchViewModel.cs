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
    internal class DomainSearchViewModel : ModelSearchViewModel<Domain>
    {
        public DomainSearchViewModel(VirtualModelRepository<Domain> repository) : base(repository)
        {
        }

        protected override TLKStringSet? GetModelText(Domain? model)
        {
            return model?.Name;
        }

        protected override string GetWindowTitle()
        {
            return "Search Domain";
        }
    }
}
