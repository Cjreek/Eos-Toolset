using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Eos.Models.Tables;
using Eos.ViewModels.Base;

namespace Eos.Usercontrols
{
    public partial class ItemPropertyCostTableComboBox : UserControl
    {
        public ItemPropertyCostTableComboBox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<ItemPropertyCostTable?> SelectedValueProperty = AvaloniaProperty.Register<ItemPropertyCostTableComboBox, ItemPropertyCostTable?>("SelectedValue", null, false, Avalonia.Data.BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsNullableProperty = AvaloniaProperty.Register<ItemPropertyCostTableComboBox, bool>("IsNullable", true);

        public ItemPropertyCostTable? SelectedValue
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
