using Avalonia.Media;
using Eos.Models;
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
    public class SpellbookViewModel : DataDetailViewModel<Spellbook>
    {
        public SpellbookViewModel() : base()
        {
            AddSpellCommand = ReactiveCommand.Create<int?>(AddSpell);
            DeleteSpellCommand = ReactiveCommand.Create<SpellbookEntry>(DeleteSpell);
        }

        public SpellbookViewModel(Spellbook spellbook) : base(spellbook)
        {
            AddSpellCommand = ReactiveCommand.Create<int?>(AddSpell);
            DeleteSpellCommand = ReactiveCommand.Create<SpellbookEntry>(DeleteSpell);
        }

        protected override string GetHeader()
        {
            return Data.Name + " (Spellbook)";
        }

        protected override ISolidColorBrush GetEntityColor()
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

        public ReactiveCommand<int?, Unit> AddSpellCommand { get; private set; }
        public ReactiveCommand<SpellbookEntry, Unit> DeleteSpellCommand { get; private set; }
    }
}
