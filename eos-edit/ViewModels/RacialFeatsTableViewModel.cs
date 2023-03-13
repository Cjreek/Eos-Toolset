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
    internal class RacialFeatsTableViewModel : DataDetailViewModel<RacialFeatsTable>
    {
        public RacialFeatsTableViewModel() : base()
        {
            DeleteRacialFeatItemCommand = ReactiveCommand.Create<RacialFeatsTableItem>(DeleteRacialFeatItem);
            AddRacialFeatItemCommand = ReactiveCommand.Create(AddRacialFeatItem);
        }

        public RacialFeatsTableViewModel(RacialFeatsTable racialFeatsTable) : base(racialFeatsTable)
        {
            DeleteRacialFeatItemCommand = ReactiveCommand.Create<RacialFeatsTableItem>(DeleteRacialFeatItem);
            AddRacialFeatItemCommand = ReactiveCommand.Create(AddRacialFeatItem);
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

        public ReactiveCommand<RacialFeatsTableItem, Unit> DeleteRacialFeatItemCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> AddRacialFeatItemCommand { get; private set; }
    }
}
