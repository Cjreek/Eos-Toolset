using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Eos.Views.Dialogs
{
    public partial class IconSearchView : UserControl
    {
        public IconSearchView()
        {
            InitializeComponent();
        }

        private void IconSearchView_Loaded(object? sender, RoutedEventArgs e)
        {
            searchTextbox.Focus();
        }
    }
}
