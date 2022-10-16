using Eos.Models.Tables;
using Eos.Types;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
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
    class AlignmentValidToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Alignment alignment)
                return Alignments.IsValid(alignment) ? Visibility.Hidden : Visibility.Visible;

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class AlignmentToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Alignment alignment && parameter is Alignment alignmentToCheck)
                return alignment.HasFlag(alignmentToCheck);

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaktionslogik für AlignmentMatrix.xaml
    /// </summary>
    public partial class AlignmentMatrix : UserControl
    {
        public AlignmentMatrix()
        {
            InitializeComponent();
            ToggleAlignmentCommand = new DelegateCommand<Alignment?>(ToggleAlignment);
        }

        public static readonly DependencyProperty AlignmentProperty = DependencyProperty.Register("Alignment", typeof(Alignment), typeof(AlignmentMatrix), new FrameworkPropertyMetadata(Alignments.All, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public Alignment Alignment
        {
            get { return (Alignment)GetValue(AlignmentProperty); }
            set { SetValue(AlignmentProperty, value); }
        }

        private void ToggleAlignment(Alignment? alignment)
        {
            if (alignment != null)
                Alignment = Alignment ^ (alignment ?? (Alignment)0);
        }

        public DelegateCommand<Alignment?> ToggleAlignmentCommand { get; private set; }
    }
}