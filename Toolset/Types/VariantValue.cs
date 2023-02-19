using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public class VariantValue : INotifyPropertyChanged
    {
        private DataTypeDefinition? _dataType;
        private object? _value;

        public DataTypeDefinition? DataType
        {
            get { return _dataType; }
            set
            {
                if (_dataType != value)
                {
                    _dataType = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(Value));
                    Value = null;
                }
            }
        }
        public object? Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
