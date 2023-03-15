using Avalonia.Media;
using Eos.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels.Base
{
    public abstract class DataDetailViewModelBase : ViewModelBase
    {
        protected abstract String GetHeader();
        public abstract object GetDataObject();

        protected virtual ISolidColorBrush GetEntityColor()
        {
            return Brushes.Transparent;
        }

        public string Header { get { return GetHeader(); } }
        public ISolidColorBrush EntityColor { get { return GetEntityColor(); } }
        public int SelectedTabIndex { get; set; }
    }
}
