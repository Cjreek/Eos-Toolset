using Eos.Models.Tables;
using Eos.Types;
using Eos.ViewModels.Base;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    internal class CustomObjectViewModel : DataDetailViewModel<CustomObject>
    {
        public event EventHandler<DeleteCustomPropertyEventArgs>? OnDeleteCustomProperty;
        public void RemoveAllEventsCalling(EventHandler<DeleteCustomPropertyEventArgs> method)
        {
            if (OnDeleteCustomProperty != null)
            {
                foreach (var d in OnDeleteCustomProperty.GetInvocationList())
                {
                    if (d.Method == method.Method)
                        OnDeleteCustomProperty -= (EventHandler<DeleteCustomPropertyEventArgs>)d;
                }
            }
        }

        public CustomObjectViewModel() : base()
        {
            DeleteObjectPropertyCommand = ReactiveCommand.Create<CustomObjectProperty>(DeleteObjectProperty);
            AddObjectPropertyCommand = ReactiveCommand.Create(AddObjectProperty);

            MoveObjectPropertyUpCommand = ReactiveCommand.Create<CustomObjectProperty>(MoveUp);
            MoveObjectPropertyDownCommand = ReactiveCommand.Create<CustomObjectProperty>(MoveDown);
        }

        public CustomObjectViewModel(CustomObject customObject) : base(customObject)
        {
            DeleteObjectPropertyCommand = ReactiveCommand.Create<CustomObjectProperty>(DeleteObjectProperty);
            AddObjectPropertyCommand = ReactiveCommand.Create(AddObjectProperty);

            MoveObjectPropertyUpCommand = ReactiveCommand.Create<CustomObjectProperty>(MoveUp);
            MoveObjectPropertyDownCommand = ReactiveCommand.Create<CustomObjectProperty>(MoveDown);
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }

        private void AddObjectProperty()
        {
            var newItem = new CustomObjectProperty();
            Data.Add(newItem);
            NotifyPropertyChanged("Data");
        }

        private void DeleteObjectProperty(CustomObjectProperty item)
        {
            var eventData = new DeleteCustomPropertyEventArgs(item);
            OnDeleteCustomProperty?.Invoke(this, eventData);
            if (eventData.CanDelete)
            {
                this.Data.Remove(item);
                NotifyPropertyChanged("Data");
            }
        }

        private void MoveUp(CustomObjectProperty item)
        {
            var index = Data.Items.IndexOf(item);
            if (index > 0)
                Data.Items.Move(index, index - 1);
        }

        private void MoveDown(CustomObjectProperty item)
        {
            var index = Data.Items.IndexOf(item);
            if (index < Data.Items.Count - 1)
                Data.Items.Move(index, index + 1);
        }

        public ReactiveCommand<CustomObjectProperty, Unit> DeleteObjectPropertyCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> AddObjectPropertyCommand { get; private set; }

        public ReactiveCommand<CustomObjectProperty, Unit> MoveObjectPropertyUpCommand { get; private set; }
        public ReactiveCommand<CustomObjectProperty, Unit> MoveObjectPropertyDownCommand { get; private set; }
    }
}
