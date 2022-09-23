using Eos.Models.Tables;
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
using Eos.Types;
using System.Xml.Linq;
using static Eos.Views.PrerequisiteTableView;

namespace Eos.Views
{
    class Param1TemplateSelectorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value is RequirementType requType) && (parameter is FrameworkElement element))
            {
                object? resource = null;
                switch (requType)
                {
                    case RequirementType.SAVE:
                        resource = element.FindResource("saveParamTemplate");
                        if (resource is DataTemplate saveTemplate)
                            return saveTemplate;
                        break;

                    case RequirementType.VAR:
                        resource = element.FindResource("strParamTemplate");
                        if (resource is DataTemplate strTemplate)
                            return strTemplate;
                        break;

                    case RequirementType.BAB:
                    case RequirementType.SPELL:
                    case RequirementType.ARCSPELL:
                        resource = element.FindResource("intParamTemplate");
                        if (resource is DataTemplate intTemplate)
                            return intTemplate;
                        break;

                    case RequirementType.RACE:
                        resource = element.FindResource("raceParamTemplate");
                        if (resource is DataTemplate raceTemplate)
                            return raceTemplate;
                        break;

                    case RequirementType.CLASSNOT:
                    case RequirementType.CLASSOR:
                        resource = element.FindResource("classParamTemplate");
                        if (resource is DataTemplate classTemplate)
                            return classTemplate;
                        break;

                    case RequirementType.FEAT:
                    case RequirementType.FEATOR:
                        resource = element.FindResource("featParamTemplate");
                        if (resource is DataTemplate featTemplate)
                            return featTemplate;
                        break;

                    case RequirementType.SKILL:
                        resource = element.FindResource("skillParamTemplate");
                        if (resource is DataTemplate skillTemplate)
                            return skillTemplate;
                        break;
                }
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class Param2TemplateSelectorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value is RequirementType requType) && (parameter is FrameworkElement element))
            {
                object? resource = null;
                switch (requType)
                {
                    case RequirementType.SKILL:
                    case RequirementType.SAVE:
                    case RequirementType.VAR:
                        resource = element.FindResource("intParam2Template");
                        if (resource is DataTemplate strTemplate)
                            return strTemplate;
                        break;

                    default:
                        resource = element.FindResource("emptyTemplate");
                        if (resource is DataTemplate emptyTemplate)
                            return emptyTemplate;
                        break;
                }
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaktionslogik für PrerequisiteTableView.xaml
    /// </summary>
    public partial class PrerequisiteTableView : LanguageAwarePage
    {
        public class SaveType
        {
            public string Name { get; set; } = "";
            public int Value { get; set; }
        }

        public List<SaveType> SaveTypes { get; set; } = new List<SaveType>();

        public PrerequisiteTableView()
        {
            SaveTypes.Add(new SaveType()
            {
                Name = "Fortitude",
                Value = 1,
            });
            SaveTypes.Add(new SaveType()
            {
                Name = "Reflex",
                Value = 2,
            });
            SaveTypes.Add(new SaveType()
            {
                Name = "Willpower",
                Value = 3,
            });

            InitializeComponent();
        }
    }
}
