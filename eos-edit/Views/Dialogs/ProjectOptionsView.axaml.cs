using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Eos.Repositories;
using Eos.ViewModels.Dialogs;
using System;
using System.IO;
using Avalonia.Platform.Storage;

namespace Eos.Views.Dialogs
{
    public partial class ProjectOptionsView : UserControl
    {
        public ProjectOptionsView()
        {
            InitializeComponent();
        }

        private async void btOpenDlg_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProjectOptionsViewModel vm)
            {
                if ((Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime app) && (app.MainWindow != null))
                {
                    var target = (String?)((Button)sender)?.Tag;
                    
                    var directory = "";
                    switch (target)
                    {
                        case "BACKUP":
                            directory = Path.GetFullPath(vm.SettingsCopy.BackupFolder, MasterRepository.Project.ProjectFolder);
                            break;
                        case "2DA":
                            directory = Path.GetFullPath(vm.SettingsCopy.Export.TwoDAFolder, MasterRepository.Project.ProjectFolder);
                            break;
                        case "SSF":
                            directory = Path.GetFullPath(vm.SettingsCopy.Export.SsfFolder, MasterRepository.Project.ProjectFolder);
                            break;
                        case "HAK":
                            directory = Path.GetFullPath(vm.SettingsCopy.Export.HakFolder, MasterRepository.Project.ProjectFolder);
                            break;
                        case "ERF":
                            directory = Path.GetFullPath(vm.SettingsCopy.Export.ErfFolder, MasterRepository.Project.ProjectFolder);
                            break;
                        case "TLK":
                            directory = Path.GetFullPath(vm.SettingsCopy.Export.TlkFolder, MasterRepository.Project.ProjectFolder);
                            break;
                        case "INC":
                            directory = Path.GetFullPath(vm.SettingsCopy.Export.IncludeFolder, MasterRepository.Project.ProjectFolder);
                            break;
                        case "EXT":
                            directory = Path.GetFullPath(vm.ExternalPathToAdd, MasterRepository.Project.ProjectFolder);
                            break;
                    }
                    
                    var result = await app.MainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
                    {
                        Title = "Select base folder for where to save this new project",
                        AllowMultiple = false,
                        SuggestedStartLocation = await app.MainWindow.StorageProvider.TryGetFolderFromPathAsync(directory)
                    });
                    if (result.Count >= 1)
                    {
                        var path = result[0].TryGetLocalPath() ?? "";
                        if (path != "")
                        {
                            var resultPath = Path.GetRelativePath(MasterRepository.Project.ProjectFolder, path);
                            if (resultPath.Contains(
                                    $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..")) // 3+ back? Just use the absolute path
                                resultPath = path;
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
                    }
                }
            }
        }

        private async void btOpenTlk_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProjectOptionsViewModel vm)
            {
                if ((Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime app) && (app.MainWindow != null))
                {
                    var result = await app.MainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
                    {
                        Title = "Select Base Talk Table File",
                        AllowMultiple = false,
                        FileTypeFilter = new FilePickerFileType[]
                        {
                            new ("Talk Table File (*.tlk)") { Patterns = new[] { "*.tlk" } }
                        }
                    });
                    if (result.Count >= 1)
                    {
                        var path = result[0].TryGetLocalPath() ?? "";
                        if (path != "")
                            vm.SettingsCopy.Export.BaseTlkFile = path;
                    }
                }
            }
        }
    }
}
