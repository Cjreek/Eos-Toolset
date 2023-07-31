using Eos.Models;
using Eos.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    public class InventorySoundViewModel : DataDetailViewModel<InventorySound>
    {
        public InventorySoundViewModel() : base()
        {
        }

        public InventorySoundViewModel(InventorySound inventorySound) : base(inventorySound)
        {
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
