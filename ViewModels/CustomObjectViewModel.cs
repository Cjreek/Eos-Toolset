using Eos.Models.Tables;
using Eos.Types;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    internal class CustomObjectViewModel : DataDetailViewModel<CustomObject>
    {
        public CustomObjectViewModel() : base()
        {
            DeleteObjectPropertyCommand = new DelegateCommand<CustomObjectProperty>(DeleteObjectProperty);
            AddObjectPropertyCommand = new DelegateCommand(AddObjectProperty);

            MoveObjectPropertyUpCommand = new DelegateCommand<CustomObjectProperty>(MoveUp);
            MoveObjectPropertyDownCommand = new DelegateCommand<CustomObjectProperty>(MoveDown);
        }

        public CustomObjectViewModel(CustomObject customObject) : base(customObject)
        {
            DeleteObjectPropertyCommand = new DelegateCommand<CustomObjectProperty>(DeleteObjectProperty);
            AddObjectPropertyCommand = new DelegateCommand(AddObjectProperty);

            MoveObjectPropertyUpCommand = new DelegateCommand<CustomObjectProperty>(MoveUp);
            MoveObjectPropertyDownCommand = new DelegateCommand<CustomObjectProperty>(MoveDown);
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
            this.Data.Remove(item);
            NotifyPropertyChanged("Data");
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

        public DelegateCommand<CustomObjectProperty> DeleteObjectPropertyCommand { get; private set; }
        public DelegateCommand AddObjectPropertyCommand { get; private set; }

        public DelegateCommand<CustomObjectProperty> MoveObjectPropertyUpCommand { get; private set; }
        public DelegateCommand<CustomObjectProperty> MoveObjectPropertyDownCommand { get; private set; }
    }
}
