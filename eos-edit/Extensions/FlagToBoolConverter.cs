using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Eos.Types;
using Eos.Usercontrols;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Eos.Extensions
{
    // WARNING: Absolutely terrible stuff ahead

    internal class FlagToBoolConverter : AvaloniaObject, IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is Grid paramGrid)
            {
                var item = (EnumSourceItem?)((Control?)paramGrid.GetVisualChildren().Where(ctrl => ctrl.Name == "paramItem").FirstOrDefault())?.Tag;
                var flags = (FlagListbox?)((Control?)paramGrid.GetVisualChildren().Where(ctrl => ctrl.Name == "paramFlags").FirstOrDefault())?.Tag;
                return ((System.Convert.ToInt64(flags?.Flags) & System.Convert.ToInt64(item?.Value)) != 0);
            }

            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is Grid paramGrid)
            {
                var item = (EnumSourceItem?)((Control?)paramGrid.GetVisualChildren().Where(ctrl => ctrl.Name == "paramItem").FirstOrDefault())?.Tag;
                var flags = (FlagListbox?)((Control?)paramGrid.GetVisualChildren().Where(ctrl => ctrl.Name == "paramFlags").FirstOrDefault())?.Tag;

                if ((item?.Value != null) && (flags?.Flags != null))
                {
                    if ((bool?)value ?? false)
                        flags.Flags = Enum.ToObject(item.Value.GetType(), (System.Convert.ToInt64(flags.Flags) | System.Convert.ToInt64(item.Value)));
                    else
                        flags.Flags = Enum.ToObject(item.Value.GetType(), (System.Convert.ToInt64(flags.Flags) & ~System.Convert.ToInt64(item.Value)));

                    return item.Value;
                }
            }

            return false;
        }
    }
}
