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
    internal class MasterFeatViewModel : DataDetailViewModel<MasterFeat>
    {
        public MasterFeatViewModel() : base()
        {
        }

        public MasterFeatViewModel(MasterFeat masterFeat) : base(masterFeat)
        {
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }

        protected override ISolidColorBrush GetEntityColor()
        {
            return new SolidColorBrush(Color.FromArgb(150, 91, 114, 147));
        }
    }
}
