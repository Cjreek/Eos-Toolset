using Avalonia.Controls;
using Avalonia.Data.Converters;
using System.Globalization;
using System;
using Avalonia.Media;
using Avalonia;
using Eos.Nwn.Tlk;
using Eos.Types;
using Avalonia.Data;
using System.Threading.Tasks;
using System.Threading;
using Avalonia.Styling;
using ReactiveUI;
using Avalonia.VisualTree;
using System.Reflection;

namespace Eos.Usercontrols
{
    public class TLKConverter : AvaloniaObject, IValueConverter
    {
        public static readonly StyledProperty<TLKTextbox?> TLKTextboxProperty = AvaloniaProperty.Register<TLKConverter, TLKTextbox?>("TLKTextbox");

        public TLKTextbox? TLKTextbox
        {
            get => GetValue(TLKTextboxProperty);
            set => SetValue(TLKTextboxProperty, value);
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var tlkTextbox = GetValue(TLKTextboxProperty);
            if (tlkTextbox != null)
            {
                if (tlkTextbox.TLKStrings != null)
                {
                    if (tlkTextbox.Gender)
                        return tlkTextbox.TLKStrings[tlkTextbox.TLKLanguage].TextF;
                    return tlkTextbox.TLKStrings[tlkTextbox.TLKLanguage].Text;
                }
            }

            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var tlkTextbox = GetValue(TLKTextboxProperty);
            if (tlkTextbox != null)
            {
                if (tlkTextbox?.TLKStrings != null)
                {
                    if (tlkTextbox.Gender)
                        tlkTextbox.TLKStrings[tlkTextbox.TLKLanguage].TextF = (string?)value ?? "";
                    else
                        tlkTextbox.TLKStrings[tlkTextbox.TLKLanguage].Text = (string?)value ?? "";

                    return tlkTextbox.ChangedTrigger;
                }
            }

            return false;
        }
    }

    public partial class TLKTextbox : UserControl
    {
        public TLKTextbox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<bool> ChangedTriggerProperty = AvaloniaProperty.Register<TLKTextbox, bool>("ChangedTrigger");

        public static readonly StyledProperty<bool> IsReadonlyProperty = AvaloniaProperty.Register<TLKTextbox, bool>("IsReadonly");
        public static readonly StyledProperty<bool> AcceptsReturnProperty = AvaloniaProperty.Register<TLKTextbox, bool>("AcceptsReturn");
        public static readonly StyledProperty<Thickness> InnerBorderThicknessProperty = AvaloniaProperty.Register<TLKTextbox, Thickness>("InnerBorderThickness", new Thickness(1));
        public static readonly StyledProperty<IBrush> InnerBorderBrushProperty = AvaloniaProperty.Register<TLKTextbox, IBrush>("InnerBorderBrush", new SolidColorBrush(Color.FromArgb(0xFF, 0xAB, 0xAD, 0xB3)));

        public static readonly StyledProperty<TLKStringSet?> TLKStringsProperty = AvaloniaProperty.Register<TLKTextbox, TLKStringSet?>("TLKStrings", null, false, BindingMode.OneWay, null, UpdateTLKStrings);
        public static readonly StyledProperty<TLKLanguage> TLKLanguageProperty = AvaloniaProperty.Register<TLKTextbox, TLKLanguage>("TLKLanguage", TLKLanguage.English, false, BindingMode.OneWay, null, UpdateTLKLang);
        public static readonly StyledProperty<bool> GenderProperty = AvaloniaProperty.Register<TLKTextbox, bool>("Gender", false, false, BindingMode.OneWay, null, UpdateTLKGender);

        public TLKStringSet? TLKStrings
        {
            get { return GetValue(TLKStringsProperty); }
            set { SetValue(TLKStringsProperty, value); }
        }

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

        public bool ChangedTrigger
        {
            get { return GetValue(ChangedTriggerProperty); }
            set { SetValue(ChangedTriggerProperty, value); }
        }

        public bool AcceptsReturn
        {
            get { return GetValue(AcceptsReturnProperty); }
            set { SetValue(AcceptsReturnProperty, value); }
        }

        public bool IsReadonly
        {
            get { return GetValue(IsReadonlyProperty); }
            set { SetValue(IsReadonlyProperty, value); }
        }

        public Thickness InnerBorderThickness
        {
            get { return GetValue(InnerBorderThicknessProperty); }
            set { SetValue(InnerBorderThicknessProperty, value); }
        }

        public IBrush InnerBorderBrush
        {
            get { return GetValue(InnerBorderBrushProperty); }
            set { SetValue(InnerBorderBrushProperty, value); }
        }

        private static void TriggerChange(TLKTextbox textbox)
        {
            Task.Run(() => Task.Delay(1)).ContinueWith(a => { textbox.ChangedTrigger = !textbox.ChangedTrigger; }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        protected override bool IsEnabledCore => true;

        public static TLKStringSet? UpdateTLKStrings(AvaloniaObject d, TLKStringSet? text)
        {
            if (d is TLKTextbox textbox)
                TriggerChange(textbox);

            return text;
        }

        public static TLKLanguage UpdateTLKLang(AvaloniaObject d, TLKLanguage lang)
        {
            if (d is TLKTextbox textbox)
                TriggerChange(textbox);

            return lang;
        }

        public static bool UpdateTLKGender(AvaloniaObject d, bool gender)
        {
            if (d is TLKTextbox textbox)
                TriggerChange(textbox);

            return gender;
        }
    }
}
