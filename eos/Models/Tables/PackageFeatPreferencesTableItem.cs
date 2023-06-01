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
    public class PackageFeatPreferencesTableItem : TableItem
    {
        private Feat? _feat;

        public Feat? Feat
        {
            get { return _feat; }
            set { Set(ref _feat, value); }
        }

        public PackageFeatPreferencesTableItem() : base()
        {
        }

        public PackageFeatPreferencesTableItem(PackageFeatPreferencesTable parentTable) : base(parentTable)
        {
        }

        public override void ResolveReferences()
        {
            Feat = MasterRepository.Feats.Resolve(Feat);
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Feat = CreateRefFromJson<Feat>(json["Feat"]?.AsObject());
        }

        public override JsonObject ToJson()
        {
            var json = base.ToJson();
            json.Add("Feat", this.Feat?.ToJsonRef());

            return json;
        }

        public override bool IsValid()
        {
            return (Feat != null);
        }
    }
}
