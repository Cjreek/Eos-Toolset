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
    internal class ViewModelBase : INotifyPropertyChanged
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
