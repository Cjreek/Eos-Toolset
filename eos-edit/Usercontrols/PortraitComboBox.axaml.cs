using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Eos.Models;

namespace Eos.Usercontrols
{
    public partial class PortraitComboBox : UserControl
    {
        public PortraitComboBox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<Portrait?> SelectedValueProperty = AvaloniaProperty.Register<PortraitComboBox, Portrait?>("SelectedValue", null, false, BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsNullableProperty = AvaloniaProperty.Register<PortraitComboBox, bool>("IsNullable", true);

        public Portrait? SelectedValue
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
    }
}
