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
    public class PackageFeatPreferencesTableViewModel : DataDetailViewModel<PackageFeatPreferencesTable>
    {
        public PackageFeatPreferencesTableViewModel() : base()
        {
            DeleteFeatPreferenceItemCommand = ReactiveCommand.Create<PackageFeatPreferencesTableItem>(DeleteFeatPreferenceItem);
            AddFeatPreferenceItemCommand = ReactiveCommand.Create(AddFeatPreferenceItem);

            MoveFeatPrefUpCommand = ReactiveCommand.Create<PackageFeatPreferencesTableItem>(MoveUp);
            MoveFeatPrefDownCommand = ReactiveCommand.Create<PackageFeatPreferencesTableItem>(MoveDown);
        }

        public PackageFeatPreferencesTableViewModel(PackageFeatPreferencesTable featPreferencesTable) : base(featPreferencesTable)
        {
            DeleteFeatPreferenceItemCommand = ReactiveCommand.Create<PackageFeatPreferencesTableItem>(DeleteFeatPreferenceItem);
            AddFeatPreferenceItemCommand = ReactiveCommand.Create(AddFeatPreferenceItem);

            MoveFeatPrefUpCommand = ReactiveCommand.Create<PackageFeatPreferencesTableItem>(MoveUp);
            MoveFeatPrefDownCommand = ReactiveCommand.Create<PackageFeatPreferencesTableItem>(MoveDown);
        }

        protected override ISolidColorBrush GetEntityColor()
        {
            return base.GetEntityColor();
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }

        private void AddFeatPreferenceItem()
        {
            var newItem = new PackageFeatPreferencesTableItem();
            Data.Add(newItem);
            NotifyPropertyChanged("Data");
        }

        private void DeleteFeatPreferenceItem(PackageFeatPreferencesTableItem item)
        {
            this.Data.Remove(item);
            NotifyPropertyChanged("Data");
        }

        private void MoveUp(PackageFeatPreferencesTableItem item)
        {
            var index = Data.Items.IndexOf(item);
            if (index > 0)
                Data.Items.Move(index, index - 1);
        }

        private void MoveDown(PackageFeatPreferencesTableItem item)
        {
            var index = Data.Items.IndexOf(item);
            if (index < Data.Items.Count - 1)
                Data.Items.Move(index, index + 1);
        }

        public ReactiveCommand<PackageFeatPreferencesTableItem, Unit> DeleteFeatPreferenceItemCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> AddFeatPreferenceItemCommand { get; private set; }

        public ReactiveCommand<PackageFeatPreferencesTableItem, Unit> MoveFeatPrefUpCommand { get; private set; }
        public ReactiveCommand<PackageFeatPreferencesTableItem, Unit> MoveFeatPrefDownCommand { get; private set; }
    }
}
