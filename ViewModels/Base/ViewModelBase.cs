using Eos.Models;
using Eos.Models.Tables;
using Eos.Nwn.Tlk;
using Eos.ViewModels.Base;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Eos.ViewModels
{
    public enum ViewModelQueryResult
    {
        Yes, No, Cancel
    }

    public class ViewModelErrorEventArgs : EventArgs
    {
        public String Message { get; set; } = String.Empty;
    }

    public class ViewModelQueryEventArgs : EventArgs
    {
        public String Title { get; set; } = String.Empty;
        public String Message { get; set; } = String.Empty;
        public ViewModelQueryResult Result { get; set; } = ViewModelQueryResult.Cancel;
    }

    public delegate void ErrorEventHandler(ViewModelBase viewModel, ViewModelErrorEventArgs args);
    public delegate void QueryEventHandler(ViewModelBase viewModel, ViewModelQueryEventArgs args);

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

        public event ErrorEventHandler? OnError;
        public event QueryEventHandler? OnQuery;

        protected void DoError(String message)
        {
            OnError?.Invoke(this, new ViewModelErrorEventArgs() { Message = message });
        }

        protected ViewModelQueryResult DoQuery(String title, String message)
        {
            var args = new ViewModelQueryEventArgs();
            args.Title = title;
            args.Message = message;
            args.Result = ViewModelQueryResult.Cancel;

            OnQuery?.Invoke(this, args);

            return args.Result;
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

        public DelegateCommand<CustomObject> NewCustomDetailCommand { get; private set; } = new DelegateCommand<CustomObject>(template =>
        {
            MessageDispatcher.Send(MessageType.NewCustomDetail, template);
        });

        public DelegateCommand<BaseModel> OverrideDetailCommand { get; private set; } = new DelegateCommand<BaseModel>(originalModel =>
        {
            MessageDispatcher.Send(MessageType.OverrideDetail, originalModel);
        });

        public DelegateCommand<BaseModel> CopyDetailCommand { get; private set; } = new DelegateCommand<BaseModel>(originalModel =>
        {
            MessageDispatcher.Send(MessageType.CopyDetail, originalModel);
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

        public DelegateCommand<object> DeleteDetailCommand { get; private set; } = new DelegateCommand<object>(detailModel =>
        {
            MessageDispatcher.Send(MessageType.DeleteDetail, detailModel);
        });

        public DelegateCommand<int?> GotoDetailCommand { get; private set; } = new DelegateCommand<int?>(index =>
        {
            MessageDispatcher.Send(MessageType.GotoDetail, index);
        });

        public DelegateCommand GlobalSearchCommand { get; private set; } = new DelegateCommand(() =>
        {
            MessageDispatcher.Send(MessageType.OpenGlobalSearch, null);
        });
    }
}
