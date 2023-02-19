using Eos.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace Eos.Extensions
{
    public class VariantValueTemplateSelector : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is InstancePropertyValueTemplateSelector selector)
            {
                if (value is DataTypeDefinition dataTypeDef)
                    return selector.SelectTemplate(dataTypeDef, new DependencyObject()) ?? new DataTemplate();
                return selector.SpaceTemplate ?? new DataTemplate();
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
