using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Eos.Models;
using Eos.Repositories;
using Eos.Services;
using Eos.ViewModels.Base;
using Eos.ViewModels.Dialogs;

namespace Eos.Usercontrols
{
    public partial class AppearanceSoundsetComboBox : UserControl
    {
        public AppearanceSoundsetComboBox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<AppearanceSoundset?> SelectedValueProperty = AvaloniaProperty.Register<AppearanceSoundsetComboBox, AppearanceSoundset?>("SelectedValue", null, false, BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsNullableProperty = AvaloniaProperty.Register<AppearanceSoundsetComboBox, bool>("IsNullable", true);

        public AppearanceSoundset? SelectedValue
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
            var viewModel = new AppearanceSoundsetSearchViewModel(MasterRepository.AppearanceSoundsets);
            WindowService.OpenDialog(viewModel);
            if (viewModel.ResultModel != null)
                SetValue(SelectedValueProperty, viewModel.ResultModel);
        }

        private void btGoto_Click(object sender, RoutedEventArgs e)
        {
            MessageDispatcher.Send(MessageType.OpenDetail, SelectedValue, true);
        }
    }
}
