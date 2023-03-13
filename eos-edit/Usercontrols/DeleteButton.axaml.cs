using Avalonia;
using Avalonia.Controls;
using System.Windows.Input;

namespace Eos.Usercontrols
{
    public partial class DeleteButton : UserControl
    {
        public DeleteButton()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<ICommand?> CommandProperty = AvaloniaProperty.Register<DeleteButton, ICommand?>("Command", null);
        public static readonly StyledProperty<object?> CommandParameterProperty = AvaloniaProperty.Register<DeleteButton, object?>("CommandParameter", null);

        public ICommand? Command
        {
            get { return GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object? CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
    }
}
