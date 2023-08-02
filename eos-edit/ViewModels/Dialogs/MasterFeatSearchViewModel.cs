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
    public class MasterFeatSearchViewModel : ModelSearchViewModel<MasterFeat>
    {
        public MasterFeatSearchViewModel(VirtualModelRepository<MasterFeat> repository) : base(repository)
        {
        }

        protected override TLKStringSet? GetModelText(MasterFeat? model)
        {
            return model?.Name;
        }

        protected override string GetWindowTitle()
        {
            return "Search Master Feat";
        }
    }
}
