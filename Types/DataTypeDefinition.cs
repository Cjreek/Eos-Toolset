using Eos.Models.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Eos.Types
{
    public delegate JsonNode? DataTypeToJsonDelegate(object? value);
    public delegate object? DataTypeFromJsonDelegate(JsonNode? json);
    public delegate object? DataTypeTo2DADelegate(object? value);

    public class DataTypeDefinition
    {
        public Guid ID { get; set; }
        public String Label { get; set; } = "";
        public bool IsCustom { get; set; }
        public Type? Type { get; set; }
        public object? CustomType { get; set; }
        public bool IsVisualOnly { get; set; } = false;

        public DataTypeToJsonDelegate? ToJson { get; set; } = null;
        public DataTypeFromJsonDelegate? FromJson { get; set; } = null;
        public DataTypeTo2DADelegate? To2DA { get; set; } = null;

        public DataTypeDefinition(Guid id, string label, object? type, bool isCustom = false)
        {
            ID = id;
            Label = label;
            IsCustom = isCustom;
            if (!isCustom && type is Type t)
                Type = t;
            else
                CustomType = type;
        }
    }
}
