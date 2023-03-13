using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Eos.Extensions
{
    public class EnumSourceItem : ReactiveObject
    {
        public string? DisplayName { get; set; }
        public object? Value { get; set; }

        public void RaiseChanged()
        {
            IReactiveObjectExtensions.RaisePropertyChanged(this, nameof(Value));
        }
    }

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
                }).Where(esi => !IgnoreEnumValue(esi.Value)).ToArray();

                return result;
            }

            return new object { };
        }

        private bool IgnoreEnumValue(object? value)
        {
            if ((value == null) || (enumType == null)) return true;

            var enumName = Enum.GetName(enumType, value) ?? "INVALID_ENUM_VALUE";
            return enumType.GetField(enumName)?.GetCustomAttributes(typeof(IgnoreEnumValueAttribute), false).Any() ?? false;
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
    }
}
