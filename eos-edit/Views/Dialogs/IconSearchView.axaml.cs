using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System.Drawing;

namespace Eos.Views.Dialogs
{
    public partial class IconSearchView : LanguageAwarePage
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
