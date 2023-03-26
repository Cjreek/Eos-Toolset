using Eos.Models;
using Eos.Models.Tables;
using Eos.Nwn.Tlk;
using Eos.ViewModels.Base;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Eos.ViewModels.Base
{
    public enum ViewModelQueryResult
    {
        Yes, No, Cancel
    }

    public enum ViewModelQueryType
    {
        Information, Question, Warning, Error
    }

    public class ViewModelErrorEventArgs : EventArgs
    {
        public String Message { get; set; } = String.Empty;
    }

    public class ViewModelQueryEventArgs : EventArgs
    {
        public String Title { get; set; } = String.Empty;
        public String Message { get; set; } = String.Empty;
        public ViewModelQueryType QueryType { get; set; } = ViewModelQueryType.Question;
        public ViewModelQueryResult Result { get; set; } = ViewModelQueryResult.Cancel;
    }

    public delegate void ErrorEventHandler(ViewModelBase viewModel, ViewModelErrorEventArgs args);
    public delegate void QueryEventHandler(ViewModelBase viewModel, ViewModelQueryEventArgs args);

    public class ViewModelBase : ReactiveObject
    {
        public event ErrorEventHandler? OnError;
        public event QueryEventHandler? OnQuery;

        protected void DoError(String message)
        {
            OnError?.Invoke(this, new ViewModelErrorEventArgs() { Message = message });
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.RaisePropertyChanged(propertyName);
        }

        protected ViewModelQueryResult DoQuery(String title, String message, ViewModelQueryType type = ViewModelQueryType.Question)
        {
            var args = new ViewModelQueryEventArgs();
            args.Title = title;
            args.Message = message;
            args.Result = ViewModelQueryResult.Cancel;
            args.QueryType = type;

            OnQuery?.Invoke(this, args);

            return args.Result;
        }

        // Commands
        public ReactiveCommand<TLKLanguage?, Unit> ChangeLanguageCommand { get; private set; } = ReactiveCommand.Create<TLKLanguage?>((lang) =>
        {
            MessageDispatcher.Send(MessageType.ChangeLanguage, lang);
        });

        public ReactiveCommand<Unit, Unit> NewProjectCommand { get; private set; } = ReactiveCommand.Create(() =>
        {
            MessageDispatcher.Send(MessageType.NewProject, null);
        });

        public ReactiveCommand<String, Unit> OpenProjectCommand { get; private set; } = ReactiveCommand.Create<String>(projectFolder =>
        {
            MessageDispatcher.Send(MessageType.OpenProject, projectFolder);
        });

        public ReactiveCommand<Unit, Unit> SaveProjectCommand { get; private set; } = ReactiveCommand.Create(() =>
        {
            MessageDispatcher.Send(MessageType.SaveProject, null);
        });

        public ReactiveCommand<Unit, Unit> CloseProjectCommand { get; private set; } = ReactiveCommand.Create(() =>
        {
            MessageDispatcher.Send(MessageType.CloseProject, null);
        });

        public ReactiveCommand<Unit, Unit> OpenProjectSettingsCommand { get; private set; } = ReactiveCommand.Create(() =>
        {
            MessageDispatcher.Send(MessageType.OpenProjectSettings, null);
        });

        public ReactiveCommand<Type, Unit> NewDetailCommand { get; private set; } = ReactiveCommand.Create<Type>(detailModelType =>
        {
            MessageDispatcher.Send(MessageType.NewDetail, detailModelType);
        });

        public ReactiveCommand<CustomObject, Unit> NewCustomDetailCommand { get; private set; } = ReactiveCommand.Create<CustomObject>(template =>
        {
            MessageDispatcher.Send(MessageType.NewCustomDetail, template);
        });

        public ReactiveCommand<BaseModel, Unit> OverrideDetailCommand { get; private set; } = ReactiveCommand.Create<BaseModel>(originalModel =>
        {
            MessageDispatcher.Send(MessageType.OverrideDetail, originalModel);
        });

        public ReactiveCommand<BaseModel, Unit> CopyDetailCommand { get; private set; } = ReactiveCommand.Create<BaseModel>(originalModel =>
        {
            MessageDispatcher.Send(MessageType.CopyDetail, originalModel);
        });

        public ReactiveCommand<object, Unit> OpenDetailCommand { get; private set; } = ReactiveCommand.Create<object>(detailModel =>
        {
            MessageDispatcher.Send(MessageType.OpenDetail, detailModel);
        });

        public ReactiveCommand<object, Unit> OpenDetailSilentCommand { get; private set; } = ReactiveCommand.Create<object>(detailModel =>
        {
            MessageDispatcher.Send(MessageType.OpenDetailSilent, detailModel);
        });

        public ReactiveCommand<object, Unit> CloseDetailCommand { get; private set; } = ReactiveCommand.Create<object>(detailModel =>
        {
            MessageDispatcher.Send(MessageType.CloseDetail, detailModel);
        });

        public ReactiveCommand<object, Unit> DeleteDetailCommand { get; private set; } = ReactiveCommand.Create<object>(detailModel =>
        {
            MessageDispatcher.Send(MessageType.DeleteDetail, detailModel);
        });

        public ReactiveCommand<int?, Unit> GotoDetailCommand { get; private set; } = ReactiveCommand.Create<int?>(index =>
        {
            MessageDispatcher.Send(MessageType.GotoDetail, index);
        });

        public ReactiveCommand<Unit, Unit> GlobalSearchCommand { get; private set; } = ReactiveCommand.Create(() =>
        {
            MessageDispatcher.Send(MessageType.OpenGlobalSearch, null);
        });

        public ReactiveCommand<Unit, Unit> OpenDataImportCommand { get; private set; } = ReactiveCommand.Create(() =>
        {
            MessageDispatcher.Send(MessageType.OpenDataImport, null);
        });
    }
}
