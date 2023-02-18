using Eos.Types;
using Eos.ViewModels;
using Eos.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Eos.Views
{
    /// <summary>
    /// Interaktionslogik für ModelExtensionView.xaml
    /// </summary>
    public partial class ModelExtensionView : LanguageAwarePage
    {
        public ModelExtensionView()
        {
            InitializeComponent();
            this.DataContextChanged += ModelExtensionView_DataContextChanged; 
        }

        private void ModelExtensionView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ModelExtensionViewModel vmOld)
                vmOld.RemoveAllEventsCalling(Vm_OnDeleteCustomProperty);
            if (DataContext is ModelExtensionViewModel vm)
            {
                vm.RemoveAllEventsCalling(Vm_OnDeleteCustomProperty);
                vm.OnDeleteCustomProperty += Vm_OnDeleteCustomProperty;
            }
        }

        private void Vm_OnDeleteCustomProperty(object? sender, DeleteCustomPropertyEventArgs e)
        {
            e.CanDelete = false;
            if (MessageBox.Show("Deleting this extension field will delete this field and its contents for all instances of this data type!\nAre you sure you want to delete this field?", "Confirm Deletion", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                e.CanDelete = true;
        }
    }
}
