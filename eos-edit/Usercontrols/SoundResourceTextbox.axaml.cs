using Avalonia;
using Avalonia.Controls;
using System;

namespace Eos.Usercontrols
{
    public partial class SoundResourceTextbox : UserControl
    {
        public SoundResourceTextbox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<String?> ResRefProperty = AvaloniaProperty.Register<SoundResourceTextbox, String?>("ResRef", null, false, Avalonia.Data.BindingMode.TwoWay);

        public String? ResRef
        {
            get { return GetValue(ResRefProperty); }
            set { SetValue(ResRefProperty, value); }
        }
    }
}
