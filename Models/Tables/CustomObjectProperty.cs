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
    public class CustomObjectProperty : TableItem
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public String Label { get; set; } = "";
        public String Column { get; set; } = "";
        public DataTypeDefinition? DataType { get; set; }

        public override void ResolveReferences()
        {
            DataType = MasterRepository.GetDataTypeById(DataType?.ID ?? Guid.Empty);
        }

        public override void FromJson(JsonObject json)
        {
            this.ID = ParseGuid(json["ID"]?.GetValue<String>());
            this.Label = json["Label"]?.GetValue<String>() ?? "";
            this.Column = json["Column"]?.GetValue<String>() ?? "";
            this.DataType = new DataTypeDefinition(ParseGuid(json["DataType"]?.GetValue<String>()), String.Empty, null);
        }

        public override JsonObject ToJson()
        {
            var json = new JsonObject();
            json.Add("ID", this.ID.ToString());
            json.Add("Label", this.Label);
            json.Add("Column", this.Column);
            json.Add("DataType", this.DataType?.ID.ToString());

            return json;
        }

        public JsonNode? ValueToJson(object? value)
        {
            if (DataType?.ToJson != null)
                return DataType?.ToJson(value);
            return null;
        }

        public object? ValueFromJson(JsonNode? node)
        {
            if (DataType?.FromJson != null)
                return DataType.FromJson(node);
            return null;
        }
    }
}
