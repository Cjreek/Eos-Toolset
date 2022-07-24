using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    internal abstract class DataDetailViewModel<T> : DataDetailViewModelBase where T : class
    {
        public string Header { get { return GetHeader(); } }
        public T Data { get { return GetData(); } }
        protected abstract T GetData();

        public override object GetDataObject()
        {
            return GetData();
        }
    }
}
