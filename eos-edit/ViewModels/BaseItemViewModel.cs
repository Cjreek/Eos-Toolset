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
    public class BaseItemViewModel : DataDetailViewModel<BaseItem>
    {
        public BaseItemViewModel() : base()
        {
        }

        public BaseItemViewModel(BaseItem baseItem) : base(baseItem)
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
