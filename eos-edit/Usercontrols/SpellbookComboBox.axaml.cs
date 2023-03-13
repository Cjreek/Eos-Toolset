using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Eos.Models;
using Eos.Models.Tables;
using Eos.ViewModels.Base;

namespace Eos.Usercontrols
{
    public partial class SpellbookComboBox : UserControl
    {
        public SpellbookComboBox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<Spellbook?> SelectedValueProperty = AvaloniaProperty.Register<SpellbookComboBox, Spellbook?>("SelectedValue", null, false, Avalonia.Data.BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsNullableProperty = AvaloniaProperty.Register<SpellbookComboBox, bool>("IsNullable", true);

        public Spellbook? SelectedValue
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
