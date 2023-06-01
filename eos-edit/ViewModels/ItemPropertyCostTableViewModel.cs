using Avalonia.Media;
using Eos.Models;
using Eos.Models.Tables;
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
    internal class ItemPropertyCostTableViewModel : DataDetailViewModel<ItemPropertyCostTable>
    {
        public ItemPropertyCostTableViewModel() : base()
        {
            DeleteItemPropertyCostTableItemCommand = ReactiveCommand.Create<ItemPropertyCostTableItem>(DeleteItemPropertyCostTableItem);
            AddItemPropertyCostTableItemCommand = ReactiveCommand.Create(AddItemPropertyCostTableItem);

            MoveUpCommand = ReactiveCommand.Create<ItemPropertyCostTableItem>(MoveUp);
            MoveDownCommand = ReactiveCommand.Create<ItemPropertyCostTableItem>(MoveDown);
        }

        public ItemPropertyCostTableViewModel(ItemPropertyCostTable itemPropertyCostTable) : base(itemPropertyCostTable)
        {
            DeleteItemPropertyCostTableItemCommand = ReactiveCommand.Create<ItemPropertyCostTableItem>(DeleteItemPropertyCostTableItem);
            AddItemPropertyCostTableItemCommand = ReactiveCommand.Create(AddItemPropertyCostTableItem);

            MoveUpCommand = ReactiveCommand.Create<ItemPropertyCostTableItem>(MoveUp);
            MoveDownCommand = ReactiveCommand.Create<ItemPropertyCostTableItem>(MoveDown);
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }

        protected override ISolidColorBrush GetEntityColor()
        {
            return base.GetEntityColor();
        }

        private void AddItemPropertyCostTableItem()
        {
            var newItem = new ItemPropertyCostTableItem(Data);
            Data.Add(newItem);
            NotifyPropertyChanged("Data");
        }

        private void DeleteItemPropertyCostTableItem(ItemPropertyCostTableItem item)
        {
            this.Data.Remove(item);
            NotifyPropertyChanged("Data");
        }

        private void MoveUp(ItemPropertyCostTableItem item)
        {
            var index = Data.Items.IndexOf(item);
            if (index > 0)
                Data.Items.Move(index, index - 1);
        }

        private void MoveDown(ItemPropertyCostTableItem item)
        {
            var index = Data.Items.IndexOf(item);
            if (index < Data.Items.Count - 1)
                Data.Items.Move(index, index + 1);
        }

        public ReactiveCommand<ItemPropertyCostTableItem, Unit> DeleteItemPropertyCostTableItemCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> AddItemPropertyCostTableItemCommand { get; private set; }

        public ReactiveCommand<ItemPropertyCostTableItem, Unit> MoveUpCommand { get; private set; }
        public ReactiveCommand<ItemPropertyCostTableItem, Unit> MoveDownCommand { get; private set; }
    }
}
