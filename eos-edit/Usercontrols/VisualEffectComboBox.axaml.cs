using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Eos.Models;

namespace Eos.Usercontrols
{
    public partial class VisualEffectComboBox : UserControl
    {
        public VisualEffectComboBox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<VisualEffect?> SelectedValueProperty = AvaloniaProperty.Register<VisualEffectComboBox, VisualEffect?>("SelectedValue", null, false, BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsNullableProperty = AvaloniaProperty.Register<VisualEffectComboBox, bool>("IsNullable", true);

        public VisualEffect? SelectedValue
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
