using Eos.Models;
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
    public class CustomDynamicTableInstanceViewModel : DataDetailViewModel<CustomDynamicTableInstance>
    {
        public CustomDynamicTableInstanceViewModel() : base()
        {
            DeleteCustomDynTableItemCommand = ReactiveCommand.Create<CustomDynamicTableInstanceItem>(DeleteCustomDynTableItem);
            AddCustomDynTableItemCommand = ReactiveCommand.Create(AddCustomDynTableItem);

            MoveUpCommand = ReactiveCommand.Create<CustomDynamicTableInstanceItem>(MoveUp);
            MoveDownCommand = ReactiveCommand.Create<CustomDynamicTableInstanceItem>(MoveDown);
        }

        public CustomDynamicTableInstanceViewModel(CustomDynamicTableInstance instance) : base(instance)
        {
            DeleteCustomDynTableItemCommand = ReactiveCommand.Create<CustomDynamicTableInstanceItem>(DeleteCustomDynTableItem);
            AddCustomDynTableItemCommand = ReactiveCommand.Create(AddCustomDynTableItem);

            MoveUpCommand = ReactiveCommand.Create<CustomDynamicTableInstanceItem>(MoveUp);
            MoveDownCommand = ReactiveCommand.Create<CustomDynamicTableInstanceItem>(MoveDown);
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

        private void AddCustomDynTableItem()
        {
            var newItem = new CustomDynamicTableInstanceItem(Data);
            Data.Add(newItem);
            NotifyPropertyChanged("Data");
        }

        private void DeleteCustomDynTableItem(CustomDynamicTableInstanceItem item)
        {
            this.Data.Remove(item);
            NotifyPropertyChanged("Data");
        }

        private void MoveUp(CustomDynamicTableInstanceItem item)
        {
            Data.MoveUp(item);
        }

        private void MoveDown(CustomDynamicTableInstanceItem item)
        {
            Data.MoveDown(item);
        }

        public ReactiveCommand<CustomDynamicTableInstanceItem, Unit> DeleteCustomDynTableItemCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> AddCustomDynTableItemCommand { get; private set; }

        public ReactiveCommand<CustomDynamicTableInstanceItem, Unit> MoveUpCommand { get; private set; }
        public ReactiveCommand<CustomDynamicTableInstanceItem, Unit> MoveDownCommand { get; private set; }
    }
}
