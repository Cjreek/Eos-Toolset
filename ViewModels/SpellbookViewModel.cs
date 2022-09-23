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
            {
                Data.AddSpell(level ?? 0, new SpellbookEntry());
            }
        }

        private void DeleteSpell(SpellbookEntry entry)
        {
            Data.Level0.Remove(entry);
            Data.Level1.Remove(entry);
            Data.Level2.Remove(entry);
            Data.Level3.Remove(entry);
            Data.Level4.Remove(entry);
            Data.Level5.Remove(entry);
            Data.Level6.Remove(entry);
            Data.Level7.Remove(entry);
            Data.Level8.Remove(entry);
            Data.Level9.Remove(entry);
        }

        public DelegateCommand<int?> AddSpellCommand { get; private set; }
        public DelegateCommand<SpellbookEntry> DeleteSpellCommand { get; private set; }
    }
}
