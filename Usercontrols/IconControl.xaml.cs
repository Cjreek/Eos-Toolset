using Eos.Models;
using Eos.Services;
using Eos.ViewModels;
using Eos.ViewModels.Dialogs;
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

namespace Eos.Usercontrols
{
    /// <summary>
    /// Interaktionslogik für IconControl.xaml
    /// </summary>
    public partial class IconControl : UserControl
    {
        public IconControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(String), typeof(IconControl), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public String Icon
        {
            get { return (String)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        private void btLoadIcon_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = new IconSearchViewModel();
            WindowService.OpenDialog(viewModel);
            if (viewModel.ResultResRef != null)
                SetValue(IconProperty, viewModel.ResultResRef);
        }
    }
}
