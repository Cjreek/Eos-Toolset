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
    internal class PackageSpellPreferencesTableViewModel : DataDetailViewModel<PackageSpellPreferencesTable>
    {
        public PackageSpellPreferencesTableViewModel() : base()
        {
            DeleteSpellPreferenceItemCommand = ReactiveCommand.Create<PackageSpellPreferencesTableItem>(DeleteSpellPreferenceItem);
            AddSpellPreferenceItemCommand = ReactiveCommand.Create(AddSpellPreferenceItem);

            MoveSpellPrefUpCommand = ReactiveCommand.Create<PackageSpellPreferencesTableItem>(MoveUp);
            MoveSpellPrefDownCommand = ReactiveCommand.Create<PackageSpellPreferencesTableItem>(MoveDown);
        }

        public PackageSpellPreferencesTableViewModel(PackageSpellPreferencesTable spellPreferencesTable) : base(spellPreferencesTable)
        {
            DeleteSpellPreferenceItemCommand = ReactiveCommand.Create<PackageSpellPreferencesTableItem>(DeleteSpellPreferenceItem);
            AddSpellPreferenceItemCommand = ReactiveCommand.Create(AddSpellPreferenceItem);

            MoveSpellPrefUpCommand = ReactiveCommand.Create<PackageSpellPreferencesTableItem>(MoveUp);
            MoveSpellPrefDownCommand = ReactiveCommand.Create<PackageSpellPreferencesTableItem>(MoveDown);
        }

        protected override ISolidColorBrush GetEntityColor()
        {
            return base.GetEntityColor();
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }

        private void AddSpellPreferenceItem()
        {
            var newItem = new PackageSpellPreferencesTableItem();
            Data.Add(newItem);
            NotifyPropertyChanged("Data");
        }

        private void DeleteSpellPreferenceItem(PackageSpellPreferencesTableItem item)
        {
            this.Data.Remove(item);
            NotifyPropertyChanged("Data");
        }

        private void MoveUp(PackageSpellPreferencesTableItem item)
        {
            var index = Data.Items.IndexOf(item);
            if (index > 0)
                Data.Items.Move(index, index - 1);
        }

        private void MoveDown(PackageSpellPreferencesTableItem item)
        {
            var index = Data.Items.IndexOf(item);
            if (index < Data.Items.Count - 1)
                Data.Items.Move(index, index + 1);
        }

        public ReactiveCommand<PackageSpellPreferencesTableItem, Unit> DeleteSpellPreferenceItemCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> AddSpellPreferenceItemCommand { get; private set; }
        public ReactiveCommand<PackageSpellPreferencesTableItem, Unit> MoveSpellPrefUpCommand { get; private set; }
        public ReactiveCommand<PackageSpellPreferencesTableItem, Unit> MoveSpellPrefDownCommand { get; private set; }
    }
}
