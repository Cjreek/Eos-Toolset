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
