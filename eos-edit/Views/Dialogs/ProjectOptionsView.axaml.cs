using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Eos.ViewModels.Dialogs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Eos.Views.Dialogs
{
    public partial class ProjectOptionsView : UserControl
    {
        public ProjectOptionsView()
        {
            InitializeComponent();
        }

        private void btOpenDlg_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProjectOptionsViewModel vm)
            {
                if ((Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime app) && (app.MainWindow != null))
                {
                    var dlg = new OpenFolderDialog();
                    dlg.ShowAsync(app.MainWindow).ContinueWith(t =>
                    {
                        if (t.Result != null)
                        {
                            var target = (String?)((Button)sender)?.Tag;
                            switch (target)
                            {
                                case "2DA":
                                    vm.SettingsCopy.Export.TwoDAFolder = t.Result;
                                    break;
                                case "HAK":
                                    vm.SettingsCopy.Export.HakFolder = t.Result;
                                    break;
                                case "ERF":
                                    vm.SettingsCopy.Export.ErfFolder = t.Result;
                                    break;
                                case "TLK":
                                    vm.SettingsCopy.Export.TlkFolder = t.Result;
                                    break;
                                case "INC":
                                    vm.SettingsCopy.Export.IncludeFolder = t.Result;
                                    break;
                                case "EXT":
                                    vm.ExternalPathToAdd = t.Result;
                                    break;
                            }
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }

        private void btOpenTlk_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProjectOptionsViewModel vm)
            {
                if ((Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime app) && (app.MainWindow != null))
                {
                    var dlg = new OpenFileDialog();
                    dlg.AllowMultiple = false;
                    dlg.Filters?.Add(new FileDialogFilter() { Name = "Talk Table File (*.tlk)", Extensions = { "tlk" } });
                    dlg.ShowAsync(app.MainWindow).ContinueWith(t =>
                    {
                        if ((t.Result != null) && (t.Result.Any()))
                        {
                            vm.SettingsCopy.Export.BaseTlkFile = t.Result.First();
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }
    }
}
