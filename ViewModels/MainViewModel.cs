using Prism.Commands;
using Eos.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Eos.ViewModels.Base;
using Eos.Nwn.Tlk;
using Eos.Services;
using Eos.Views;
using System.Globalization;
using Eos.ViewModels.Dialogs;
using Eos.Repositories;
using Eos.Models.Tables;

namespace Eos.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        private ObservableCollection<DataDetailViewModelBase> detailViewList = new ObservableCollection<DataDetailViewModelBase>();
        private Dictionary<object, DataDetailViewModelBase> detailViewDict = new Dictionary<object, DataDetailViewModelBase>();

        private DataDetailViewModelBase? currentView;
        private TLKLanguage currentLanguage;
        private bool currentGender;

        public ObservableCollection<DataDetailViewModelBase> DetailViewList { get { return detailViewList; } }

        public TLKLanguage CurrentLanguage
        {
            get { return currentLanguage; }
            set
            {
                if (currentLanguage != value)
                {
                    currentLanguage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool CurrentGender
        {
            get { return currentGender; }
            set
            {
                if (currentGender != value)
                {
                    currentGender = value;
                    NotifyPropertyChanged();
                }
            }
        }

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
            if ((jumpToOverride) && !(model is CustomObjectInstance))
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

        private void DeleteDetail(BaseModel model)
        {
            if (model.ReferenceCount > 0)
            {
                var message = "There are still " + model.ReferenceCount.ToString() + " references to this " + model.GetType().Name + ":\n\n";
                foreach (var reference in model.References)
                    message += "> [" + reference.ReferenceObject?.GetType().Name + "] " + reference.ReferenceName + " (" + reference.ReferenceProperty + ")\n";
                message += "\nDo you still want to delete this item and remove all its references?";
                var queryResult = DoQuery("References", message);
                if (queryResult != ViewModelQueryResult.Yes)
                    return;
            }

            CloseDetail(model);
            MasterRepository.Project.Delete(model);
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
                            MasterRepository.Project.CustomObjectRepositories[template].Add(instance);
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
                        MasterRepository.Project.Load((String?)message ?? "");
                        MessageDispatcher.Send(MessageType.ChangeLanguage, MasterRepository.Project.DefaultLanguage, null);
                        break;

                    case MessageType.SaveProject:
                        MasterRepository.Project.Save();
                        break;

                    case MessageType.ChangeLanguage:
                        CurrentLanguage = (TLKLanguage?)message ?? CurrentLanguage;
                        break;
                }
            }
        }

        public MainViewModel()
        {
            MessageDispatcher.Subscribe(MessageType.NewProject, MessageHandler);
            MessageDispatcher.Subscribe(MessageType.OpenProject, MessageHandler);
            MessageDispatcher.Subscribe(MessageType.SaveProject, MessageHandler);

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
            MessageDispatcher.Subscribe(MessageType.OpenGlobalSearch, MessageHandler);
        }

        ~MainViewModel()
        {
            MessageDispatcher.Unsubscribe(MessageType.NewProject, MessageHandler);
            MessageDispatcher.Unsubscribe(MessageType.OpenProject, MessageHandler);
            MessageDispatcher.Unsubscribe(MessageType.SaveProject, MessageHandler);

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
            MessageDispatcher.Unsubscribe(MessageType.OpenGlobalSearch, MessageHandler);
        }
    }
}
