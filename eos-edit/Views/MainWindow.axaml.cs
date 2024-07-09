using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Eos.Config;
using Eos.Repositories;
using Eos.Services;
using Eos.ViewModels.Base;
using System.Collections.Generic;
using System.Globalization;
using System;
using System.IO;
using System.Linq;
using Eos.ViewModels;
using Avalonia;
using Eos.Models;
using System.ComponentModel;
using Avalonia.Controls.ApplicationLifetimes;
using System.Threading.Tasks;
using Avalonia.Threading;
using Eos.ViewModels.Dialogs;
using Eos.Models.Tables;
using Eos.Views.Dialogs;
using System.Threading;
using Eos.Nwn.Mdl;

namespace Eos.Views
{
    public class CustomObjectRepositoryConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is CustomObject template)
                return MasterRepository.Project.CustomObjectRepositories[template];

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ItemCountConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is IEnumerable<object> list)
            {
                var count = list.Count();
                if (count > 0)
                    return String.Format("({0})", count);
                return String.Empty;
            }

            return "Invalid List";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContextChanged += MainWindow_DataContextChanged;

            //tabMultilineTemplate = tcTabs.Template;
            //tabSinglelineTemplate = (ControlTemplate)tcTabs.FindResource("tabMultiLineTemplate");
            //if (EosConfig.TabLayout == TabLayout.Multiline)
            //    tcTabs.Template = tabMultilineTemplate;
            //else
            //    tcTabs.Template = tabSinglelineTemplate;
        }

        private void MainWindow_DataContextChanged(object? sender, EventArgs e)
        {
            var viewModel = (MainWindowViewModel?)DataContext;
            if (viewModel != null)
            {
                viewModel.OnQuery += ViewModel_OnQuery;
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_OnQuery(ViewModelBase viewModel, ViewModelQueryEventArgs args)
        {
            var image = MessageBoxIcon.Question;
            switch (args.QueryType)
            {
                case ViewModelQueryType.Information:
                    image = MessageBoxIcon.Information;
                    break;
                case ViewModelQueryType.Question:
                    image = MessageBoxIcon.Question;
                    break;
                case ViewModelQueryType.Warning:
                    image = MessageBoxIcon.Warning;
                    break;
                case ViewModelQueryType.Error:
                    image = MessageBoxIcon.Error;
                    break;
            }

            var result = WindowService.ShowMessage(args.Message, args.Title, MessageBoxButtons.YesNoCancel, image);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    args.Result = ViewModelQueryResult.Yes;
                    break;
                case MessageBoxResult.No:
                    args.Result = ViewModelQueryResult.No;
                    break;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if ((e.Source is MenuItem menuItem) && (menuItem.Parent is ContextMenu menu) && (menu.Parent?.Parent is TreeViewItem tvi))
                tvi.IsExpanded = true;
        }

        private TreeViewItem? GetDataContainer(TreeViewItem? item, object data)
        {
            TreeViewItem? result = null;
            if (item?.Items != null)
            {
                foreach (var subItem in item.Items)
                {
                    var tmpResult = item.ContainerFromItem(subItem) as TreeViewItem;
                    if (data != subItem)
                    {
                        result = GetDataContainer(tmpResult, data);
                        if (result != null) return result;
                    }
                    else
                        return tmpResult;
                }
            }

            return result;
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var viewModel = (MainWindowViewModel?)DataContext;
            if ((viewModel != null) && (e.PropertyName == nameof(viewModel.CurrentView)) && (viewModel.CurrentView?.GetDataObject() is BaseModel model))
            {
                if ((model is CustomObjectInstance) || (model is CustomDynamicTableInstance) || (model is CustomTableInstance))
                {
                    if (tvAdditional.Items != null)
                    {
                        foreach (var itemData in tvAdditional.Items)
                        {
                            TreeViewItem? container = tvCustom.TreeContainerFromItem(itemData) as TreeViewItem;
                            if (itemData != model) container = GetDataContainer(container, model);

                            if (container != null)
                            {
                                var tmpItem = container;
                                while (tmpItem.Parent is TreeViewItem tvi)
                                {
                                    tvi.IsExpanded = true;
                                    tmpItem = tvi;
                                }

                                container.BringIntoView();
                                tvAdditional.SelectedItems = null;
                                container.IsSelected = true;
                                container.Focus();
                                break;
                            }
                        }

                        if (!tabAdditional.IsSelected)
                            Dispatcher.UIThread.Post(() => tabAdditional.IsSelected = true);
                    }
                }
                else if (!model.IsReadonly)
                {
                    if (tvCustom.Items != null)
                    {
                        foreach (var itemData in tvCustom.Items)
                        {
                            TreeViewItem? container = tvCustom.TreeContainerFromItem(itemData) as TreeViewItem;
                            if (itemData != model) container = GetDataContainer(container, model);

                            if (container != null)
                            {
                                var tmpItem = container;
                                while (tmpItem.Parent is TreeViewItem tvi)
                                {
                                    tvi.IsExpanded = true;
                                    tmpItem = tvi;
                                }

                                container.BringIntoView();
                                tvCustom.SelectedItems = null;
                                container.IsSelected = true;
                                container.Focus();
                                break;
                            }
                        }

                        if (!tabCustom.IsSelected)
                            Dispatcher.UIThread.Post(() => tabCustom.IsSelected = true);
                    }
                }
                else
                {
                    if (tvStandard.Items != null)
                    {
                        foreach (var itemData in tvStandard.Items)
                        {
                            TreeViewItem? container = tvStandard.TreeContainerFromItem(itemData) as TreeViewItem;
                            if (itemData != model) container = GetDataContainer(container, model);

                            if (container != null)
                            {
                                var tmpItem = container;
                                while (tmpItem.Parent is TreeViewItem tvi)
                                {
                                    tvi.IsExpanded = true;
                                    tmpItem = tvi;
                                }

                                container.BringIntoView();
                                tvStandard.SelectedItems = null;
                                container.IsSelected = true;
                                container.Focus();
                                break;
                            }
                        }

                        if (!tabStandard.IsSelected)
                            Dispatcher.UIThread.Post(() => tabStandard.IsSelected = true);
                    }
                }
            }
        }

        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (EosConfig.NwnBasePath == "")
            {
                var basePathInputViewModel = new InputBasePathDialogViewModel();
                WindowService.OpenDialog<InputBasePathDialogViewModel>(basePathInputViewModel);

                if (basePathInputViewModel.BasePath != "")
                {
                    EosConfig.OverrideNwnBasePath(basePathInputViewModel.BasePath);
                    EosConfig.Save();

                    if (Application.Current is App app)
                        app.InitializeRepositories();
                    else
                        Environment.Exit(0);
                }
                else
                    Environment.Exit(0);
            }

            if (MasterRepository.Standard.Classes.Count == 0)
            {
                WindowService.ShowMessage("No base game data has been found!\nThe standard game files will now be imported.\nThis may take a minute.", "Game Data Import", MessageBoxButtons.Ok, MessageBoxIcon.Information);
                MessageDispatcher.Send(MessageType.DoGameDataImport, false, null);
            }
            
            if ((EosConfig.LastProject != "") && (File.Exists(EosConfig.LastProject)))
                MessageDispatcher.Send(MessageType.OpenProject, EosConfig.LastProject);

            // .................
            //var stream = MasterRepository.Resources.GetRaw("d_div021", Nwn.NWNResourceType.MDL);
            //var mdl = new MdlFile(stream ?? new MemoryStream());
        }

        private void miOpenProject_Click(object sender, RoutedEventArgs e)
        {
            if ((Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime app) && (app.MainWindow != null))
            {
                var dlg = new OpenFileDialog();
                dlg.AllowMultiple = false;
                dlg.Filters?.Add(new FileDialogFilter() { Name = "Eos Project File (*.eosproj)", Extensions = { "eosproj" } });
                dlg.ShowAsync(app.MainWindow).ContinueWith(t =>
                {
                    if ((t.Result != null) && (t.Result.Any()))
                        MessageDispatcher.Send(MessageType.OpenProject, t.Result.First());
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private void miImportBaseData_Click(object sender, RoutedEventArgs e)
        {
            MessageDispatcher.Send(MessageType.DoGameDataImport, true, null);
        }

        private void miExportProject_Click(object sender, RoutedEventArgs e)
        {
            MasterRepository.Resources.LoadExternalResources(MasterRepository.Project.Settings.ExternalFolders);

            MessageDispatcher.Send(MessageType.SaveProject, null);

            var export = new CustomDataExport();
            export.Export(MasterRepository.Project);

            WindowService.ShowMessage("Export successful!", "Export successful", MessageBoxButtons.Ok, MessageBoxIcon.Information);
        }

        private void miBackupProject_Click(object sender, RoutedEventArgs e)
        {
            MessageDispatcher.Send(MessageType.SaveProject, null);
            MasterRepository.Project.CreateBackup();

            WindowService.ShowMessage("Backup successful!", "Backup successful", MessageBoxButtons.Ok, MessageBoxIcon.Information);
        }

        private void miReloadExternalData_Click(object sender, RoutedEventArgs e)
        {
            MasterRepository.Resources.LoadExternalResources(MasterRepository.Project.Settings.ExternalFolders);

            WindowService.ShowMessage("External files reloaded successfully!", "Reload successful", MessageBoxButtons.Ok, MessageBoxIcon.Information);
        }

        private void miExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void mainWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            if (MasterRepository.Project.IsLoaded)
            {
                var result = WindowService.ShowMessage("Save changes before exit?", "Save changes?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == MessageBoxResult.Yes)
                    MessageDispatcher.Send(MessageType.SaveProject, null);
                else if (result == MessageBoxResult.Cancel)
                    e.Cancel = true;
            }
        }
    }
}
