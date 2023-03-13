using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Eos.Views.Dialogs
{
    public class BoolToAutoSizeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool autosize)
                return autosize ? SizeToContent.WidthAndHeight : SizeToContent.Manual;

            return SizeToContent.Manual;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class ViewWindow : Window
    {
        public ViewWindow()
        {
            InitializeComponent();
        }
    }
}
