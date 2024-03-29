using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Eos.Services;
using Eos.ViewModels.Dialogs;

namespace Eos.Usercontrols
{
    public partial class IconControl : UserControl
    {
        public IconControl()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<string?> IconProperty = AvaloniaProperty.Register<IconControl, string?>("Icon", default, false, Avalonia.Data.BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsReadonlyProperty = AvaloniaProperty.Register<IconControl, bool>("IsReadonly", false);

        public string? Icon
        {
            get { return GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public bool IsReadonly
        {
            get { return GetValue(IsReadonlyProperty); }
            set { SetValue(IsReadonlyProperty, value); }
        }

        private void btLoadIcon_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = new IconSearchViewModel();
            WindowService.OpenDialog(viewModel);
            if (viewModel.ResultResRef != null)
                SetValue(IconProperty, viewModel.ResultResRef);
        }
    }
}
