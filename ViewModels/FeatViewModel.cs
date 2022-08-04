using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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

        protected override Brush GetColor()
        {
            return Brushes.LightSteelBlue;
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
