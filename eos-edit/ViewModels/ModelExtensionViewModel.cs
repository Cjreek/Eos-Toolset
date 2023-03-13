using Avalonia.Threading;
using Eos.Models.Tables;
using Eos.Types;
using Eos.ViewModels.Base;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    internal class ModelExtensionViewModel : DataDetailViewModel<ModelExtension>
    {
        public event EventHandler<DeleteCustomPropertyEventArgs>? OnDeleteCustomProperty;

        public ModelExtensionViewModel() : base()
        {
            DeleteExtensionPropertyCommand = ReactiveCommand.Create<CustomObjectProperty>(DeleteExtensionProperty);
            AddExtensionPropertyCommand = ReactiveCommand.Create(AddExtensionProperty);

            MoveExtensionPropertyUpCommand = ReactiveCommand.Create<CustomObjectProperty>(MoveUp);
            MoveExtensionPropertyDownCommand = ReactiveCommand.Create<CustomObjectProperty>(MoveDown);
        }

        public ModelExtensionViewModel(ModelExtension extension) : base(extension)
        {
            DeleteExtensionPropertyCommand = ReactiveCommand.Create<CustomObjectProperty>(DeleteExtensionProperty);
            AddExtensionPropertyCommand = ReactiveCommand.Create(AddExtensionProperty);

            MoveExtensionPropertyUpCommand = ReactiveCommand.Create<CustomObjectProperty>(MoveUp);
            MoveExtensionPropertyDownCommand = ReactiveCommand.Create<CustomObjectProperty>(MoveDown);
        }

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

        protected override string GetHeader()
        {
            return Data.Name + " Extensions";
        }

        private void AddExtensionProperty()
        {
            var newItem = new CustomObjectProperty();
            Data.Add(newItem);
            NotifyPropertyChanged("Data");
        }

        private void DeleteExtensionProperty(CustomObjectProperty item)
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

        public ReactiveCommand<CustomObjectProperty, Unit> DeleteExtensionPropertyCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> AddExtensionPropertyCommand { get; private set; }

        public ReactiveCommand<CustomObjectProperty, Unit> MoveExtensionPropertyUpCommand { get; private set; }
        public ReactiveCommand<CustomObjectProperty, Unit> MoveExtensionPropertyDownCommand { get; private set; }
    }
}
