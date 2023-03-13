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
    internal class CustomEnumViewModel : DataDetailViewModel<CustomEnum>
    {
        public CustomEnumViewModel() : base()
        {
            DeleteEnumItemCommand = ReactiveCommand.Create<CustomEnumItem>(DeleteEnumItem);
            AddEnumItemCommand = ReactiveCommand.Create(AddEnumItem);
        }

        public CustomEnumViewModel(CustomEnum customEnum) : base(customEnum)
        {
            DeleteEnumItemCommand = ReactiveCommand.Create<CustomEnumItem>(DeleteEnumItem);
            AddEnumItemCommand = ReactiveCommand.Create(AddEnumItem);
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }

        private void AddEnumItem()
        {
            var newItem = new CustomEnumItem();
            Data.Add(newItem);
            NotifyPropertyChanged("Data");
        }

        private void DeleteEnumItem(CustomEnumItem item)
        {
            this.Data.Remove(item);
            NotifyPropertyChanged("Data");
        }

        public ReactiveCommand<CustomEnumItem, Unit> DeleteEnumItemCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> AddEnumItemCommand { get; private set; }
    }
}
