using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Eos.ViewModels.Dialogs;
using System;
using System.Globalization;

namespace Eos.Views.Dialogs
{
    public class ButtonToVisibleConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if ((value is MessageBoxButtons buttons) && (parameter is string btn) && (btn != ""))
            {
                switch (buttons)
                {
                    case MessageBoxButtons.Ok: return (btn == "O");
                    case MessageBoxButtons.OkCancel: return (btn == "O") || (btn == "C");
                    case MessageBoxButtons.YesNo: return (btn == "Y") || (btn == "N");
                    case MessageBoxButtons.YesNoCancel: return (btn != "O");
                }
            }

            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IconToIconResourceConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is MessageBoxIcon icon)
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                switch (icon)
                {
                    case MessageBoxIcon.Information: return new Bitmap(AssetLoader.Open(new Uri($"avares://{assembly}/Assets/Icons/info.png")));
                    case MessageBoxIcon.Question: return new Bitmap(AssetLoader.Open(new Uri($"avares://{assembly}/Assets/Icons/question.png")));
                    case MessageBoxIcon.Warning: return new Bitmap(AssetLoader.Open(new Uri($"avares://{assembly}/Assets/Icons/warning.png")));
                    case MessageBoxIcon.Error: return new Bitmap(AssetLoader.Open(new Uri($"avares://{assembly}/Assets/Icons/error.png")));
                }
            }

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MessageBox : UserControl
    {
        public MessageBox()
        {
            InitializeComponent();
        }
    }
}
