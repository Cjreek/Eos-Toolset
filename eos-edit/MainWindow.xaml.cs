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
using Eos.ViewModels.Dialogs;
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
using System.Windows.Controls.Primitives;
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
        private ControlTemplate tabMultilineTemplate;
        private ControlTemplate tabSinglelineTemplate;

        public MainWindow()
        {
            InitializeComponent();
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            viewModel.OnQuery += ViewModel_OnQuery;

            tabMultilineTemplate = tcTabs.Template;
            tabSinglelineTemplate = (ControlTemplate)tcTabs.FindResource("tabMultiLineTemplate");
            if (EosConfig.TabLayout == TabLayout.Multiline)
                tcTabs.Template = tabMultilineTemplate;
            else
                tcTabs.Template = tabSinglelineTemplate;
        }

        private void ViewModel_OnQuery(ViewModelBase viewModel, ViewModelQueryEventArgs args)
        {
            var image = MessageBoxImage.Question;
            switch(args.QueryType)
            {
                case ViewModelQueryType.Information:
                    image = MessageBoxImage.Information;
                    break;
                case ViewModelQueryType.Question:
                    image = MessageBoxImage.Question;
                    break;
                case ViewModelQueryType.Warning:
                    image = MessageBoxImage.Warning;
                    break;
                case ViewModelQueryType.Error:
                    image = MessageBoxImage.Error;
                    break;
            }

            var result = MessageBox.Show(this, args.Message, args.Title, MessageBoxButton.YesNoCancel, image, MessageBoxResult.Cancel);
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
            if (MessageBox.Show("Importing base game data will take a while.\nThe current active project will be saved if you continue!\n\nDo you really want to import the base game data?", "Game Data Import", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    if (MasterRepository.Project.IsLoaded)
                        MessageDispatcher.Send(MessageType.SaveProject, null);

                    var import = new GameDataImport();
                    import.Import(EosConfig.NwnBasePath);
                    
                    if (MasterRepository.Project.IsLoaded)
                        MessageDispatcher.Send(MessageType.OpenProject, EosConfig.LastProject);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }

                MessageBox.Show("Game files have been imported successfully!", "Game Data Import", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Save changes before exit?", "Save changes?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                MessageDispatcher.Send(MessageType.SaveProject, null);
            else if (result == MessageBoxResult.Cancel)
                e.Cancel = true;
        }

        private void miSearchClick(object sender, RoutedEventArgs e)
        {
            var viewModel = new GlobalSearchViewModel();
            WindowService.OpenDialog(viewModel);
            if (viewModel.ResultModel != null)
                MessageDispatcher.Send(MessageType.OpenDetail, viewModel.ResultModel);
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToHorizontalOffset(scv.HorizontalOffset - (e.Delta / 5));
            e.Handled = true;
        }

        private void btTabScrollRight_Click(object sender, RoutedEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)((RepeatButton)sender).Tag;
            scv.ScrollToHorizontalOffset(Math.Min(scv.ScrollableWidth, scv.HorizontalOffset + 10));
        }

        private void btTabScrollLeft_Click(object sender, RoutedEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)((RepeatButton)sender).Tag;
            scv.ScrollToHorizontalOffset(Math.Max(0, scv.HorizontalOffset - 10));
        }

        private void btChangeTabLayout_Click(object sender, RoutedEventArgs e)
        {
            if (EosConfig.TabLayout == TabLayout.Multiline)
            {
                tcTabs.Template = tabSinglelineTemplate;
                EosConfig.TabLayout = TabLayout.Simple;
            }
            else
            {
                tcTabs.Template = tabMultilineTemplate;
                EosConfig.TabLayout = TabLayout.Multiline;
            }
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ((ContextMenu)sender).IsOpen = false;
            e.Handled = true; 
        }

        private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var tvi = e.Source is TreeViewItem ? (TreeViewItem)e.Source : null;
            if ((tvi == null) && (e.Source is DependencyObject))
            {
                var depObj = (DependencyObject)e.Source;
                do
                {
                    if (depObj is Visual)
                        depObj = VisualTreeHelper.GetParent(depObj);
                    else if (depObj is FrameworkContentElement contentElement)
                        depObj = contentElement.Parent;
                    else
                        depObj = null;
                }
                while ((!(depObj is TreeViewItem)) && (depObj != null));


                if (depObj is TreeViewItem parentTvi)
                    tvi = parentTvi;
            }
            
            if (tvi != null)
            {
                if ((String)tvi.DisplayMemberPath == "NOCONTEXT")
                    e.Handled = true;
            }
        }
    }
}
