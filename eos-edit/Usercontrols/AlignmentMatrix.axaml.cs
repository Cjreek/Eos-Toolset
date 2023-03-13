using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Eos.Types;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reactive;

namespace Eos.Usercontrols
{
    class AlignmentValidToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Alignment alignment)
                return Alignments.IsValid(alignment) ? false : true;

            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class AlignmentToBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Alignment alignment && parameter is Alignment alignmentToCheck)
                return alignment.HasFlag(alignmentToCheck);

            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AlignmentButtonColorConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if ((values.Count == 2) && (values[0] is bool enabled) && (values[1] is bool check))
            {
                if (!enabled)
                    return check ? Brushes.LightGray : Brushes.DarkGray;
                else
                    return check ? Brushes.LightGreen : Brushes.LightCoral;
            }

            return Brushes.White;
        }
    }

    public partial class AlignmentMatrix : UserControl
    {
        public AlignmentMatrix()
        {
            InitializeComponent();
            ToggleAlignmentCommand = ReactiveCommand.Create<Alignment?>(ToggleAlignment);
        }

        public static readonly StyledProperty<Alignment> AlignmentProperty = AvaloniaProperty.Register<AlignmentMatrix, Alignment>("Alignment", Alignments.All, false, Avalonia.Data.BindingMode.TwoWay);

        public Alignment Alignment
        {
            get { return GetValue(AlignmentProperty); }
            set { SetValue(AlignmentProperty, value); }
        }

        private void ToggleAlignment(Alignment? alignment)
        {
            if (alignment != null)
                Alignment = Alignment ^ (alignment ?? (Alignment)0);
        }

        public ReactiveCommand<Alignment?, Unit> ToggleAlignmentCommand { get; private set; }
    }
}
