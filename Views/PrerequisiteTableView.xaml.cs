using Eos.Models.Tables;
using System;
using System.Collections.Generic;
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
    public class PrerequisiteTypeTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (container is FrameworkElement element && item is PrerequisiteTableItem requTableItem)
            {
                object? resource = null; 
                switch (requTableItem.RequirementType)
                {
                    case Types.RequirementType.VAR:
                        resource = element.FindResource("strParamTemplate");
                        if (resource is DataTemplate strTemplate)
                            return strTemplate;
                        break;

                    case Types.RequirementType.BAB:
                    case Types.RequirementType.SPELL:
                    case Types.RequirementType.ARCSPELL:
                        resource = element.FindResource("intParamTemplate");
                        if (resource is DataTemplate intTemplate)
                            return intTemplate;
                        break;

                    case Types.RequirementType.RACE:
                        resource = element.FindResource("raceParamTemplate");
                        if (resource is DataTemplate raceTemplate)
                            return raceTemplate;
                        break;

                    case Types.RequirementType.CLASSNOT:
                    case Types.RequirementType.CLASSOR:
                        resource = element.FindResource("classParamTemplate");
                        if (resource is DataTemplate classTemplate)
                            return classTemplate;
                        break;

                    case Types.RequirementType.FEAT:
                    case Types.RequirementType.FEATOR:
                        resource = element.FindResource("featParamTemplate");
                        if (resource is DataTemplate featTemplate)
                            return featTemplate;
                        break;

                    case Types.RequirementType.SKILL:
                        resource = element.FindResource("skillParamTemplate");
                        if (resource is DataTemplate skillTemplate)
                            return skillTemplate;
                        break;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }

    public class PrerequisiteTypeParam2TemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (container is FrameworkElement element && item is PrerequisiteTableItem requTableItem)
            {
                object? resource = null;
                switch (requTableItem.RequirementType)
                {
                    case Types.RequirementType.SKILL:
                    case Types.RequirementType.SAVE:
                    case Types.RequirementType.VAR:
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

            return base.SelectTemplate(item, container);
        }
    }

    /// <summary>
    /// Interaktionslogik für PrerequisiteTableView.xaml
    /// </summary>
    public partial class PrerequisiteTableView : LanguageAwarePage
    {
        public PrerequisiteTableView()
        {
            InitializeComponent();
        }
    }
}
