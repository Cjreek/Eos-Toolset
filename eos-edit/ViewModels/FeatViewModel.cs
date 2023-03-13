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
    internal class FeatViewModel : DataDetailViewModel<Feat>
    {
        public FeatViewModel() : base()
        {
        }

        public FeatViewModel(Feat feat) : base(feat)
        {
        }

        protected override ISolidColorBrush GetEntityColor()
        {
            return new SolidColorBrush(Color.FromArgb(100, 91, 114, 147));
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
