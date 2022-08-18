﻿using Eos.Nwn.Tlk;
using Eos.ViewModels.Base;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // Commands
        public DelegateCommand<TLKLanguage?> ChangeLanguageCommand { get; private set; } = new DelegateCommand<TLKLanguage?>(lang =>
        {
            MessageDispatcher.Send(MessageType.ChangeLanguage, lang);
        });

        public DelegateCommand NewProjectCommand { get; private set; } = new DelegateCommand(() =>
        {
            MessageDispatcher.Send(MessageType.NewProject, null);
        });

        public DelegateCommand<String> OpenProjectCommand { get; private set; } = new DelegateCommand<String>(projectFolder =>
        {
            MessageDispatcher.Send(MessageType.OpenProject, projectFolder);
        });

        public DelegateCommand SaveProjectCommand { get; private set; } = new DelegateCommand(() =>
        {
            MessageDispatcher.Send(MessageType.SaveProject, null);
        });

        public DelegateCommand<Type> NewDetailCommand { get; private set; } = new DelegateCommand<Type>(detailModelType =>
        {
            MessageDispatcher.Send(MessageType.NewDetail, detailModelType);
        });

        public DelegateCommand<object> OpenDetailCommand { get; private set; } = new DelegateCommand<object>(detailModel =>
        {
            MessageDispatcher.Send(MessageType.OpenDetail, detailModel);
        });

        public DelegateCommand<object> OpenDetailSilentCommand { get; private set; } = new DelegateCommand<object>(detailModel =>
        {
            MessageDispatcher.Send(MessageType.OpenDetailSilent, detailModel);
        });

        public DelegateCommand<object> CloseDetailCommand { get; private set; } = new DelegateCommand<object>(detailModel =>
        {
            MessageDispatcher.Send(MessageType.CloseDetail, detailModel);
        });
    }
}
