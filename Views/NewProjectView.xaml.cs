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
using Eos.ViewModels;
using Ookii.Dialogs.Wpf;

namespace Eos.Views
{
    /// <summary>
    /// Interaktionslogik für NewProjectView.xaml
    /// </summary>
    public partial class NewProjectView : Page
    {
        public NewProjectView()
        {
            InitializeComponent();
            this.DataContextChanged += NewProjectView_DataContextChanged;
        }

        private void NewProjectView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is NewProjectViewModel vm)
                vm.OnError += Vm_OnError;
        }

        private void Vm_OnError(ViewModelBase viewModel, ViewModelErrorEventArgs args)
        {
            MessageBox.Show(Window.GetWindow(this), args.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void btOpenDlg_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is NewProjectViewModel vm)
            {
                VistaFolderBrowserDialog dlg = new VistaFolderBrowserDialog();
                dlg.ShowNewFolderButton = true;
                if (dlg.ShowDialog() ?? false)
                    vm.ProjectFolder = dlg.SelectedPath;
            }
        }
    }
}
