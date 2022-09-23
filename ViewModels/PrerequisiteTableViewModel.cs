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
    internal class PrerequisiteTableViewModel : DataDetailViewModel<PrerequisiteTable>
    {
        public PrerequisiteTableViewModel() : base()
        {
            DeletePrerequisiteItemCommand = new DelegateCommand<PrerequisiteTableItem>(DeletePrerequisiteItem);
            AddPrerequisiteItemCommand = new DelegateCommand(AddPrerequisiteItem);
        }

        public PrerequisiteTableViewModel(PrerequisiteTable requTable) : base(requTable)
        {
            DeletePrerequisiteItemCommand = new DelegateCommand<PrerequisiteTableItem>(DeletePrerequisiteItem);
            AddPrerequisiteItemCommand = new DelegateCommand(AddPrerequisiteItem);
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }

        private void AddPrerequisiteItem()
        {
            var newItem = new PrerequisiteTableItem();
            Data.Add(newItem);
            NotifyPropertyChanged("Data");
        }

        private void DeletePrerequisiteItem(PrerequisiteTableItem item)
        {
            this.Data.Remove(item);
            NotifyPropertyChanged("Data");
        }

        public DelegateCommand<PrerequisiteTableItem> DeletePrerequisiteItemCommand { get; private set; }
        public DelegateCommand AddPrerequisiteItemCommand { get; private set; }
    }
}
