﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Eos.ViewModels
{
    internal abstract class DataDetailViewModel<T> : DataDetailViewModelBase where T : class, new()
    {
        private T data;

        public string Header { get { return GetHeader(); } }
        public Brush Color { get { return GetColor(); } }

        public T Data { get { return data; } }

        public override object GetDataObject()
        {
            return data;
        }

        public DataDetailViewModel()
        {
            this.data = new T();
        }

        public DataDetailViewModel(T data)
        {
            this.data = data; 
        }
    }
}
