using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Eos.Services;
using Eos.ViewModels.Base;
using Eos.ViewModels.Dialogs;
using Avalonia.Platform.Storage;

namespace Eos.Views.Dialogs
{
    public partial class ImportDialogView : LanguageAwarePage
    {
        public ImportDialogView()
        {
            InitializeComponent();
            DataContextChanged += ImportDialogView_DataContextChanged;
        }

        private void ImportDialogView_DataContextChanged(object? sender, System.EventArgs e)
        {
            if (DataContext is ImportDialogViewModel vm)
                vm.OnError += Vm_OnError;
        }

        private void Vm_OnError(ViewModelBase viewModel, ViewModelErrorEventArgs args)
        {
            WindowService.ShowMessage(args.Message, "Missing Information", MessageBoxButtons.Ok, MessageBoxIcon.Warning);
        }

        private async void btSelectFile_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is ImportDialogViewModel vm)
            {
                if ((Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime app) && (app.MainWindow != null))
                {
                    var result = await app.MainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
                    {
                        Title = "Select File(s) To Import",
                        AllowMultiple = true,
                        FileTypeFilter = new FilePickerFileType[]
                        {
                            new ("Importable Files (*.hak, *.erf, *.2da)") { Patterns = new[] { "*.hak", "*.erf", "*.2da" } },
                            new ("Hak File (*.hak)") { Patterns = new[] { "*.hak" } },
                            new ("Encapsulated Resource File (*.erf)") { Patterns = new[] { "*.erf" } },
                            new ("2D Array File (*.2da)") { Patterns = new[] { "*.2da" } } 
                        }
                    });
                    if (result.Count >= 1)
                    {
                        foreach (var file in result)
                        {
                            var path = file.TryGetLocalPath() ?? "";
                            if (path != "")
                                vm.Files.Add(path);
                        }
                    }
                }
            }
        }

        private async void btSelectTlkFile_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is ImportDialogViewModel vm)
            {
                if ((Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime app) && (app.MainWindow != null))
                {
                    var result = await app.MainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
                    {
                        Title = "Select Talk Table File",
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
                            vm.TlkFile = path;
                    }
                }
            }
        }
    }
}
