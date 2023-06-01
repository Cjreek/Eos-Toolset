using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class SpellbookRepository : ModelRepository<Spellbook>
    {
        private Dictionary<string, Spellbook> _spellbookNameLookup = new Dictionary<string, Spellbook>();

        public SpellbookRepository(bool isReadonly) : base(isReadonly)
        {

        }

        public bool Contains(string name)
        {
            return _spellbookNameLookup.ContainsKey(name);
        }

        public Spellbook? GetByName(string name)
        {
            if (!_spellbookNameLookup.TryGetValue(name, out Spellbook? spellbook))
                return null;
            return spellbook;
        }

        public override void Add(Spellbook? model)
        {
            if (model == null) return;

            if (!_spellbookNameLookup.ContainsKey(model.Name))
            {
                base.Add(model);
                _spellbookNameLookup.Add(model.Name, model);
            }
        }

        public override void Remove(Spellbook item)
        {
            base.Remove(item);
            _spellbookNameLookup.Remove(item.Name);
        }

        public override void Clear()
        {
            base.Clear();
            _spellbookNameLookup.Clear();
        }
    }
}
