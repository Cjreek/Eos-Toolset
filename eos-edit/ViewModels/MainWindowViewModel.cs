using Eos.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using Eos.ViewModels.Base;
using Eos.Nwn.Tlk;
using Eos.Services;
using System.Globalization;
using Eos.Repositories;
using Eos.Models.Tables;
using Eos.Config;
using Eos.ViewModels.Dialogs;
using Avalonia.Input;

namespace Eos.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<DataDetailViewModelBase> detailViewList = new ObservableCollection<DataDetailViewModelBase>();
        private Dictionary<object, DataDetailViewModelBase> detailViewDict = new Dictionary<object, DataDetailViewModelBase>();

        private DataDetailViewModelBase? currentView;
        private bool inProjectUpdate;

        public ObservableCollection<DataDetailViewModelBase> DetailViewList { get { return detailViewList; } }

        public ModelRepository<Race> Races { get { return MasterRepository.Standard.Races; } }

        public DataDetailViewModelBase? CurrentView
        {
            get { return currentView; }
            set
            {
                if (currentView != value)
                {
                    currentView = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void OpenDetail(BaseModel model, bool jumpToOverride, bool changeView)
        {
            if ((jumpToOverride) && !(model is CustomObjectInstance) && !(model is CustomTableInstance) && !(model is CustomDynamicTableInstance))
                model = MasterRepository.Project.GetOverride(model) ?? model;

            if (!detailViewDict.ContainsKey(model))
            {
                var detailView = ViewModelFactory.CreateViewModel(model);
                detailViewList.Add(detailView);
                detailViewDict.Add(model, detailView);
            }

            if (changeView) CurrentView = detailViewDict[model];
        }

        private void CloseDetail(BaseModel model)
        {
            if (detailViewDict.TryGetValue(model, out DataDetailViewModelBase? viewModel))
            {
                detailViewList.Remove(viewModel);
                detailViewDict.Remove(model);
            }
        }

        private void CloseAllProjectDetails()
        {
            foreach (var key in detailViewDict.Keys)
            {
                if ((key is BaseModel model) && (!model.IsReadonly))
                    CloseDetail(model);
            }
        }

        private void DeleteDetail(BaseModel model)
        {
            if (model.ReferenceCount > 0)
            {
                var message = "There are still " + model.ReferenceCount.ToString() + " references to this " + model.TypeName + ":\n\n";
                foreach (var reference in model.References)
                    message += "> [" + reference.ReferenceObject?.GetType().Name + "] " + reference.ReferenceName + " (" + reference.ReferenceProperty + ")\n";
                message += "\nDo you still want to delete this item and remove all its references?";
                var queryResult = DoQuery("References", message, ViewModelQueryType.Warning);
                if (queryResult != ViewModelQueryResult.Yes)
                    return;
            }
            else
            {
                ViewModelQueryResult queryResult = ViewModelQueryResult.Cancel;
                if (model is CustomObject)
                {
                    var message = "Do you really want to delete the definition of: \"" + model.GetLabel() + "\"?\n";
                    message += "WARNING: This will delete every instance of this object type!\n\n";
                    message += "Do you still want to delete this Custom Object definition?";
                    queryResult = DoQuery("Delete Confirmation", message, ViewModelQueryType.Warning);
                }
                else if (model is CustomTable)
                {
                    var message = "Do you really want to delete the definition of: \"" + model.GetLabel() + "\"?\n";
                    message += "WARNING: This will delete every instance of this table type!\n\n";
                    message += "Do you still want to delete this Custom Table definition?";
                    queryResult = DoQuery("Delete Confirmation", message, ViewModelQueryType.Warning);
                }
                else if (model is CustomDynamicTable)
                {
                    var message = "Do you really want to delete the definition of: \"" + model.GetLabel() + "\"?\n";
                    message += "WARNING: This will delete every instance of this dynamic table type!\n\n";
                    message += "Do you still want to delete this Custom Dynamic Table definition?";
                    queryResult = DoQuery("Delete Confirmation", message, ViewModelQueryType.Warning);
                }
                else
                {
                    var message = "Do you really want to delete this " + model.TypeName + ": \"" + model.GetLabel() + "\"?";
                    queryResult = DoQuery("Delete Confirmation", message, ViewModelQueryType.Question);
                }
                
                if (queryResult != ViewModelQueryResult.Yes)
                    return;
            }

            CloseDetail(model);
            if (model is CustomObject customObject)
            {
                foreach (var custObjInstance in MasterRepository.Project.CustomObjectRepositories[customObject])
                {
                    if (custObjInstance != null)
                        CloseDetail(custObjInstance);
                }
            }
            else if (model is CustomTable customTable)
            {
                foreach (var custTableInstance in MasterRepository.Project.CustomTableRepositories[customTable])
                {
                    if (custTableInstance != null)
                        CloseDetail(custTableInstance);
                }
            }
            else if (model is CustomDynamicTable customDynTable)
            {
                foreach (var custDynTableInstance in MasterRepository.Project.CustomDynamicTableRepositories[customDynTable])
                {
                    if (custDynTableInstance != null)
                        CloseDetail(custDynTableInstance);
                }
            }
            MasterRepository.Project.Delete(model);
        }

        private void DoGameDataImport(bool showConfirmationMessage)
        {
            if ((!showConfirmationMessage) || (WindowService.ShowMessage("Importing base game data will take a while.\nThe current active project will be saved if you continue!\n\nDo you really want to import the base game data?", "Game Data Import", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == MessageBoxResult.Yes))
            {
                WindowService.BeginWaitCursor();
                try
                {
                    if (MasterRepository.Project.IsLoaded)
                        MessageDispatcher.Send(MessageType.SaveProject, null);

                    var import = new GameDataImport();
                    import.Import(EosConfig.NwnBasePath);

                    if (MasterRepository.Project.IsLoaded)
                        MessageDispatcher.Send(MessageType.OpenProject, EosConfig.LastProject);
                }
                finally
                {
                    WindowService.EndWaitCursor();
                }

                WindowService.ShowMessage("Game files have been imported successfully!", "Game Data Import", MessageBoxButtons.Ok, MessageBoxIcon.Information);
            }
        }

        private void MessageHandler(MessageType type, object? message, object? param)
        {
            if (message is BaseModel model)
            {
                switch (type)
                {
                    case MessageType.NewCustomDetail:
                        if (model is CustomObject template)
                        {
                            var instance = new CustomObjectInstance();
                            instance.Template = template;
                            MasterRepository.Add(instance);
                            MessageDispatcher.Send(MessageType.OpenDetail, instance);
                        }
                        else if (model is CustomTable tableTemplate)
                        {
                            var instance = new CustomTableInstance();
                            instance.Template = tableTemplate;
                            MasterRepository.Add(instance);
                            MessageDispatcher.Send(MessageType.OpenDetail, instance);
                        }
                        else if (model is CustomDynamicTable dynTableTemplate)
                        {
                            var instance = new CustomDynamicTableInstance();
                            instance.Template = dynTableTemplate;
                            MasterRepository.Add(instance);
                            MessageDispatcher.Send(MessageType.OpenDetail, instance);
                        }
                        break;

                    case MessageType.OpenDetail:
                        OpenDetail(model, (bool?)param ?? false, true);
                        break;
                    case MessageType.OpenDetailSilent:
                        OpenDetail(model, (bool?)param ?? false, false);
                        break;
                    case MessageType.CloseDetail:
                        CloseDetail(model);
                        break;
                    case MessageType.DeleteDetail:
                        DeleteDetail(model);
                        break;
                        
                    case MessageType.CopyDetail:
                        var copyModel = model?.Copy();
                        if (copyModel != null)
                        {
                            MasterRepository.Add(copyModel);
                            MessageDispatcher.Send(MessageType.OpenDetail, copyModel, param);
                        }
                        break;

                    case MessageType.OverrideDetail:
                        if (model != null)
                        {
                            var overrideModel = MasterRepository.Project.GetOverride(model);
                            if (overrideModel != null)
                            {
                                MessageDispatcher.Send(MessageType.OpenDetail, overrideModel); 
                                return;
                            }

                            var newModel = model.Override();
                            if (newModel != null)
                            {
                                MasterRepository.Add(newModel);
                                MessageDispatcher.Send(MessageType.OpenDetail, newModel, param);
                            }
                        }
                        break;
                }
            }
            else
            {
                switch(type)
                {
                    case MessageType.OpenGlobalSearch:
                        var viewModel = new GlobalSearchViewModel();
                        WindowService.OpenDialog(viewModel);
                        if (viewModel.ResultModel != null)
                            MessageDispatcher.Send(MessageType.OpenDetail, viewModel.ResultModel, true);
                        break;

                    case MessageType.GotoDetail:
                        var index = (int?)message;
                        if (index != null && index < detailViewList.Count && index >= 0)
                            CurrentView = detailViewList.ElementAt(index ?? 0);
                        break;

                    case MessageType.NewDetail:
                        var newModel = MasterRepository.New((Type?)message ?? typeof(BaseModel));
                        MessageDispatcher.Send(MessageType.OpenDetail, newModel, param);
                        break;

                    case MessageType.NewProject:
                        WindowService.OpenDialog<NewProjectViewModel>();
                        break;

                    case MessageType.OpenProject:
                        if (!inProjectUpdate)
                        {
                            CloseAllProjectDetails();

                            MasterRepository.Project.Load((String?)message ?? "");
                            MessageDispatcher.Send(MessageType.ChangeLanguage, MasterRepository.Project.DefaultLanguage, null);
                            MessageDispatcher.Send(MessageType.UpdateProject, MasterRepository.Project, null);
                        }
                        break;

                    case MessageType.SaveProject:
                        MasterRepository.Project.Save();
                        break;

                    case MessageType.CloseProject:
                        var result = DoQuery("Save changes?", "Save changes before closing the project?", ViewModelQueryType.Question);
                        if (result != ViewModelQueryResult.Cancel)
                        {
                            if (result == ViewModelQueryResult.Yes)
                                MasterRepository.Project.Save();

                            CloseAllProjectDetails();
                            MasterRepository.Project.Close();
                        }
                        break;

                    case MessageType.UpdateProject:
                        if (message is EosProject project)
                        {
                            inProjectUpdate = true;
                            try
                            {
                                var updateService = new ProjectUpdateService(project);
                                if (updateService.CheckForUpdates())
                                {
                                    if (WindowService.ShowMessage("There is an update available to apply to your project to keep it up-to-date.\n" + 
                                        "It is recommended to apply the update timely.\n" + "" +
                                        "A backup of your project will be made before the update!\n\n" + 
                                        "Do you want to start the update process now?", "Project Update", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == MessageBoxResult.Yes)
                                    {
                                        var canUpdate = true;
                                        if (updateService.NeedsGameDataUpdate)
                                        {
                                            canUpdate = EosConfig.CurrentGameBuildDate >= EosConfig.BaseGameDataBuildDate;
                                            if (canUpdate)
                                            {
                                                var gduQuery = DoQuery("Game Data Import", "Before applying the project update(s) the base game data has to be updated\nContinue?", ViewModelQueryType.Question);
                                                if (gduQuery == ViewModelQueryResult.Yes)
                                                    MessageDispatcher.Send(MessageType.DoGameDataImport, false, null);
                                                else
                                                    canUpdate = false;
                                            }
                                        }

                                        if (canUpdate)
                                        {
                                            project.CreateBackup();
                                            if (updateService.ApplyUpdates())
                                                WindowService.ShowMessage("Project was updated successfully!", "Project Update", MessageBoxButtons.Ok, MessageBoxIcon.Information);
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                inProjectUpdate = false;
                            }
                        }
                        break;

                    case MessageType.OpenProjectSettings:
                        WindowService.OpenDialog<ProjectOptionsViewModel>();
                        break;

                    case MessageType.OpenDataImport:
                        WindowService.OpenDialog<ImportDialogViewModel>();
                        break;

                    case MessageType.ChangeLanguage:
                        EosConfig.RuntimeConfig.CurrentLanguage = (TLKLanguage?)message ?? EosConfig.RuntimeConfig.CurrentLanguage;
                        break;

                    case MessageType.DoGameDataImport:
                        DoGameDataImport((bool?)message ?? true);
                        break;
                    
                    case MessageType.OpenTlkEditor:
                        OpenDetail(MasterRepository.Project.CustomTlkStrings, false, true);
                        break;
                }
            }
        }

        public MainWindowViewModel()
        {
            MessageDispatcher.Subscribe(MessageType.NewProject, MessageHandler);
            MessageDispatcher.Subscribe(MessageType.OpenProject, MessageHandler);
            MessageDispatcher.Subscribe(MessageType.SaveProject, MessageHandler);
            MessageDispatcher.Subscribe(MessageType.CloseProject, MessageHandler);
            MessageDispatcher.Subscribe(MessageType.UpdateProject, MessageHandler);

            MessageDispatcher.Subscribe(MessageType.OpenProjectSettings, MessageHandler);
            MessageDispatcher.Subscribe(MessageType.OpenDataImport, MessageHandler);

            MessageDispatcher.Subscribe(MessageType.NewDetail, MessageHandler);
            MessageDispatcher.Subscribe(MessageType.OverrideDetail, MessageHandler);
            MessageDispatcher.Subscribe(MessageType.CopyDetail, MessageHandler);
            MessageDispatcher.Subscribe(MessageType.OpenDetail, MessageHandler);
            MessageDispatcher.Subscribe(MessageType.GotoDetail, MessageHandler);
            MessageDispatcher.Subscribe(MessageType.OpenDetailSilent, MessageHandler);
            MessageDispatcher.Subscribe(MessageType.CloseDetail, MessageHandler);
            MessageDispatcher.Subscribe(MessageType.DeleteDetail, MessageHandler);

            MessageDispatcher.Subscribe(MessageType.NewCustomDetail, MessageHandler);

            MessageDispatcher.Subscribe(MessageType.ChangeLanguage, MessageHandler);
            MessageDispatcher.Subscribe(MessageType.DoGameDataImport, MessageHandler);
            MessageDispatcher.Subscribe(MessageType.OpenGlobalSearch, MessageHandler);
            MessageDispatcher.Subscribe(MessageType.OpenTlkEditor, MessageHandler);
        }

        ~MainWindowViewModel()
        {
            MessageDispatcher.Unsubscribe(MessageType.NewProject, MessageHandler);
            MessageDispatcher.Unsubscribe(MessageType.OpenProject, MessageHandler);
            MessageDispatcher.Unsubscribe(MessageType.SaveProject, MessageHandler);
            MessageDispatcher.Unsubscribe(MessageType.CloseProject, MessageHandler);
            MessageDispatcher.Unsubscribe(MessageType.UpdateProject, MessageHandler);

            MessageDispatcher.Unsubscribe(MessageType.OpenProjectSettings, MessageHandler);
            MessageDispatcher.Unsubscribe(MessageType.OpenDataImport, MessageHandler);

            MessageDispatcher.Unsubscribe(MessageType.NewDetail, MessageHandler);
            MessageDispatcher.Unsubscribe(MessageType.OverrideDetail, MessageHandler);
            MessageDispatcher.Unsubscribe(MessageType.CopyDetail, MessageHandler);
            MessageDispatcher.Unsubscribe(MessageType.OpenDetail, MessageHandler);
            MessageDispatcher.Unsubscribe(MessageType.GotoDetail, MessageHandler);
            MessageDispatcher.Unsubscribe(MessageType.OpenDetailSilent, MessageHandler);
            MessageDispatcher.Unsubscribe(MessageType.CloseDetail, MessageHandler);
            MessageDispatcher.Unsubscribe(MessageType.DeleteDetail, MessageHandler);

            MessageDispatcher.Unsubscribe(MessageType.NewCustomDetail, MessageHandler);

            MessageDispatcher.Unsubscribe(MessageType.ChangeLanguage, MessageHandler);
            MessageDispatcher.Unsubscribe(MessageType.DoGameDataImport, MessageHandler);
            MessageDispatcher.Unsubscribe(MessageType.OpenGlobalSearch, MessageHandler);
            MessageDispatcher.Unsubscribe(MessageType.OpenTlkEditor, MessageHandler);
        }
    }
}
