using Eos.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Eos.Usercontrols
{
    internal class TLKConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is FrameworkElement param)
            {
                var textBox = (TLKTextbox)param.Tag;
                if (textBox.TLKStrings != null)
                {
                    if (textBox.Gender)
                        return textBox.TLKStrings[textBox.TLKLanguage].TextF;
                    return textBox.TLKStrings[textBox.TLKLanguage].Text;
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is FrameworkElement param)
            {
                var textBox = (TLKTextbox)param.Tag;
                if (textBox.TLKStrings != null)
                {
                    if (textBox.Gender)
                        textBox.TLKStrings[textBox.TLKLanguage].TextF = (string)value;
                    else
                        textBox.TLKStrings[textBox.TLKLanguage].Text = (string)value;

                    return textBox.ChangedTrigger;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Interaktionslogik für TLKTextbox.xaml
    /// </summary>
    public partial class TLKTextbox : UserControl
    {
        public TLKTextbox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ChangedTriggerProperty = DependencyProperty.Register("ChangedTrigger", typeof(bool), typeof(TLKTextbox));

        public static readonly DependencyProperty AcceptsReturnProperty = DependencyProperty.Register("AcceptsReturn", typeof(bool), typeof(TLKTextbox));

        public static readonly DependencyProperty TLKStringsProperty = DependencyProperty.Register("TLKStrings", typeof(TLKStringSet), typeof(TLKTextbox), new PropertyMetadata(null, UpdateTLK));
        public static readonly DependencyProperty TLKLanguageProperty = DependencyProperty.Register("TLKLanguage", typeof(TLKLanguage), typeof(TLKTextbox), new PropertyMetadata(TLKLanguage.English, UpdateTLK));
        public static readonly DependencyProperty GenderProperty = DependencyProperty.Register("Gender", typeof(bool), typeof(TLKTextbox), new PropertyMetadata(false, UpdateTLK));

        public TLKStringSet? TLKStrings
        {
            get { return (TLKStringSet)GetValue(TLKStringsProperty); }
            set { SetValue(TLKStringsProperty, value); }
        }

        public TLKLanguage TLKLanguage
        {
            get { return (TLKLanguage)GetValue(TLKLanguageProperty); }
            set { SetValue(TLKLanguageProperty, value); }
        }

        public bool Gender
        {
            get { return (bool)GetValue(GenderProperty); }
            set { SetValue(GenderProperty, value); }
        }

        public bool ChangedTrigger
        {
            get { return (bool)GetValue(ChangedTriggerProperty); }
            set { SetValue(ChangedTriggerProperty, value); }
        }

        public bool AcceptsReturn
        {
            get { return (bool)GetValue(AcceptsReturnProperty); }
            set { SetValue(AcceptsReturnProperty, value); }
        }

        public static void UpdateTLK(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TLKTextbox textbox)
                textbox.ChangedTrigger = !textbox.ChangedTrigger;
        }
    }
}
