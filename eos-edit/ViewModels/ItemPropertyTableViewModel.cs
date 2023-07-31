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
    public class ItemPropertyTableViewModel : DataDetailViewModel<ItemPropertyTable>
    {
        public ItemPropertyTableViewModel() : base()
        {
            DeleteItemPropertyTableItemCommand = ReactiveCommand.Create<ItemPropertyTableItem>(DeleteItemPropertyTableItem);
            AddItemPropertyTableItemCommand = ReactiveCommand.Create(AddItemPropertyTableItem);

            MoveUpCommand = ReactiveCommand.Create<ItemPropertyTableItem>(MoveUp);
            MoveDownCommand = ReactiveCommand.Create<ItemPropertyTableItem>(MoveDown);
        }

        public ItemPropertyTableViewModel(ItemPropertyTable itemPropertyTable) : base(itemPropertyTable)
        {
            DeleteItemPropertyTableItemCommand = ReactiveCommand.Create<ItemPropertyTableItem>(DeleteItemPropertyTableItem);
            AddItemPropertyTableItemCommand = ReactiveCommand.Create(AddItemPropertyTableItem);

            MoveUpCommand = ReactiveCommand.Create<ItemPropertyTableItem>(MoveUp);
            MoveDownCommand = ReactiveCommand.Create<ItemPropertyTableItem>(MoveDown);
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }

        protected override ISolidColorBrush GetEntityColor()
        {
            return base.GetEntityColor();
        }

        private void AddItemPropertyTableItem()
        {
            var newItem = new ItemPropertyTableItem(Data);
            Data.Add(newItem);
            NotifyPropertyChanged("Data");
        }

        private void DeleteItemPropertyTableItem(ItemPropertyTableItem item)
        {
            this.Data.Remove(item);
            NotifyPropertyChanged("Data");
        }

        private void MoveUp(ItemPropertyTableItem item)
        {
            var index = Data.Items.IndexOf(item);
            if (index > 0)
                Data.Items.Move(index, index - 1);
        }

        private void MoveDown(ItemPropertyTableItem item)
        {
            var index = Data.Items.IndexOf(item);
            if (index < Data.Items.Count - 1)
                Data.Items.Move(index, index + 1);
        }

        public ReactiveCommand<ItemPropertyTableItem, Unit> DeleteItemPropertyTableItemCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> AddItemPropertyTableItemCommand { get; private set; }

        public ReactiveCommand<ItemPropertyTableItem, Unit> MoveUpCommand { get; private set; }
        public ReactiveCommand<ItemPropertyTableItem, Unit> MoveDownCommand { get; private set; }
    }
}
