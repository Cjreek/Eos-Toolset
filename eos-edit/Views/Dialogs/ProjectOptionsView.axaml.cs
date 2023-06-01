using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Eos.Repositories;
using Eos.ViewModels.Dialogs;
using System;
using System.IO;
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
                    var target = (String?)((Button)sender)?.Tag;

                    var dlg = new OpenFolderDialog();
                    switch (target)
                    {
                        case "BACKUP":
                            dlg.Directory = Path.GetFullPath(vm.SettingsCopy.BackupFolder, MasterRepository.Project.ProjectFolder);
                            break;
                        case "2DA":
                            dlg.Directory = Path.GetFullPath(vm.SettingsCopy.Export.TwoDAFolder, MasterRepository.Project.ProjectFolder);
                            break;
                        case "SSF":
                            dlg.Directory = Path.GetFullPath(vm.SettingsCopy.Export.SsfFolder, MasterRepository.Project.ProjectFolder);
                            break;
                        case "HAK":
                            dlg.Directory = Path.GetFullPath(vm.SettingsCopy.Export.HakFolder, MasterRepository.Project.ProjectFolder);
                            break;
                        case "ERF":
                            dlg.Directory = Path.GetFullPath(vm.SettingsCopy.Export.ErfFolder, MasterRepository.Project.ProjectFolder);
                            break;
                        case "TLK":
                            dlg.Directory = Path.GetFullPath(vm.SettingsCopy.Export.TlkFolder, MasterRepository.Project.ProjectFolder);
                            break;
                        case "INC":
                            dlg.Directory = Path.GetFullPath(vm.SettingsCopy.Export.IncludeFolder, MasterRepository.Project.ProjectFolder);
                            break;
                        case "EXT":
                            dlg.Directory = Path.GetFullPath(vm.ExternalPathToAdd, MasterRepository.Project.ProjectFolder);
                            break;
                    }

                    dlg.ShowAsync(app.MainWindow).ContinueWith(t =>
                    {
                        if (t.Result != null)
                        {
                            var resultPath = Path.GetRelativePath(MasterRepository.Project.ProjectFolder, t.Result);
                            if (resultPath.Contains($"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..")) // 3+ back? Just use the absolute path
                                resultPath = t.Result;
                            switch (target)
                            {
                                case "BACKUP":
                                    vm.SettingsCopy.BackupFolder = resultPath;
                                    break;
                                case "2DA":
                                    vm.SettingsCopy.Export.TwoDAFolder = resultPath;
                                    break;
                                case "SSF":
                                    vm.SettingsCopy.Export.SsfFolder = resultPath;
                                    break;
                                case "HAK":
                                    vm.SettingsCopy.Export.HakFolder = resultPath;
                                    break;
                                case "ERF":
                                    vm.SettingsCopy.Export.ErfFolder = resultPath;
                                    break;
                                case "TLK":
                                    vm.SettingsCopy.Export.TlkFolder = resultPath;
                                    break;
                                case "INC":
                                    vm.SettingsCopy.Export.IncludeFolder = resultPath;
                                    break;
                                case "EXT":
                                    vm.ExternalPathToAdd = resultPath;
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
