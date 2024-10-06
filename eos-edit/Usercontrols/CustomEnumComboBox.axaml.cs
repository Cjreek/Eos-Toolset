using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Eos.Models.Tables;

namespace Eos.Usercontrols
{
    public partial class CustomEnumComboBox : UserControl
    {
        public CustomEnumComboBox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<CustomEnum?> CustomEnumProperty = AvaloniaProperty.Register<CustomEnumComboBox, CustomEnum?>("CustomEnum");
        public static readonly StyledProperty<CustomEnumItem?> SelectedValueProperty = AvaloniaProperty.Register<CustomEnumComboBox, CustomEnumItem?>("SelectedValue", null, false, Avalonia.Data.BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsNullableProperty = AvaloniaProperty.Register<CustomEnumComboBox, bool>("IsNullable");

        public CustomEnum? CustomEnum
        {
            get { return GetValue(CustomEnumProperty); }
            set { SetValue(CustomEnumProperty, value); }
        }

        public object? SelectedValue
        {
            get { return GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        public bool IsNullable
        {
            get { return (bool)GetValue(IsNullableProperty); }
            set { SetValue(IsNullableProperty, value); }
        }

        private void btClear_Click(object sender, RoutedEventArgs e)
        {
            SetValue(SelectedValueProperty, null);
        }
    }
}
