using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories
{
    public class SpellRepository : ModelRepository<Spell>
    {
        public SpellRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public IEnumerable<Spell?> CreaturePowers => this.Where(spell => spell?.Type == SpellType.CreaturePower);
        public IEnumerable<Spell?> FeatSpells => this.Where(spell => spell?.Type == SpellType.Feat);
        public IEnumerable<Spell?> OtherSpells => this.Where(spell => spell?.Type == SpellType.Other);

        public IEnumerable<Spell?> GeneralSpells => this.Where(spell => (spell?.Type == SpellType.Spell) && (spell?.School == SpellSchool.G));
        public IEnumerable<Spell?> AbjurationSpells => this.Where(spell => (spell?.Type == SpellType.Spell) && (spell?.School == SpellSchool.A));
        public IEnumerable<Spell?> ConjurationSpells => this.Where(spell => (spell?.Type == SpellType.Spell) && (spell?.School == SpellSchool.C));
        public IEnumerable<Spell?> DivinationSpells => this.Where(spell => (spell?.Type == SpellType.Spell) && (spell?.School == SpellSchool.D));
        public IEnumerable<Spell?> EnchantmentSpells => this.Where(spell => (spell?.Type == SpellType.Spell) && (spell?.School == SpellSchool.E));
        public IEnumerable<Spell?> EvocationSpells => this.Where(spell => (spell?.Type == SpellType.Spell) && (spell?.School == SpellSchool.V));
        public IEnumerable<Spell?> IllusionSpells => this.Where(spell => (spell?.Type == SpellType.Spell) && (spell?.School == SpellSchool.I));
        public IEnumerable<Spell?> NecromancySpells => this.Where(spell => (spell?.Type == SpellType.Spell) && (spell?.School == SpellSchool.N));
        public IEnumerable<Spell?> TransmutationSpells => this.Where(spell => (spell?.Type == SpellType.Spell) && (spell?.School == SpellSchool.T));

        protected override void Changed()
        {
            RaisePropertyChanged("CreaturePowers");
            RaisePropertyChanged("FeatSpells");
            RaisePropertyChanged("OtherSpells");

            RaisePropertyChanged("GeneralSpells");
            RaisePropertyChanged("AbjurationSpells");
            RaisePropertyChanged("ConjurationSpells");
            RaisePropertyChanged("DivinationSpells");
            RaisePropertyChanged("EnchantmentSpells");
            RaisePropertyChanged("EvocationSpells");
            RaisePropertyChanged("IllusionSpells");
            RaisePropertyChanged("NecromancySpells");
            RaisePropertyChanged("TransmutationSpells");
        }
    }
}
