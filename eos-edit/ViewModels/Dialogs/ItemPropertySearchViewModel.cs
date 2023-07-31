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
    public class ItemPropertySearchViewModel : ModelSearchViewModel<ItemProperty>
    {
        public ItemPropertySearchViewModel(VirtualModelRepository<ItemProperty> repository) : base(repository)
        {
        }

        protected override TLKStringSet? GetModelText(ItemProperty? model)
        {
            return model?.Name;
        }

        protected override string GetWindowTitle()
        {
            return "Search Item Property";
        }
    }
}
