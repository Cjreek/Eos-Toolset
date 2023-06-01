using Avalonia.Media;
using Eos.Models;
using Eos.Models.Tables;
using Eos.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    internal class ItemPropertyParamViewModel : DataDetailViewModel<ItemPropertyParam>
    {
        public ItemPropertyParamViewModel() : base()
        {
        }

        public ItemPropertyParamViewModel(ItemPropertyParam itemPropertyParam) : base(itemPropertyParam)
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
