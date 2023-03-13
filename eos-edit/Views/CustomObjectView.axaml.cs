using Avalonia.Controls;
using Eos.Types;
using Eos.ViewModels;

namespace Eos.Views
{
    public partial class CustomObjectView : LanguageAwarePage
    {
        public CustomObjectView()
        {
            InitializeComponent();
            DataContextChanged += CustomObjectView_DataContextChanged;
        }

        private void CustomObjectView_DataContextChanged(object? sender, System.EventArgs e)
        {
            //if (e.OldValue is CustomObjectViewModel vmOld)
            //    vmOld.RemoveAllEventsCalling(Vm_OnDeleteCustomProperty);

            //if (e.NewValue is CustomObjectViewModel vm)
            //{
            //    vm.RemoveAllEventsCalling(Vm_OnDeleteCustomProperty);
            //    vm.OnDeleteCustomProperty += Vm_OnDeleteCustomProperty;
            //}
        }

        private void Vm_OnDeleteCustomProperty(object? sender, DeleteCustomPropertyEventArgs e)
        {
            if (sender == this.DataContext)
            {
                e.CanDelete = false;
                //if (MessageBox.Show("Deleting this extension field will delete this field and its contents for all instances of this data type!\nAre you sure you want to delete this field?", "Confirm Deletion", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                //    e.CanDelete = true;
            }
        }
    }
}
