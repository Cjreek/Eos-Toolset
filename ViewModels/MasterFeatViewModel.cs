using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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

        protected override Brush GetEntityColor()
        {
            return new SolidColorBrush(Color.FromArgb(100, 255, 167, 255));
        }
    }
}
