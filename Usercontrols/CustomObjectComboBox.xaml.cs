using Eos.Models;
using Eos.Models.Tables;
using Eos.Repositories;
using Eos.Services;
using Eos.ViewModels.Base;
using Eos.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Eos.Usercontrols
{
    /// <summary>
    /// Interaktionslogik für CustomObjectComboBox.xaml
    /// </summary>
    public partial class CustomObjectComboBox : UserControl
    {
        public CustomObjectComboBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty CustomObjectTemplateProperty = DependencyProperty.Register("CustomObjectTemplate", typeof(CustomObject), typeof(CustomObjectComboBox), new PropertyMetadata(null));
        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register("SelectedValue", typeof(CustomObjectInstance), typeof(CustomObjectComboBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty IsNullableProperty = DependencyProperty.Register("IsNullable", typeof(bool), typeof(CustomObjectComboBox), new PropertyMetadata(true));

        public CustomObject? CustomObjectTemplate
        {
            get { return (CustomObject)GetValue(CustomObjectTemplateProperty); }
            set { SetValue(CustomObjectTemplateProperty, value); }
        }

        public CustomObjectInstance? SelectedValue
        {
            get { return (CustomObjectInstance)GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        public bool IsNullable
        {
            get { return (bool)GetValue(IsNullableProperty); }
            set { SetValue(IsNullableProperty, value); }
        }

        private void btClear_Click(object sender, RoutedEventArgs e)
        {
            SetValue(SelectedValueProperty, null);
        }

        private void btSearch_Click(object sender, RoutedEventArgs e)
        {
            if (CustomObjectTemplate != null)
            {
                var viewModel = new CustomObjectInstanceSearchViewModel(CustomObjectTemplate, MasterRepository.Project.CustomObjectRepositories[CustomObjectTemplate]);
                WindowService.OpenDialog(viewModel);
                if (viewModel.ResultModel != null)
                    SetValue(SelectedValueProperty, viewModel.ResultModel);
            }
        }

        private void btGoto_Click(object sender, RoutedEventArgs e)
        {
            MessageDispatcher.Send(MessageType.OpenDetail, SelectedValue, true);
        }
    }
}
