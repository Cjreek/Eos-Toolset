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
    public class CustomTableInstanceViewModel : DataDetailViewModel<CustomTableInstance>
    {
        public CustomTableInstanceViewModel() : base()
        {
            DeleteItemCommand = ReactiveCommand.Create<CustomTableInstanceItem>(DeleteItem);
            AddItemCommand = ReactiveCommand.Create(AddItem);
        }

        public CustomTableInstanceViewModel(CustomTableInstance instance) : base(instance)
        {
            DeleteItemCommand = ReactiveCommand.Create<CustomTableInstanceItem>(DeleteItem);
            AddItemCommand = ReactiveCommand.Create(AddItem);
        }

        protected override HashSet<String> GetHeaderSourceFields()
        {
            return new HashSet<String>()
            {
                "Label"
            };
        }

        protected override string GetHeader()
        {
            return Data.Label;
        }

        private void AddItem()
        {
            var newItem = new CustomTableInstanceItem();
            Data.Add(newItem);
            NotifyPropertyChanged("Data");
        }

        private void DeleteItem(CustomTableInstanceItem item)
        {
            this.Data.Remove(item);
            NotifyPropertyChanged("Data");
        }

        public ReactiveCommand<CustomTableInstanceItem, Unit> DeleteItemCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> AddItemCommand { get; private set; }
    }
}
