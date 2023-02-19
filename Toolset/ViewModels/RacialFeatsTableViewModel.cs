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
    internal class RacialFeatsTableViewModel : DataDetailViewModel<RacialFeatsTable>
    {
        public RacialFeatsTableViewModel() : base()
        {
            DeleteRacialFeatItemCommand = new DelegateCommand<RacialFeatsTableItem>(DeleteRacialFeatItem);
            AddRacialFeatItemCommand = new DelegateCommand(AddRacialFeatItem);
        }

        public RacialFeatsTableViewModel(RacialFeatsTable racialFeatsTable) : base(racialFeatsTable)
        {
            DeleteRacialFeatItemCommand = new DelegateCommand<RacialFeatsTableItem>(DeleteRacialFeatItem);
            AddRacialFeatItemCommand = new DelegateCommand(AddRacialFeatItem);
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }

        private void AddRacialFeatItem()
        {
            var newItem = new RacialFeatsTableItem();
            Data.Add(newItem);
            NotifyPropertyChanged("Data");
        }

        private void DeleteRacialFeatItem(RacialFeatsTableItem item)
        {
            this.Data.Remove(item);
            NotifyPropertyChanged("Data");
        }

        public DelegateCommand<RacialFeatsTableItem> DeleteRacialFeatItemCommand { get; private set; }
        public DelegateCommand AddRacialFeatItemCommand { get; private set; }
    }
}
