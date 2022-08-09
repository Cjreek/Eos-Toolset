using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Eos.Views
{
    public class LanguageAwarePage : Page
    {
        public static readonly DependencyProperty TLKLanguageProperty = DependencyProperty.Register("TLKLanguage", typeof(TLKLanguage), typeof(LanguageAwarePage));
        public static readonly DependencyProperty GenderProperty = DependencyProperty.Register("Gender", typeof(bool), typeof(LanguageAwarePage));

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
    }
}
