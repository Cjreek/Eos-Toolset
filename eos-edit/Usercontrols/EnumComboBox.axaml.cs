using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Eos.Extensions;

namespace Eos.Usercontrols
{
    public partial class EnumComboBox : UserControl
    {
        public EnumComboBox()
        {
            InitializeComponent();
        }

        public EnumSourceItem? SelectedItem { get; set; }

        public static readonly StyledProperty<object?> ItemsSourceProperty = AvaloniaProperty.Register<EnumComboBox, object?>("ItemsSource");
        public static readonly StyledProperty<object?> SelectedValueProperty = AvaloniaProperty.Register<EnumComboBox, object?>("SelectedValue", null, false, Avalonia.Data.BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsNullableProperty = AvaloniaProperty.Register<EnumComboBox, bool>("IsNullable");

        public object? ItemsSource
        {
            get { return GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public object? SelectedValue
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
