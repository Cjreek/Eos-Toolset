using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Eos.Models;
using Eos.Repositories;
using Eos.Services;
using Eos.ViewModels.Base;
using Eos.ViewModels.Dialogs;

namespace Eos.Usercontrols
{
    public partial class TrapComboBox : UserControl
    {
        public TrapComboBox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<Trap?> SelectedValueProperty = AvaloniaProperty.Register<TrapComboBox, Trap?>("SelectedValue", null, false, Avalonia.Data.BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsNullableProperty = AvaloniaProperty.Register<TrapComboBox, bool>("IsNullable", true);

        public Trap? SelectedValue
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
            var viewModel = new TrapSearchViewModel(MasterRepository.Traps);
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
