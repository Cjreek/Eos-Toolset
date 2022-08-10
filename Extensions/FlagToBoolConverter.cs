using Eos.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Eos.Extensions
{
    [ValueConversion(typeof(Enum), typeof(bool))]
    internal class FlagToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is Grid paramGrid)
            {
                var item = (Enum?)((FrameworkElement)paramGrid.FindName("paramItem"))?.Tag;
                var flags = (Enum?)((FrameworkElement)paramGrid.FindName("paramFlags"))?.Tag;
                return ((System.Convert.ToInt64(flags) & System.Convert.ToInt64(item)) != 0);
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is Grid paramGrid)
            {
                var item = ((FrameworkElement)paramGrid.FindName("paramItem"))?.Tag;
                var flags = ((FrameworkElement)paramGrid.FindName("paramFlags"))?.Tag;

                if ((item != null) && (flags != null))
                {
                    if ((bool)value)
                        flags = Enum.ToObject(item.GetType(), (System.Convert.ToInt64(flags) | System.Convert.ToInt64(item)));
                    else 
                        flags = Enum.ToObject(item.GetType(), (System.Convert.ToInt64(flags) & ~System.Convert.ToInt64(item)));

                    return flags;
                }
            }

            return false;
        }
    }
}
