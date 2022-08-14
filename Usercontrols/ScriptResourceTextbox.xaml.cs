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

namespace Eos.Usercontrols
{
    /// <summary>
    /// Interaktionslogik für ScriptResourceTextbox.xaml
    /// </summary>
    public partial class ScriptResourceTextbox : UserControl
    {
        public ScriptResourceTextbox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ResRefProperty = DependencyProperty.Register("ResRef", typeof(String), typeof(ScriptResourceTextbox), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public String? ResRef
        {
            get { return (String?)GetValue(ResRefProperty); }
            set { SetValue(ResRefProperty, value); }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var scriptRes = MasterRepository.Resources.Get<String>(ResRef, Nwn.NWNResourceType.NSS);
            var tempPath = System.IO.Path.GetTempPath() + System.IO.Path.DirectorySeparatorChar + ResRef + ".nss";
            System.IO.File.WriteAllText(tempPath, scriptRes);
        }
    }
}
