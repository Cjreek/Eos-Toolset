using Eos.Nwn.Tlk;
using Eos.Repositories;
using Eos.Services;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ReactiveUI;
using System.Reactive;

namespace Eos.ViewModels.Dialogs
{
    public class ProjectOptionsViewModel : DialogViewModel
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
            DeleteExternalPathCommand = ReactiveCommand.Create<String>(DeleteExternalPath);
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
            return 650;
        }

        protected override bool GetCanResize()
        {
            return false;
        }

        private void DeleteExternalPath(String path)
        {
            SettingsCopy.ExternalFolders.Remove(path);
        }

        public ReactiveCommand<ProjectOptionsViewModel, Unit> AddExternalPathCommand { get; private set; } = ReactiveCommand.Create<ProjectOptionsViewModel>(vm =>
        {
            if (vm.ExternalPathToAdd.Trim() != "")
            {
                if (!vm.ExternalPathToAdd.EndsWith(Path.DirectorySeparatorChar)) vm.ExternalPathToAdd += Path.DirectorySeparatorChar;
                vm.SettingsCopy.ExternalFolders.Add(vm.ExternalPathToAdd);
                vm.ExternalPathToAdd = "";
            }
        });

        public ReactiveCommand<String, Unit> DeleteExternalPathCommand { get; private set; }

        public ReactiveCommand<ProjectOptionsViewModel, Unit> CloseCommand { get; private set; } = ReactiveCommand.Create<ProjectOptionsViewModel>(vm =>
        {
            WindowService.Close(vm);
        });

        public ReactiveCommand<ProjectOptionsViewModel, Unit> OKCommand { get; private set; } = ReactiveCommand.Create<ProjectOptionsViewModel>(vm =>
        {
            MasterRepository.Project.OverrideSettings(vm.SettingsCopy);
            WindowService.Close(vm);
        });

        public ReactiveCommand<ProjectSettingsCustomData, Unit> RestoreDefaultExportOffsetCommand { get; private set; } = ReactiveCommand.Create<ProjectSettingsCustomData>(settings =>
        {
            settings.ExportOffset = settings.DefaultExportOffset;
        });
    }
}
