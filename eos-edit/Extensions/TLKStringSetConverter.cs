using Avalonia.Data.Converters;
using Eos.Nwn.Tlk;
using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Extensions
{
    internal class TLKStringSetConverter : IMultiValueConverter
    {
        private static TLKLanguage DefaultLanguage = MasterRepository.Project.DefaultLanguage;

        public bool AlwaysUseDefaultLanguage { get; set; } = false;

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if ((values.Count > 0) && (values[0] is String text))
            {
                return text;
            }
            if ((values.Count == 1) && (values[0] is TLKStringSet defaultStrings))
            {
                return defaultStrings[DefaultLanguage].Text;
            }
            else if (values.Count == 3)
            {
                if ((values[0] is TLKStringSet strings) && (values[1] is TLKLanguage lang) && (values[2] is bool gender))
                {
                    lang = AlwaysUseDefaultLanguage ? DefaultLanguage : lang;
                    var result = gender ? strings[lang].TextF : strings[lang].Text;
                    if (result == String.Empty)
                        result = gender ? strings[DefaultLanguage].TextF : strings[DefaultLanguage].Text;

                    return result;
                }
            }

            return "";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
