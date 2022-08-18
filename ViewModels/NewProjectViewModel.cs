using Eos.Nwn.Tlk;
using Eos.Repositories;
using Eos.Services;
using Eos.ViewModels.Base;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    public class NewProjectViewModel : ViewModelBase
    {
        private String _projectFolder = "";

        public string Title { get; set; } = "New Project";
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

        public DelegateCommand<NewProjectViewModel> CloseCommand { get; private set; } = new DelegateCommand<NewProjectViewModel>(vm =>
        {
            WindowService.Close(vm);
        });

        public DelegateCommand<NewProjectViewModel> OKCommand { get; private set; } = new DelegateCommand<NewProjectViewModel>(vm =>
        {
            MasterRepository.Project.CreateNew(vm.ProjectFolder, vm.ProjectName, vm.DefaultLanguage);
            MessageDispatcher.Send(MessageType.ChangeLanguage, vm.DefaultLanguage);
            WindowService.Close(vm);
        });
    }
}
