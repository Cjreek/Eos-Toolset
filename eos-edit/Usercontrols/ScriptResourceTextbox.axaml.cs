using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Eos.Repositories;
using System;

namespace Eos.Usercontrols
{
    public partial class ScriptResourceTextbox : UserControl
    {
        public ScriptResourceTextbox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<String?> ResRefProperty = AvaloniaProperty.Register<ScriptResourceTextbox, String?>("ResRef", null, false, Avalonia.Data.BindingMode.TwoWay);

        public String? ResRef
        {
            get { return GetValue(ResRefProperty); }
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
