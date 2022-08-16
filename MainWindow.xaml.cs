using Eos.Import;
using Eos.Repositories;
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

namespace Eos
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MasterRepository.Initialize(@"D:\Steam\steamapps\common\Neverwinter Nights");
            MasterRepository.Load();

            //var import = new GameDataImport();
            //import.Import(@"D:\Steam\steamapps\common\Neverwinter Nights");
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
    }
}
