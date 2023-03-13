using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels.Base
{
    internal abstract class DataDetailViewModel<T> : DataDetailViewModelBase where T : BaseModel, new()
    {
        private T data;
        private HashSet<String> headerSourceFields;

        public T Data { get { return data; } }

        public override object GetDataObject()
        {
            return data;
        }

        private void Data_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (headerSourceFields.Contains(e.PropertyName ?? ""))
                NotifyPropertyChanged(nameof(Header));
        }

        protected virtual HashSet<String> GetHeaderSourceFields()
        {
            return new HashSet<String>()
            {
                "Name"
            };
        }

        public DataDetailViewModel()
        {
            headerSourceFields = GetHeaderSourceFields();

            this.data = new T();
            data.PropertyChanged += Data_PropertyChanged;
        }

        public DataDetailViewModel(T data)
        {
            headerSourceFields = GetHeaderSourceFields();

            this.data = data;
            data.PropertyChanged += Data_PropertyChanged;
        }

        ~DataDetailViewModel()
        {
            data.PropertyChanged -= Data_PropertyChanged;
        }
    }
}
