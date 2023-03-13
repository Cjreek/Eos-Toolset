using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Eos.Models;
using Eos.ViewModels.Base;

namespace Eos.Usercontrols
{
    public partial class SoundsetComboBox : UserControl
    {
        public SoundsetComboBox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<Soundset?> SelectedValueProperty = AvaloniaProperty.Register<SoundsetComboBox, Soundset?>("SelectedValue", null, false, BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsNullableProperty = AvaloniaProperty.Register<SoundsetComboBox, bool>("IsNullable", true);

        public Soundset? SelectedValue
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
