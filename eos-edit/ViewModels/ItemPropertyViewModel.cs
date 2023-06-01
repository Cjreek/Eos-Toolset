using Avalonia.Media;
using Eos.Models;
using Eos.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    internal class ItemPropertyViewModel : DataDetailViewModel<ItemProperty>
    {
        public ItemPropertyViewModel() : base()
        {
        }

        public ItemPropertyViewModel(ItemProperty itemProperty) : base(itemProperty)
        {
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }

        protected override ISolidColorBrush GetEntityColor()
        {
            return base.GetEntityColor();
        }
    }
}
