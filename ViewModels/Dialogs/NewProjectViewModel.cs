using Eos.Nwn.Tlk;
using Eos.Repositories;
using Eos.Services;
using Eos.ViewModels.Base;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels.Dialogs
{
    public class NewProjectViewModel : DialogViewModel
    {
        private string _projectFolder = "";

        public string ProjectName { get; set; } = "New Project";
        public string ProjectFolder
        {
            get { return _projectFolder; }
            set
            {
                if (value != _projectFolder)
                {
                    _projectFolder = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public TLKLanguage DefaultLanguage { get; set; }

        protected override String GetWindowTitle()
        {
            return "New Project";
        }

        public DelegateCommand<NewProjectViewModel> CloseCommand { get; private set; } = new DelegateCommand<NewProjectViewModel>(vm =>
        {
            WindowService.Close(vm);
        });

        public DelegateCommand<NewProjectViewModel> OKCommand { get; private set; } = new DelegateCommand<NewProjectViewModel>(vm =>
        {
            if (vm.ProjectName.Trim() == "")
            {
                vm.DoError("Project name can't be empty!");
                return;
            }

            if (!Directory.Exists(vm.ProjectFolder.Trim()))
            {
                vm.DoError("Project folder is invalid!");
                return;
            }

            MasterRepository.Project.CreateNew(vm.ProjectFolder, vm.ProjectName, vm.DefaultLanguage);
            MessageDispatcher.Send(MessageType.ChangeLanguage, vm.DefaultLanguage);
            WindowService.Close(vm);
        });

        protected override int GetDefaultWidth()
        {
            return 600;
        }

        protected override int GetDefaultHeight()
        {
            return 150;
        }
    }
}
