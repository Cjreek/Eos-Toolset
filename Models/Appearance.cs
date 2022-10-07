﻿using Eos.Nwn.Tlk;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using static Eos.Models.JsonUtils;

namespace Eos.Models
{
    public class Appearance : BaseModel
    {
        public TLKStringSet Name { get; set; } = new TLKStringSet();

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name.FromJson(json["Name"]?.AsObject());
        }

        public override JsonObject ToJson()
        {
            var appearanceJson = base.ToJson();
            appearanceJson.Add("Name", this.Name.ToJson());

            return appearanceJson;
        }
    }
}
