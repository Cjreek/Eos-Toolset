using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.Linq;

namespace Eos.Views
{
    public class PortraitToIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string resRef)
            {
                return $"po_{resRef}s";
            }

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class PortraitView : LanguageAwarePage
    {
        public PortraitView()
        {
            InitializeComponent();
        }
    }
}
