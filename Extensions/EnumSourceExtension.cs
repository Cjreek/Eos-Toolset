using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Eos.Extensions
{
    public class EnumSourceExtension : MarkupExtension
    {
        private Type? enumType;

        public EnumSourceExtension()
        {
            enumType = null;
        }

        public EnumSourceExtension(Type enumType)
        {
            this.enumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (enumType != null)
            {
                var enumValues = Enum.GetValues(enumType);
                var result = enumValues.Cast<int>().Select(e => new EnumSourceItem()
                {
                    Value = Enum.Parse(enumType, Enum.GetName(enumType, e) ?? "INVALID_ENUM_VALUE"),
                    DisplayName = GetDisplayName(e)
                }
                ).ToArray();

                return result;
            }

            return new object { };
        }

        private String GetDisplayName(int value)
        {
            if (enumType != null)
            {
                var enumName = Enum.GetName(enumType, value) ?? "INVALID_ENUM_VALUE";
                var attr = enumType.GetField(enumName)?.GetCustomAttributes(typeof(DisplayNameAttribute), false).FirstOrDefault() as DisplayNameAttribute;
                return attr?.DisplayName ?? enumName ?? String.Empty;
            }

            return String.Empty;
        }

        public class EnumSourceItem
        {
            public string? DisplayName { get; set; }
            public object? Value { get; set; }
        }
    }
}
