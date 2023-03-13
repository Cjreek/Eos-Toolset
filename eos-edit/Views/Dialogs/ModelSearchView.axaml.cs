using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Eos.Views.Dialogs
{
    public partial class ModelSearchView : UserControl
    {
        public ModelSearchView()
        {
            InitializeComponent();
        }

        private void ModelSearchView_Loaded(object? sender, RoutedEventArgs e)
        {
            searchTextbox.Focus();
        }
    }
}
