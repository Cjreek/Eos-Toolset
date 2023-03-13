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
    internal class SkillTableViewModel : DataDetailViewModel<SkillsTable>
    {
        public SkillTableViewModel() : base()
        {
            DeleteSkillItemCommand = ReactiveCommand.Create<SkillsTableItem>(DeleteSkillItem);
            AddSkillItemCommand = ReactiveCommand.Create(AddSkillItem);
        }

        public SkillTableViewModel(SkillsTable skillTable) : base(skillTable)
        {
            DeleteSkillItemCommand = ReactiveCommand.Create<SkillsTableItem>(DeleteSkillItem);
            AddSkillItemCommand = ReactiveCommand.Create(AddSkillItem);
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }

        private void AddSkillItem()
        {
            var newItem = new SkillsTableItem();
            Data.Add(newItem);
            NotifyPropertyChanged("Data");
        }

        private void DeleteSkillItem(SkillsTableItem item)
        {
            this.Data.Remove(item);
            NotifyPropertyChanged("Data");
        }

        public ReactiveCommand<SkillsTableItem, Unit> DeleteSkillItemCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> AddSkillItemCommand { get; private set; }
    }
}
