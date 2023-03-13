using Avalonia;
using Avalonia.Controls;
using System;

namespace Eos.Usercontrols
{
    public partial class ModelResourceTextbox : UserControl
    {
        public ModelResourceTextbox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<String?> ResRefProperty = AvaloniaProperty.Register<ModelResourceTextbox, String?>("ResRef", null, false, Avalonia.Data.BindingMode.TwoWay);

        public String? ResRef
        {
            get { return GetValue(ResRefProperty); }
            set { SetValue(ResRefProperty, value); }
        }
    }
}
