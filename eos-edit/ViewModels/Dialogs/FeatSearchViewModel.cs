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
    public class FeatSearchViewModel : ModelSearchViewModel<Feat>
    {
        public FeatSearchViewModel(VirtualModelRepository<Feat> repository) : base(repository)
        {
        }

        protected override TLKStringSet? GetModelText(Feat? model)
        {
            return model?.Name;
        }

        protected override string GetWindowTitle()
        {
            return "Search Feat";
        }
    }
}
