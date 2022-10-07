using Eos.Config;
using Eos.Import;
using Eos.Models;
using Eos.Repositories;
using Eos.ViewModels;
using Eos.ViewModels.Base;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
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

            //var import = new GameDataImport();
            //import.Import(EosConfig.NwnBasePath);
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
                if (!model.IsReadonly)
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
            if (EosConfig.LastProject != "")
                MessageDispatcher.Send(MessageType.OpenProject, EosConfig.LastProject);
        }
    }
}
