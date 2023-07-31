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
    public partial class CustomDynamicTableComboBox : UserControl
    {
        public CustomDynamicTableComboBox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<CustomDynamicTable?> CustomDynamicTableTemplateProperty = AvaloniaProperty.Register<CustomDynamicTableComboBox, CustomDynamicTable?>("CustomDynamicTableTemplate", null);
        public static readonly StyledProperty<CustomDynamicTableInstance?> SelectedValueProperty = AvaloniaProperty.Register<CustomDynamicTableComboBox, CustomDynamicTableInstance?>("SelectedValue", null, false, BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsNullableProperty = AvaloniaProperty.Register<CustomDynamicTableComboBox, bool>("IsNullable", true);

        public CustomDynamicTable? CustomDynamicTableTemplate
        {
            get { return GetValue(CustomDynamicTableTemplateProperty); }
            set { SetValue(CustomDynamicTableTemplateProperty, value); }
        }

        public CustomDynamicTableInstance? SelectedValue
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
            if (CustomDynamicTableTemplate != null)
            {
                var viewModel = new CustomDynamicTableInstanceSearchViewModel(CustomDynamicTableTemplate, MasterRepository.Project.CustomDynamicTableRepositories[CustomDynamicTableTemplate]);
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
