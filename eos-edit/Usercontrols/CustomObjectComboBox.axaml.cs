using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Eos.Models;
using Eos.Models.Tables;
using Eos.Repositories;
using Eos.Services;
using Eos.ViewModels.Base;
using Eos.ViewModels.Dialogs;

namespace Eos.Usercontrols
{
    public partial class CustomObjectComboBox : UserControl
    {
        public CustomObjectComboBox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<CustomObject?> CustomObjectTemplateProperty = AvaloniaProperty.Register<CustomObjectComboBox, CustomObject?>("CustomObjectTemplate", null);
        public static readonly StyledProperty<CustomObjectInstance?> SelectedValueProperty = AvaloniaProperty.Register<CustomObjectComboBox, CustomObjectInstance?>("SelectedValue", null, false, BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsNullableProperty = AvaloniaProperty.Register<CustomObjectComboBox, bool>("IsNullable", true);

        public CustomObject? CustomObjectTemplate
        {
            get { return GetValue(CustomObjectTemplateProperty); }
            set { SetValue(CustomObjectTemplateProperty, value); }
        }

        public CustomObjectInstance? SelectedValue
        {
            get { return GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        public bool IsNullable
        {
            get { return GetValue(IsNullableProperty); }
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
