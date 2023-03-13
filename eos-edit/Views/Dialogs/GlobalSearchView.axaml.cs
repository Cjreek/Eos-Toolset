using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Eos.Views.Dialogs
{
    public partial class GlobalSearchView : LanguageAwarePage
    {
        public GlobalSearchView()
        {
            InitializeComponent();
        }

        private void ModelSearchView_Loaded(object? sender, RoutedEventArgs e)
        {
            searchTextbox.Focus();
        }
    }
}
