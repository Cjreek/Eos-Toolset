using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Eos.Nwn.Tlk;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Eos.Views
{
    public class LanguageAwarePage : UserControl
    {
        public static readonly StyledProperty<TLKLanguage> TLKLanguageProperty = AvaloniaProperty.Register<LanguageAwarePage, TLKLanguage>("TLKLanguage", default, false, Avalonia.Data.BindingMode.TwoWay);
        public static readonly StyledProperty<bool> GenderProperty = AvaloniaProperty.Register<LanguageAwarePage, bool>("Gender", false, false, Avalonia.Data.BindingMode.TwoWay);

        public TLKLanguage TLKLanguage
        {
            get { return GetValue(TLKLanguageProperty); }
            set { SetValue(TLKLanguageProperty, value); }
        }

        public bool Gender
        {
            get { return GetValue(GenderProperty); }
            set { SetValue(GenderProperty, value); }
        }

        private string? FilterCharacters(string? text)
        {
            if (text == null) return null;

            var result = "";
            for (int i = 0; i < text.Length; i++)
            {
                if ((char.IsLetterOrDigit(text[i]) && (char.IsAscii(text[i]))) || (text[i] == '_'))
                    result = result + char.ToUpper(text[i]);
                else if (char.IsWhiteSpace(text[i]))
                    result = result + '_';
            }

            return result;
        }

        protected void ConstantTextbox_TextChanging(object? sender, TextChangingEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.Text = FilterCharacters(tb.Text);
                e.Handled = true;
            }
        }

        protected void ItemsRepeater_ElementPrepared(object? sender, ItemsRepeaterElementPreparedEventArgs e)
        {
            if (e.Element is Grid grid)
            {
                var presenter = grid.Children.FirstOrDefault(ctrl => ctrl is ContentPresenter) as ContentPresenter;
                if (presenter != null)
                    presenter.Content = e.Element.DataContext;
            }
        }
    }
}
