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
using Eos.Repositories;
using Eos.ViewModels.Base;
using Eos.Nwn.Tlk;
using Eos.Services;

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

        private void OpenDetail(BaseModel model, bool changeView)
        {
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

        private void MessageHandler(MessageType type, object? param)
        {
            if (param is BaseModel model)
            {
                switch (type)
                {
                    case MessageType.OpenDetail:
                        OpenDetail(model, true);
                        break;
                    case MessageType.OpenDetailSilent:
                        OpenDetail(model, false);
                        break;
                    case MessageType.CloseDetail:
                        CloseDetail(model);
                        break;
                }
            }
            else
            {
                switch(type)
                {
                    case MessageType.NewDetail:
                        var newModel = MasterRepository.New((Type?)param ?? typeof(BaseModel));
                        MessageDispatcher.Send(MessageType.OpenDetail, newModel);
                        break;

                    case MessageType.NewProject:
                        WindowService.OpenDialog<NewProjectViewModel>();
                        break;

                    case MessageType.OpenProject:
                        MasterRepository.Project.Load((String?)param ?? "");
                        MessageDispatcher.Send(MessageType.ChangeLanguage, MasterRepository.Project.DefaultLanguage);
                        break;

                    case MessageType.SaveProject:
                        MasterRepository.Project.Save();
                        break;

                    case MessageType.ChangeLanguage:
                        CurrentLanguage = (TLKLanguage?)param ?? CurrentLanguage;
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
            MessageDispatcher.Subscribe(MessageType.OpenDetail, MessageHandler);
            MessageDispatcher.Subscribe(MessageType.OpenDetailSilent, MessageHandler);
            MessageDispatcher.Subscribe(MessageType.CloseDetail, MessageHandler);

            MessageDispatcher.Subscribe(MessageType.ChangeLanguage, MessageHandler);
        }

        ~MainViewModel()
        {
            MessageDispatcher.Unsubscribe(MessageType.NewProject, MessageHandler);
            MessageDispatcher.Unsubscribe(MessageType.OpenProject, MessageHandler);
            MessageDispatcher.Unsubscribe(MessageType.SaveProject, MessageHandler);

            MessageDispatcher.Unsubscribe(MessageType.NewDetail, MessageHandler);
            MessageDispatcher.Unsubscribe(MessageType.OpenDetail, MessageHandler);
            MessageDispatcher.Unsubscribe(MessageType.OpenDetailSilent, MessageHandler);
            MessageDispatcher.Unsubscribe(MessageType.CloseDetail, MessageHandler);

            MessageDispatcher.Unsubscribe(MessageType.ChangeLanguage, MessageHandler);
        }
    }
}
