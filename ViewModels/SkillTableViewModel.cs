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
    internal class SkillTableViewModel : DataDetailViewModel<SkillsTable>
    {
        public SkillTableViewModel() : base()
        {
            DeleteSkillItemCommand = new DelegateCommand<SkillsTableItem>(DeleteSkillItem);
            AddSkillItemCommand = new DelegateCommand(AddSkillItem);
        }

        public SkillTableViewModel(SkillsTable skillTable) : base(skillTable)
        {
            DeleteSkillItemCommand = new DelegateCommand<SkillsTableItem>(DeleteSkillItem);
            AddSkillItemCommand = new DelegateCommand(AddSkillItem);
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

        public DelegateCommand<SkillsTableItem> DeleteSkillItemCommand { get; private set; }
        public DelegateCommand AddSkillItemCommand { get; private set; }
    }
}
