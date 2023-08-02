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
    public partial class CustomTableComboBox : UserControl
    {
        public CustomTableComboBox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<CustomTable?> CustomTableTemplateProperty = AvaloniaProperty.Register<CustomTableComboBox, CustomTable?>("CustomTableTemplate", null);
        public static readonly StyledProperty<CustomTableInstance?> SelectedValueProperty = AvaloniaProperty.Register<CustomTableComboBox, CustomTableInstance?>("SelectedValue", null, false, BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsNullableProperty = AvaloniaProperty.Register<CustomTableComboBox, bool>("IsNullable", true);

        public CustomTable? CustomTableTemplate
        {
            get { return GetValue(CustomTableTemplateProperty); }
            set { SetValue(CustomTableTemplateProperty, value); }
        }

        public CustomTableInstance? SelectedValue
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
            if (CustomTableTemplate != null)
            {
                var viewModel = new CustomTableInstanceSearchViewModel(CustomTableTemplate, MasterRepository.Project.CustomTableRepositories[CustomTableTemplate]);
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
