using Eos.Nwn.Tlk;
using Eos.Repositories;
using Eos.Services;
using Eos.ViewModels.Base;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels.Dialogs
{
    public class InputBasePathDialogViewModel : DialogViewModel
    {
        private string _basePath = "";

        public string BasePath
        {
            get { return _basePath; }
            set
            {
                if (value != _basePath)
                {
                    _basePath = value;
                    NotifyPropertyChanged();
                }
            }
        }

        protected override String GetWindowTitle()
        {
            return "Select Neverwinter Nights installation directory";
        }

        public ReactiveCommand<InputBasePathDialogViewModel, Unit> CloseCommand { get; private set; } = ReactiveCommand.Create<InputBasePathDialogViewModel>(vm =>
        {
            vm.BasePath = "";
            WindowService.Close(vm);
        });

        public ReactiveCommand<InputBasePathDialogViewModel, Unit> OKCommand { get; private set; } = ReactiveCommand.Create<InputBasePathDialogViewModel>(vm =>
        {
            if (!Directory.Exists(Path.Combine(vm.BasePath.Trim(), "data")))
            {
                vm.DoError("This is not a valid Neverwinter Nights installation!");
                return;
            }

            WindowService.Close(vm);
        });

        protected override int GetDefaultWidth()
        {
            return 600;
        }

        protected override int GetDefaultHeight()
        {
            return 130;
        }
    }
}
