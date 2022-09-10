using Eos.Models.Tables;
using Eos.Types;
using Eos.ViewModels.Base;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    internal class FeatsTableViewModel : DataDetailViewModel<FeatsTable>
    {
        public FeatsTableViewModel() : base()
        {
            DeleteFeatItemCommand = new DelegateCommand<FeatsTableItem>(DeleteFeatItem);
            AddFeatItemCommand = new DelegateCommand<FeatListType?>(AddFeatItem);
        }

        public FeatsTableViewModel(FeatsTable featsTable) : base(featsTable)
        {
            DeleteFeatItemCommand = new DelegateCommand<FeatsTableItem>(DeleteFeatItem);
            AddFeatItemCommand = new DelegateCommand<FeatListType?>(AddFeatItem);
            MessageDispatcher.Subscribe(MessageType.FeatTableItemChanged, MessageHandler);
        }

        private void MessageHandler(MessageType type, object? param)
        {
            Data.Changed();
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
                Data.Changed();
            }
        }

        private void DeleteFeatItem(FeatsTableItem item)
        {
            this.Data.Remove(item);
            Data.Changed();
        }

        public DelegateCommand<FeatsTableItem> DeleteFeatItemCommand { get; private set; }
        public DelegateCommand<FeatListType?> AddFeatItemCommand { get; private set; }
    }
}
