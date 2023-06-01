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
    internal class PortraitViewModel : DataDetailViewModel<Portrait>
    {
        public PortraitViewModel() : base()
        {
        }

        public PortraitViewModel(Portrait portrait) : base(portrait)
        {
        }

        protected override ISolidColorBrush GetEntityColor()
        {
            return base.GetEntityColor();
        }

        protected override string GetHeader()
        {
            return Data.ResRef;
        }
    }
}
