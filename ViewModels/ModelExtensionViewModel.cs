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
    internal class ModelExtensionViewModel : DataDetailViewModel<ModelExtension>
    {
        public ModelExtensionViewModel() : base()
        {
            DeleteExtensionPropertyCommand = new DelegateCommand<CustomObjectProperty>(DeleteExtensionProperty);
            AddExtensionPropertyCommand = new DelegateCommand(AddExtensionProperty);

            MoveExtensionPropertyUpCommand = new DelegateCommand<CustomObjectProperty>(MoveUp);
            MoveExtensionPropertyDownCommand = new DelegateCommand<CustomObjectProperty>(MoveDown);
        }

        public ModelExtensionViewModel(ModelExtension extension) : base(extension)
        {
            DeleteExtensionPropertyCommand = new DelegateCommand<CustomObjectProperty>(DeleteExtensionProperty);
            AddExtensionPropertyCommand = new DelegateCommand(AddExtensionProperty);

            MoveExtensionPropertyUpCommand = new DelegateCommand<CustomObjectProperty>(MoveUp);
            MoveExtensionPropertyDownCommand = new DelegateCommand<CustomObjectProperty>(MoveDown);
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

        public DelegateCommand<CustomObjectProperty> DeleteExtensionPropertyCommand { get; private set; }
        public DelegateCommand AddExtensionPropertyCommand { get; private set; }

        public DelegateCommand<CustomObjectProperty> MoveExtensionPropertyUpCommand { get; private set; }
        public DelegateCommand<CustomObjectProperty> MoveExtensionPropertyDownCommand { get; private set; }
    }
}
