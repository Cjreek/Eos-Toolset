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
    internal class FamiliarViewModel : DataDetailViewModel<Familiar>
    {
        public FamiliarViewModel() : base()
        {
        }
        public FamiliarViewModel(Familiar familiar) : base(familiar)
        {
        }

        protected override ISolidColorBrush GetEntityColor()
        {
            return base.GetEntityColor();
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
