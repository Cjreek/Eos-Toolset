using Eos.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels.Dialogs
{
    public enum MessageBoxIcon
    {
        Information, Question, Warning, Error
    }

    public enum MessageBoxButtons
    {
        Ok, OkCancel, YesNo, YesNoCancel
    }

    public enum MessageBoxResult
    {
        Cancel, Ok, Yes, No
    }

    public class MessageBoxViewModel : DialogViewModel
    {
        public string MessageTitle { get; }
        public string Message { get; }
        public MessageBoxIcon Icon { get; }
        public MessageBoxButtons Buttons { get; }

        public MessageBoxResult Result { get; private set; } = MessageBoxResult.Cancel;

        public MessageBoxViewModel(string title, string message, MessageBoxIcon icon, MessageBoxButtons buttons) : base()
        {
            MessageTitle = title;
            Message = message;
            Icon = icon;
            Buttons = buttons;
        }

        protected override String GetWindowTitle()
        {
            return MessageTitle;
        }

        protected override int GetDefaultWidth()
        {
            return 400;
        }

        protected override int GetDefaultHeight()
        {
            return 100;
        }

        protected override bool GetCanResize()
        {
            return false;
        }
        protected override bool GetAutosize()
        {
            return true;
        }

        public ReactiveCommand<MessageBoxViewModel, Unit> CancelCommand { get; private set; } = ReactiveCommand.Create<MessageBoxViewModel>(vm =>
        {
            vm.Result = MessageBoxResult.Cancel;
            WindowService.Close(vm);
        });

        public ReactiveCommand<MessageBoxViewModel, Unit> OkCommand { get; private set; } = ReactiveCommand.Create<MessageBoxViewModel>(vm =>
        {
            vm.Result = MessageBoxResult.Ok;
            WindowService.Close(vm);
        });

        public ReactiveCommand<MessageBoxViewModel, Unit> YesCommand { get; private set; } = ReactiveCommand.Create<MessageBoxViewModel>(vm =>
        {
            vm.Result = MessageBoxResult.Yes;
            WindowService.Close(vm);
        });

        public ReactiveCommand<MessageBoxViewModel, Unit> NoCommand { get; private set; } = ReactiveCommand.Create<MessageBoxViewModel>(vm =>
        {
            vm.Result = MessageBoxResult.No;
            WindowService.Close(vm);
        });
    }
}
