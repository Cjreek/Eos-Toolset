using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Extensions
{
    public class ColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is uint uiColor)
                return Avalonia.Media.Color.FromUInt32(uiColor | 0xFF000000);

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Avalonia.Media.Color color)
                return color.ToUint32() & 0x00FFFFFF;

            return null;
        }
    }
}
