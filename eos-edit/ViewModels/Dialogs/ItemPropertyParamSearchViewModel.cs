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
    class ItemPropertyParamSearchViewModel : ModelSearchViewModel<ItemPropertyParam>
    {
        public ItemPropertyParamSearchViewModel(VirtualModelRepository<ItemPropertyParam> repository) : base(repository)
        {
        }

        protected override TLKStringSet? GetModelText(ItemPropertyParam? model)
        {
            return model?.Name;
        }

        protected override string GetWindowTitle()
        {
            return "Search Item Property Param";
        }
    }
}
