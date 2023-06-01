using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using static Eos.Models.JsonUtils;

namespace Eos.Models.Tables
{
    public class PackageSpellPreferencesTableItem : TableItem
    {
        private Spell? _spell;

        public Spell? Spell
        {
            get { return _spell; }
            set { Set(ref _spell, value); }
        }

        public PackageSpellPreferencesTableItem() : base()
        {
        }

        public PackageSpellPreferencesTableItem(PackageSpellPreferencesTable parentTable) : base(parentTable)
        {
        }

        public override void ResolveReferences()
        {
            Spell = MasterRepository.Spells.Resolve(Spell);
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Spell = CreateRefFromJson<Spell>(json["Spell"]?.AsObject());
        }

        public override JsonObject ToJson()
        {
            var json = base.ToJson();
            json.Add("Spell", this.Spell?.ToJsonRef());

            return json;
        }

        public override bool IsValid()
        {
            return (Spell != null);
        }
    }
}
