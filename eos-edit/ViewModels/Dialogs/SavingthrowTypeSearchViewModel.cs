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
    public class SavingthrowTypeSearchViewModel : ModelSearchViewModel<SavingthrowType>
    {
        public SavingthrowTypeSearchViewModel(VirtualModelRepository<SavingthrowType> repository) : base(repository)
        {
        }

        protected override TLKStringSet? GetModelText(SavingthrowType? model)
        {
            return model?.Name;
        }

        protected override string GetWindowTitle()
        {
            return "Search Savingthrow Type";
        }
    }
}
