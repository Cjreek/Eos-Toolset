using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media.TextFormatting;
using Eos.Extensions;
using System.Threading.Tasks;

namespace Eos.Usercontrols
{
    public partial class FlagListbox : UserControl
    {
        public FlagListbox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<object?> ItemsSourceProperty = AvaloniaProperty.Register<FlagListbox, object?>("ItemsSource");
        public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<FlagListbox, Orientation>("Orientation", Orientation.Vertical);
        public static readonly StyledProperty<object?> FlagsProperty = AvaloniaProperty.Register<FlagListbox, object?>("Flags", null, false, Avalonia.Data.BindingMode.TwoWay, null, (o, flags) =>
        {
            if ((o is FlagListbox flb) && (flb.ItemsSource is EnumSourceItem[] enumItems))
            {
                Task.Run(() => Task.Delay(1)).ContinueWith(task =>
                {
                    foreach (var item in enumItems)
                        item.RaiseChanged();
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }

            return flags;
        });

        public object? Flags
        {
            get { return GetValue(FlagsProperty); }
            set { SetValue(FlagsProperty, value); }
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public object? ItemsSource
        {
            get { return GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
    }
}
