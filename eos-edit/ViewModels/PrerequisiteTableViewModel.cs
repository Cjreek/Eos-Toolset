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
    internal class PrerequisiteTableViewModel : DataDetailViewModel<PrerequisiteTable>
    {
        public PrerequisiteTableViewModel() : base()
        {
            DeletePrerequisiteItemCommand = ReactiveCommand.Create<PrerequisiteTableItem>(DeletePrerequisiteItem);
            AddPrerequisiteItemCommand = ReactiveCommand.Create(AddPrerequisiteItem);
        }

        public PrerequisiteTableViewModel(PrerequisiteTable requTable) : base(requTable)
        {
            DeletePrerequisiteItemCommand = ReactiveCommand.Create<PrerequisiteTableItem>(DeletePrerequisiteItem);
            AddPrerequisiteItemCommand = ReactiveCommand.Create(AddPrerequisiteItem);
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

        public ReactiveCommand<PrerequisiteTableItem, Unit> DeletePrerequisiteItemCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> AddPrerequisiteItemCommand { get; private set; }
    }
}
