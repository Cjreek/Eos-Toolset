using Eos.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.Templates;
using Avalonia;
using Avalonia.Data;

namespace Eos.Extensions
{
    public class VariantValueTemplateSelector : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is InstancePropertyValueTemplateSelector selector)
            {
                if (value is DataTypeDefinition dataTypeDef)
                    return selector.SelectTemplate(dataTypeDef) ?? new DataTemplate();
                return selector.SpaceTemplate ?? new DataTemplate();
            }

            return new BindingNotification(new Exception(), BindingErrorType.Error);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return new BindingNotification(new NotSupportedException(), BindingErrorType.Error);
        }
    }
}
