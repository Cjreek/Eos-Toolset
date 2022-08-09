using Eos.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Eos.Extensions
{
    [ValueConversion(typeof(TLKStringSet), typeof(String))]
    internal class TLKStringSetConverter : IMultiValueConverter
    {
        private static TLKLanguage DefaultLanguage = TLKLanguage.English;

        public bool AlwaysUseDefaultLanguage { get; set; } = false;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 3)
            {
                if (values[0] is string)
                    return values[0];

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
