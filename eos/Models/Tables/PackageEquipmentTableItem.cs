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
    public class PackageEquipmentTableItem : TableItem
    {
        public string BlueprintResRef { get; set; } = "";

        public PackageEquipmentTableItem() : base()
        {
        }

        public PackageEquipmentTableItem(PackageEquipmentTable parentTable) : base(parentTable)
        {
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.BlueprintResRef = json["BlueprintResRef"]?.GetValue<string>() ?? "";
        }

        public override JsonObject ToJson()
        {
            var json = base.ToJson();
            json.Add("BlueprintResRef", this.BlueprintResRef);

            return json;
        }

        public override bool IsValid()
        {
            return (BlueprintResRef != "");
        }
    }
}
