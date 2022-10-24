using Eos.Models;
using Eos.Models.Tables;
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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Eos.Views
{
    class VariantValueTemplateSelector : IValueConverter
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

    class InstancePropertyValueTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? ErrorTemplate { get; set; }
        public DataTemplate? SpaceTemplate { get; set; }
        public DataTemplate? IntTemplate { get; set; }
        public DataTemplate? DoubleTemplate { get; set; }
        public DataTemplate? BoolTemplate { get; set; }
        public DataTemplate? StringTemplate { get; set; }
        public DataTemplate? VariantTemplate { get; set; }
        public DataTemplate? RaceTemplate { get; set; }
        public DataTemplate? ClassTemplate { get; set; }
        public DataTemplate? SkillTemplate { get; set; }
        public DataTemplate? DomainTemplate { get; set; }
        public DataTemplate? SpellTemplate { get; set; }
        public DataTemplate? FeatTemplate { get; set; }
        public DataTemplate? CustomObjectTemplate { get; set; }
        public DataTemplate? CustomEnumTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            DataTypeDefinition? dataTypeDef = null;
            if (item is CustomValueInstance valueInstance)
                dataTypeDef = valueInstance.Property.DataType;
            else if (item is DataTypeDefinition itemDataTypeDef)
                dataTypeDef = itemDataTypeDef;

            if (dataTypeDef != null)
            {
                if (dataTypeDef.ID == Guid.Parse("a136669b-e618-4be1-9a29-8b76f85c60be"))
                    return SpaceTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(int))
                    return IntTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(double))
                    return DoubleTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(bool))
                    return BoolTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(string))
                    return StringTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(VariantValue))
                    return VariantTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(Race))
                    return RaceTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(CharacterClass))
                    return ClassTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(Domain))
                    return DomainTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(Spell))
                    return SpellTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(Skill))
                    return SkillTemplate ?? ErrorTemplate;
                if (dataTypeDef.Type == typeof(Feat))
                    return FeatTemplate ?? ErrorTemplate;

                if (dataTypeDef.CustomType is CustomObject customObject)
                    return CustomObjectTemplate ?? ErrorTemplate;
                if (dataTypeDef.CustomType is CustomEnum customEnum)
                    return CustomEnumTemplate ?? ErrorTemplate;

                return ErrorTemplate;
            }

            return SpaceTemplate;
        }
    }

    class InstanceValueDictionaryConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaktionslogik für CustomObjectInstanceView.xaml
    /// </summary>
    public partial class CustomObjectInstanceView : LanguageAwarePage
    {
        public CustomObjectInstanceView()
        {
            InitializeComponent();
        }
    }
}
