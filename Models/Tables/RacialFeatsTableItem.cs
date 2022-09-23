using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using static Eos.Models.JsonUtils;

namespace Eos.Models.Tables
{
    public class RacialFeatsTableItem : TableItem
    {
        public Feat? Feat { get; set; }

        public override void ResolveReferences()
        {
            Feat = BaseModel.Resolve(Feat, MasterRepository.Feats);
        }

        public override void FromJson(JsonObject json)
        {
            this.Feat = CreateRefFromJson<Feat>(json["Feat"]?.AsObject());
        }

        public override JsonObject ToJson()
        {
            var json = new JsonObject();
            json.Add("Feat", this.Feat?.ToJsonRef());

            return json;
        }
    }
}
