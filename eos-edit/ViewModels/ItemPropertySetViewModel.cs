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
    public class ItemPropertySetViewModel : DataDetailViewModel<ItemPropertySet>
    {
        public ItemPropertySetViewModel() : base()
        {
            DeleteItemPropertyItemCommand = ReactiveCommand.Create<ItemPropertySetEntry>(DeleteItemPropertyItem);
            AddItemPropertyItemCommand = ReactiveCommand.Create(AddItemPropertyItem);
        }

        public ItemPropertySetViewModel(ItemPropertySet itemPropertySet) : base(itemPropertySet)
        {
            DeleteItemPropertyItemCommand = ReactiveCommand.Create<ItemPropertySetEntry>(DeleteItemPropertyItem);
            AddItemPropertyItemCommand = ReactiveCommand.Create(AddItemPropertyItem);
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }

        protected override ISolidColorBrush GetEntityColor()
        {
            return base.GetEntityColor();
        }

        private void AddItemPropertyItem()
        {
            var newItem = new ItemPropertySetEntry(Data);
            Data.ItemProperties.Add(newItem);
            NotifyPropertyChanged("Data");
        }

        private void DeleteItemPropertyItem(ItemPropertySetEntry item)
        {
            this.Data.ItemProperties.Remove(item);
            NotifyPropertyChanged("Data");
        }

        public ReactiveCommand<ItemPropertySetEntry, Unit> DeleteItemPropertyItemCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> AddItemPropertyItemCommand { get; private set; }
    }
}
