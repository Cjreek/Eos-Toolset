using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Eos.Services;
using Eos.ViewModels.Base;
using Eos.ViewModels.Dialogs;
using System.Linq;
using System.Threading.Tasks;

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

        private void btSelectFile_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is ImportDialogViewModel vm)
            {
                if ((Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime app) && (app.MainWindow != null))
                {
                    var dlg = new OpenFileDialog();
                    dlg.AllowMultiple = false;
                    dlg.Filters?.Add(new FileDialogFilter() { Name = "Importable Files (*.hak, *.erf, *.2da)", Extensions = { "hak", "erf", "2da" } });
                    dlg.Filters?.Add(new FileDialogFilter() { Name = "Hak File (*.hak)", Extensions = { "hak" } });
                    dlg.Filters?.Add(new FileDialogFilter() { Name = "Encapsulated Resource File (*.erf)", Extensions = { "erf" } });
                    dlg.Filters?.Add(new FileDialogFilter() { Name = "2D Array File (*.2da)", Extensions = { "2da" } });
                    dlg.ShowAsync(app.MainWindow).ContinueWith(t =>
                    {
                        if ((t.Result != null) && (t.Result.Any()))
                        {
                            vm.InputFilePath = t.Result.First();
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }

        private void btSelectTlkFile_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is ImportDialogViewModel vm)
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
                            vm.TlkFile = t.Result.First();
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }
    }
}
