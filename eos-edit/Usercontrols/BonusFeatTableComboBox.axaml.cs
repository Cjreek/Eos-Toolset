using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Eos.Models.Tables;
using Eos.ViewModels.Base;

namespace Eos.Usercontrols
{
    public partial class BonusFeatTableComboBox : UserControl
    {
        public BonusFeatTableComboBox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<BonusFeatsTable?> SelectedValueProperty = AvaloniaProperty.Register<BonusFeatTableComboBox, BonusFeatsTable?>("SelectedValue", null, false, Avalonia.Data.BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsNullableProperty = AvaloniaProperty.Register<BonusFeatTableComboBox, bool>("IsNullable", true);

        public BonusFeatsTable? SelectedValue
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

        private void btGoto_Click(object sender, RoutedEventArgs e)
        {
            MessageDispatcher.Send(MessageType.OpenDetail, SelectedValue, true);
        }
    }
}
