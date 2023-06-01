using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Eos.Models.Tables;
using Eos.ViewModels.Base;

namespace Eos.Usercontrols
{
    public partial class SkillPreferencesTableComboBox : UserControl
    {
        public SkillPreferencesTableComboBox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<PackageSkillPreferencesTable?> SelectedValueProperty = AvaloniaProperty.Register<SkillPreferencesTableComboBox, PackageSkillPreferencesTable?>("SelectedValue", null, false, Avalonia.Data.BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsNullableProperty = AvaloniaProperty.Register<SkillPreferencesTableComboBox, bool>("IsNullable", true);

        public PackageSkillPreferencesTable? SelectedValue
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
