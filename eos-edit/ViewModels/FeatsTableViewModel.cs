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
    internal class FeatsTableViewModel : DataDetailViewModel<FeatsTable>
    {
        public FeatsTableViewModel() : base()
        {
            DeleteFeatItemCommand = ReactiveCommand.Create<FeatsTableItem>(DeleteFeatItem);
            AddFeatItemCommand = ReactiveCommand.Create<FeatListType?>(AddFeatItem);
        }

        public FeatsTableViewModel(FeatsTable featsTable) : base(featsTable)
        {
            DeleteFeatItemCommand = ReactiveCommand.Create<FeatsTableItem>(DeleteFeatItem);
            AddFeatItemCommand = ReactiveCommand.Create<FeatListType?>(AddFeatItem);
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }

        private void AddFeatItem(FeatListType? type)
        {
            if (type != null)
            {
                var newItem = new FeatsTableItem();
                newItem.FeatList = type ?? FeatListType.GeneralFeat;
                newItem.Menu = FeatMenu.NoMenuEntry;
                if (type == FeatListType.AutomaticallyGranted)
                    newItem.GrantedOnLevel = 1;
                else
                    newItem.GrantedOnLevel = -1;
                Data.Add(newItem);
            }
            else
            {
                var newItem = new FeatsTableItem();
                newItem.FeatList = FeatListType.GeneralFeatOrBonusFeat;
                newItem.Menu = FeatMenu.ClassRadialMenu;
                newItem.GrantedOnLevel = 99;
                Data.Add(newItem);
            }
        }

        private void DeleteFeatItem(FeatsTableItem item)
        {
            this.Data.Remove(item);
        }

        public ReactiveCommand<FeatsTableItem, Unit> DeleteFeatItemCommand { get; private set; }
        public ReactiveCommand<FeatListType?, Unit> AddFeatItemCommand { get; private set; }
    }
}
