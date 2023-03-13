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
    internal class DiseaseSearchViewModel : ModelSearchViewModel<Disease>
    {
        public DiseaseSearchViewModel(VirtualModelRepository<Disease> repository) : base(repository)
        {
        }

        protected override TLKStringSet? GetModelText(Disease? model)
        {
            return model?.Name;
        }

        protected override string GetWindowTitle()
        {
            return "Search disease";
        }
    }
}
