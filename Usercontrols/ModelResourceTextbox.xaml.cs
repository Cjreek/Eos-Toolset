﻿using System;
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
    /// Interaktionslogik für ModelResourceTextbox.xaml
    /// </summary>
    public partial class ModelResourceTextbox : UserControl
    {
        public ModelResourceTextbox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ResRefProperty = DependencyProperty.Register("ResRef", typeof(String), typeof(ModelResourceTextbox), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public String? ResRef
        {
            get { return (String?)GetValue(ResRefProperty); }
            set { SetValue(ResRefProperty, value); }
        }
    }
}
