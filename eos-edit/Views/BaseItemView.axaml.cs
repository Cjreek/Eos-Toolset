using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.Linq;

namespace Eos.Views
{
    public class CritRangeDisplayConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int threatRange)
            {
                if (threatRange == 1) return "20";
                else return $"{20 - (threatRange - 1)}-20";
            }

            return "N/A";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class BaseItemView : LanguageAwarePage
    {
        public BaseItemView()
        {
            InitializeComponent();
        }
    }
}
