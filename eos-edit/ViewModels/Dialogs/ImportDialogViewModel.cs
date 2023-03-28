using Eos.Repositories;
using Eos.Services;
using Eos.ViewModels.Base;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels.Dialogs
{
    internal class ImportDialogViewModel : DialogViewModel
    {
        private string _inputFilePath = "";
        private string _tlkFile = "";
        private int _selectedFile = -1;

        public string InputFilePath
        {
            get { return _inputFilePath; }
            set
            {
                if (_inputFilePath != value)
                {
                    _inputFilePath = value;
                    NotifyPropertyChanged();
                }
            }
        }

        
        public string TlkFile
        {
            get { return _tlkFile; }
            set
            {
                if (_tlkFile != value)
                {
                    _tlkFile = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int SelectedFile
        {
            get { return _selectedFile; }
            set
            {
                if (_selectedFile != value)
                {
                    _selectedFile = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int OverrideSettingIndex { get; set; } = 1;
        public int NewDataSettingIndex { get; set; } = 1;
        public bool ExtractOther { get; set; } = true;
        public string ExtractTo { get; set; } = "";

        public ObservableCollection<String> Files { get; private set; } = new ObservableCollection<String>();

        protected override String GetWindowTitle()
        {
            return "Data Import";
        }

        protected override int GetDefaultWidth()
        {
            return 800;
        }

        protected override int GetDefaultHeight()
        {
            return 610;
        }

        protected override bool GetCanResize()
        {
            return false;
        }

        public ReactiveCommand<ImportDialogViewModel, Unit> CloseCommand { get; private set; } = ReactiveCommand.Create<ImportDialogViewModel>(vm =>
        {
            WindowService.Close(vm);
        });

        public ReactiveCommand<ImportDialogViewModel, Unit> AddFileCommand { get; private set; } = ReactiveCommand.Create<ImportDialogViewModel>(vm =>
        {
            if (vm.InputFilePath.Trim() != "")
            {
                vm.Files.Add(vm.InputFilePath);
                vm.InputFilePath = "";
            }
        });

        public ReactiveCommand<ImportDialogViewModel, Unit> DeleteFileCommand { get; private set; } = ReactiveCommand.Create<ImportDialogViewModel>(vm =>
        {
            if ((vm.SelectedFile >= 0) && (vm.SelectedFile < vm.Files.Count))
            {
                var newSelection = vm.SelectedFile;
                if (vm.Files.Count == 1)
                    newSelection = -1;
                else if (vm.SelectedFile >= vm.Files.Count - 1)
                    newSelection = vm.Files.Count - 2;

                vm.Files.RemoveAt(vm.SelectedFile);
                vm.SelectedFile = newSelection;
            }
        });

        public ReactiveCommand<ImportDialogViewModel, Unit> MoveUpCommand { get; private set; } = ReactiveCommand.Create<ImportDialogViewModel>(vm =>
        {
            if (vm.SelectedFile > 0)
            {
                var newSelection = vm.SelectedFile - 1;
                vm.Files.Move(vm.SelectedFile, newSelection);
                vm.SelectedFile = newSelection;
            }
        });

        public ReactiveCommand<ImportDialogViewModel, Unit> MoveDownCommand { get; private set; } = ReactiveCommand.Create<ImportDialogViewModel>(vm =>
        {
            if (vm.SelectedFile < vm.Files.Count - 1)
            {
                var newSelection = vm.SelectedFile + 1;
                vm.Files.Move(vm.SelectedFile, newSelection);
                vm.SelectedFile = newSelection;
            }
        });

        public ReactiveCommand<ImportDialogViewModel, Unit> DoImportCommand { get; private set; } = ReactiveCommand.Create<ImportDialogViewModel>(vm =>
        {
            if (vm.Files.Count == 0)
            { 
                vm.DoError("You have to select at least one file to import!");
                return;
            }

            foreach (var file in vm.Files)
            {
                if (!File.Exists(file))
                {
                    vm.DoError($"The file \"{file}\" does not exist!\nPlease remove and correct the file path!");
                    return;
                }
            }

            if (vm.TlkFile.Trim() == "")
            {
                vm.DoError("You have to specify a .tlk file!");
                return;
            }

            if (!File.Exists(vm.TlkFile))
            {
                vm.DoError("The path for the .tlk file is invalid!");
                return;
            }

            if ((vm.ExtractOther) && (vm.ExtractTo.Trim() == ""))
            {
                vm.DoError("You have to chose an external path where other files will be extracted to!");
                return;
            }

            MasterRepository.Project.CreateBackup();

            var importService = new ImportService(vm.Files, vm.TlkFile, vm.OverrideSettingIndex > 0, vm.OverrideSettingIndex == 2, vm.NewDataSettingIndex > 0, vm.ExtractOther ? vm.ExtractTo : "");
            importService.DoImport();

            WindowService.ShowMessage("Data import successful!", "Import successful!", MessageBoxButtons.Ok, MessageBoxIcon.Information);
            WindowService.Close(vm);
        });
    }
}
