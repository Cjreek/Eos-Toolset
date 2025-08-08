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
    public class PackageSkillPreferencesTableViewModel : DataDetailViewModel<PackageSkillPreferencesTable>
    {
        public PackageSkillPreferencesTableViewModel() : base()
        {
            DeleteSkillPreferenceItemCommand = ReactiveCommand.Create<PackageSkillPreferencesTableItem>(DeleteSkillPreferenceItem);
            AddSkillPreferenceItemCommand = ReactiveCommand.Create(AddSkillPreferenceItem);

            MoveSkillPrefUpCommand = ReactiveCommand.Create<PackageSkillPreferencesTableItem>(MoveUp);
            MoveSkillPrefDownCommand = ReactiveCommand.Create<PackageSkillPreferencesTableItem>(MoveDown);
        }

        public PackageSkillPreferencesTableViewModel(PackageSkillPreferencesTable skillPreferencesTable) : base(skillPreferencesTable)
        {
            DeleteSkillPreferenceItemCommand = ReactiveCommand.Create<PackageSkillPreferencesTableItem>(DeleteSkillPreferenceItem);
            AddSkillPreferenceItemCommand = ReactiveCommand.Create(AddSkillPreferenceItem);

            MoveSkillPrefUpCommand = ReactiveCommand.Create<PackageSkillPreferencesTableItem>(MoveUp);
            MoveSkillPrefDownCommand = ReactiveCommand.Create<PackageSkillPreferencesTableItem>(MoveDown);
        }

        protected override ISolidColorBrush GetEntityColor()
        {
            return base.GetEntityColor();
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }

        private void AddSkillPreferenceItem()
        {
            var newItem = new PackageSkillPreferencesTableItem();
            Data.Add(newItem);
            NotifyPropertyChanged("Data");
        }

        private void DeleteSkillPreferenceItem(PackageSkillPreferencesTableItem item)
        {
            this.Data.Remove(item);
            NotifyPropertyChanged("Data");
        }

        private void MoveUp(PackageSkillPreferencesTableItem item)
        {
            Data.MoveUp(item);
        }

        private void MoveDown(PackageSkillPreferencesTableItem item)
        {
            Data.MoveDown(item);
        }

        public ReactiveCommand<PackageSkillPreferencesTableItem, Unit> DeleteSkillPreferenceItemCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> AddSkillPreferenceItemCommand { get; private set; }

        public ReactiveCommand<PackageSkillPreferencesTableItem, Unit> MoveSkillPrefUpCommand { get; private set; }
        public ReactiveCommand<PackageSkillPreferencesTableItem, Unit> MoveSkillPrefDownCommand { get; private set; }
    }
}
