using Eos.Nwn.Tlk;
using Eos.Repositories;
using Eos.Services;
using Ookii.Dialogs.Wpf;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Eos.ViewModels.Dialogs
{
    internal class ProjectOptionsViewModel : DialogViewModel
    {
        private String _externalPathToAdd = "";

        public string ProjectName { get; set; } = MasterRepository.Project.Name;
        public TLKLanguage DefaultLanguage { get; set; } = MasterRepository.Project.DefaultLanguage;

        public ProjectSettings SettingsCopy { get; } = MasterRepository.Project.Settings.Clone();

        public String ExternalPathToAdd
        {
            get { return _externalPathToAdd; }
            set
            {
                _externalPathToAdd = value;
                NotifyPropertyChanged();
            }
        }

        public int SelectedExternalPathIndex { get; set; }

        public ProjectOptionsViewModel()
        {
            DeleteExternalPathCommand = new DelegateCommand<String>(DeleteExternalPath);
        }

        protected override String GetWindowTitle()
        {
            return "Project Settings";
        }

        protected override int GetDefaultWidth()
        {
            return 800;
        }

        protected override int GetDefaultHeight()
        {
            return 570;
        }

        protected override bool GetCanResize()
        {
            return false;
        }

        private void DeleteExternalPath(String path)
        {
            SettingsCopy.ExternalFolders.Remove(path);
        }

        public DelegateCommand<ProjectOptionsViewModel> AddExternalPathCommand { get; private set; } = new DelegateCommand<ProjectOptionsViewModel>(vm =>
        {
            if (vm.ExternalPathToAdd.Trim() != "")
            {
                if (!vm.ExternalPathToAdd.EndsWith(Path.DirectorySeparatorChar)) vm.ExternalPathToAdd += Path.DirectorySeparatorChar;
                vm.SettingsCopy.ExternalFolders.Add(vm.ExternalPathToAdd);
                vm.ExternalPathToAdd = "";
            }
        });

        public DelegateCommand<String> DeleteExternalPathCommand { get; private set; }

        public DelegateCommand<ProjectOptionsViewModel> CloseCommand { get; private set; } = new DelegateCommand<ProjectOptionsViewModel>(vm =>
        {
            WindowService.Close(vm);
        });

        public DelegateCommand<ProjectOptionsViewModel> OKCommand { get; private set; } = new DelegateCommand<ProjectOptionsViewModel>(vm =>
        {
            MasterRepository.Project.OverrideSettings(vm.SettingsCopy);
            WindowService.Close(vm);
        });
    }
}
