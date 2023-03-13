using Eos.ViewModels.Base;
using Eos.ViewModels.Dialogs;
using Ookii.Dialogs.Wpf;
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

namespace Eos.Views.Dialogs
{
    /// <summary>
    /// Interaktionslogik für ProjectOptionsView.xaml
    /// </summary>
    public partial class ProjectOptionsView : Page
    {
        public ProjectOptionsView()
        {
            InitializeComponent();
        }

        private void btOpenDlg_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProjectOptionsViewModel vm)
            {
                VistaFolderBrowserDialog dlg = new VistaFolderBrowserDialog();
                dlg.ShowNewFolderButton = true;
                if (dlg.ShowDialog() ?? false)
                {
                    var target = (String)((Button)sender).Tag;
                    switch (target)
                    {
                        case "2DA": 
                            vm.SettingsCopy.Export.TwoDAFolder = dlg.SelectedPath;
                            break;
                        case "HAK":
                            vm.SettingsCopy.Export.HakFolder = dlg.SelectedPath;
                            break;
                        case "ERF":
                            vm.SettingsCopy.Export.ErfFolder = dlg.SelectedPath;
                            break;
                        case "TLK":
                            vm.SettingsCopy.Export.TlkFolder = dlg.SelectedPath;
                            break;
                        case "INC":
                            vm.SettingsCopy.Export.IncludeFolder = dlg.SelectedPath;
                            break;
                    }
                }
            }
        }

        private void btOpenTlk_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProjectOptionsViewModel vm)
            {
                var dlg = new VistaOpenFileDialog();
                dlg.CheckFileExists = true;
                dlg.CheckPathExists = true;
                dlg.Filter = "Talk Table File (*.tlk)|*.tlk";
                if (dlg.ShowDialog() ?? false)
                    vm.SettingsCopy.Export.BaseTlkFile = dlg.FileName;
            }
        }
    }
}
