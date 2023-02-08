using Eos.Config;
using Eos.Models;
using Eos.Models.Tables;
using Eos.Nwn.Erf;
using Eos.Nwn.Tlk;
using Eos.Nwn.TwoDimensionalArray;
using Eos.Repositories;
using Eos.Services;
using Eos.ViewModels;
using Eos.ViewModels.Base;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
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

namespace Eos
{
    class CustomObjectRepositoryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CustomObject template)
                return MasterRepository.Project.CustomObjectRepositories[template];

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class ItemCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class IsViewModelValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is ViewModelBase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            viewModel.OnQuery += ViewModel_OnQuery;

            //var import = new GameDataImport();
            //import.Import(EosConfig.NwnBasePath);
        }

        private void ViewModel_OnQuery(ViewModelBase viewModel, ViewModelQueryEventArgs args)
        {
            var result = MessageBox.Show(this, args.Message, args.Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
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

        private TreeViewItem? GetDataContainer(TreeViewItem item, object? data)
        {
            var container = (TreeViewItem?)item.ItemContainerGenerator.ContainerFromItem(data);
            if (container == null)
            {
                foreach (var subItem in item.ItemContainerGenerator.Items)
                {
                    if (subItem is TreeViewItem tvi)
                    {
                        container = GetDataContainer(tvi, data);
                        if (container != null)
                            break;
                    }
                }
            }

            return container;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == nameof(viewModel.CurrentView)) && (viewModel.CurrentView?.GetDataObject() is BaseModel model))
            {
                if (model is CustomObjectInstance customObjectInstance)
                {
                    foreach (var item in tvAdditional.ItemContainerGenerator.Items)
                    {
                        if (item is TreeViewItem tvi)
                        {
                            var container = GetDataContainer(tvi, model);
                            if (container != null)
                            {
                                container.BringIntoView(new Rect(0, -50, 100, 100));
                                container.IsSelected = true;
                                container.Focus();
                                break;
                            }
                        }
                    }

                    if (!tabAdditional.IsSelected)
                        tabAdditional.IsSelected = true;
                }
                else if (!model.IsReadonly)
                {
                    foreach (var item in tvCustom.ItemContainerGenerator.Items)
                    {
                        if (item is TreeViewItem tvi)
                        {
                            var container = GetDataContainer(tvi, model);
                            if (container != null)
                            {
                                container.BringIntoView(new Rect(0, -50, 100, 100));
                                container.IsSelected = true;
                                container.Focus();
                                break;
                            }
                        }
                    }

                    if (!tabCustom.IsSelected)
                        tabCustom.IsSelected = true;
                }
                else
                {
                    foreach (var item in tvStandard.ItemContainerGenerator.Items)
                    {
                        if (item is TreeViewItem tvi)
                        {
                            var container = GetDataContainer(tvi, model);
                            if (container != null)
                            {
                                container.BringIntoView(new Rect(0, -50, 100, 100));
                                container.IsSelected = true;
                                container.Focus();
                                break;
                            }
                        }
                    }

                    if (!tabStandard.IsSelected)
                        tabStandard.IsSelected = true;
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            MasterRepository.Cleanup();
        }

        private void SetFrameContentDataContext(Frame frame)
        {
            var content = frame.Content as FrameworkElement;
            if (content != null)
                content.DataContext = frame.DataContext;
        }

        private void Frame_LoadCompleted(object sender, NavigationEventArgs e)
        {
            SetFrameContentDataContext((Frame)sender);
        }

        private void Frame_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetFrameContentDataContext((Frame)sender);
        }

        private void miOpenProject_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new VistaOpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.Filter = "Eos Project File (*.eosproj)|*.eosproj";
            if (dlg.ShowDialog() ?? false)
                MessageDispatcher.Send(MessageType.OpenProject, dlg.FileName);
        }

        private void miExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void miSaveProject_Click(object sender, RoutedEventArgs e)
        {
            MessageDispatcher.Send(MessageType.SaveProject, null);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if ((e.Source is MenuItem menuItem) && (menuItem.Parent is ContextMenu menu) && (menu.PlacementTarget is TreeViewItem tvi))
                tvi.IsExpanded = true;
        }

        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if ((EosConfig.LastProject != "") && (File.Exists(EosConfig.LastProject)))
                MessageDispatcher.Send(MessageType.OpenProject, EosConfig.LastProject);
        }

        private void miExportProject_Click(object sender, RoutedEventArgs e)
        {
            MessageDispatcher.Send(MessageType.SaveProject, null);

            var export = new CustomDataExport();
            export.Export(MasterRepository.Project);
            MessageBox.Show("Export successful!", "Export successful", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void miImportBaseData_Click(object sender, RoutedEventArgs e)
        {
            var import = new GameDataImport();
            import.Import(EosConfig.NwnBasePath);
            MessageBox.Show("Game files have been imported successfully!", "Game Data Import", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Save changes before exit?", "Save changes?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                MessageDispatcher.Send(MessageType.SaveProject, null);
            else if (result == MessageBoxResult.Cancel)
                e.Cancel = true;
        }
    }
}
