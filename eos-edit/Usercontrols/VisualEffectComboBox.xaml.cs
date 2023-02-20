﻿using Eos.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaktionslogik für VisualEffectComboBox.xaml
    /// </summary>
    public partial class VisualEffectComboBox : UserControl
    {
        public VisualEffectComboBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register("SelectedValue", typeof(VisualEffect), typeof(VisualEffectComboBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty IsNullableProperty = DependencyProperty.Register("IsNullable", typeof(bool), typeof(VisualEffectComboBox), new PropertyMetadata(true));

        public VisualEffect? SelectedValue
        {
            get { return (VisualEffect)GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        public bool IsNullable
        {
            get { return (bool)GetValue(IsNullableProperty); }
            set { SetValue(IsNullableProperty, value); }
        }

        private void btClear_Click(object sender, RoutedEventArgs e)
        {
            SetValue(SelectedValueProperty, null);
        }
    }
}
