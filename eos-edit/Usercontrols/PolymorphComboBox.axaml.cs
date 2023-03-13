using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Eos.Models;

namespace Eos.Usercontrols
{
    public partial class PolymorphComboBox : UserControl
    {
        public PolymorphComboBox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<Polymorph?> SelectedValueProperty = AvaloniaProperty.Register<PolymorphComboBox, Polymorph?>("SelectedValue", null, false, BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsNullableProperty = AvaloniaProperty.Register<PolymorphComboBox, bool>("IsNullable", true);

        public Polymorph? SelectedValue
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
