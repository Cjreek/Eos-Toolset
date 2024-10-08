using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Eos.Models;
using Eos.Repositories;
using Eos.Services;
using Eos.ViewModels.Base;
using Eos.ViewModels.Dialogs;
using System.Threading.Tasks;

namespace Eos.Usercontrols
{
    public partial class RangedDamageTypeComboBox : UserControl
    {
        public RangedDamageTypeComboBox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<RangedDamageType?> SelectedValueProperty = AvaloniaProperty.Register<RangedDamageTypeComboBox, RangedDamageType?>("SelectedValue", null, false, Avalonia.Data.BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsNullableProperty = AvaloniaProperty.Register<RangedDamageTypeComboBox, bool>("IsNullable", true);

        public RangedDamageType? SelectedValue
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
            var viewModel = new RangedDamageTypeSearchViewModel(MasterRepository.RangedDamageTypes);
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
