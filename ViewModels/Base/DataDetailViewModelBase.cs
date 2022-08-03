using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    internal abstract class DataDetailViewModelBase : ViewModelBase
    {
        protected abstract String GetHeader();
        public abstract object GetDataObject();
    }
}
