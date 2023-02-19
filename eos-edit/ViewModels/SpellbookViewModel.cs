using Eos.Models;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Eos.ViewModels
{
    internal class SpellbookViewModel : DataDetailViewModel<Spellbook>
    {
        public SpellbookViewModel() : base()
        {
            AddSpellCommand = new DelegateCommand<int?>(AddSpell);
            DeleteSpellCommand = new DelegateCommand<SpellbookEntry>(DeleteSpell);
        }

        public SpellbookViewModel(Spellbook spellbook) : base(spellbook)
        {
            AddSpellCommand = new DelegateCommand<int?>(AddSpell);
            DeleteSpellCommand = new DelegateCommand<SpellbookEntry>(DeleteSpell);
        }

        protected override string GetHeader()
        {
            return Data.Name + " (Spellbook)";
        }

        protected override Brush GetEntityColor()
        {
            return new SolidColorBrush(Color.FromArgb(100, 116, 72, 105));
        }

        private void AddSpell(int? level)
        {
            if (level != null)
                Data.AddSpell(level ?? 0, new SpellbookEntry(Data));
        }

        private void DeleteSpell(SpellbookEntry entry)
        {
            Data.RemoveSpell(entry);
        }

        public DelegateCommand<int?> AddSpellCommand { get; private set; }
        public DelegateCommand<SpellbookEntry> DeleteSpellCommand { get; private set; }
    }
}
