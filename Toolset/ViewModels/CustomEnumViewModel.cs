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
    internal class CustomEnumViewModel : DataDetailViewModel<CustomEnum>
    {
        public CustomEnumViewModel() : base()
        {
            DeleteEnumItemCommand = new DelegateCommand<CustomEnumItem>(DeleteEnumItem);
            AddEnumItemCommand = new DelegateCommand(AddEnumItem);
        }

        public CustomEnumViewModel(CustomEnum customEnum) : base(customEnum)
        {
            DeleteEnumItemCommand = new DelegateCommand<CustomEnumItem>(DeleteEnumItem);
            AddEnumItemCommand = new DelegateCommand(AddEnumItem);
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

        public DelegateCommand<CustomEnumItem> DeleteEnumItemCommand { get; private set; }
        public DelegateCommand AddEnumItemCommand { get; private set; }
    }
}
