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
        private String _label = "";
        private String _column = "";
        private DataTypeDefinition? _dataType;
        
        public Guid ID { get; set; } = Guid.NewGuid();

        public String Label
        {
            get { return _label; }
            set 
            { 
                if (_label != value) 
                {
                    _label = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String Column
        {
            get { return _column; }
            set
            {
                if (_column != value)
                {
                    _column = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public DataTypeDefinition? DataType
        {
            get { return _dataType; }
            set
            {
                if (_dataType != value)
                {
                    _dataType = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public CustomObjectProperty() : base()
        {
        }

        public CustomObjectProperty(CustomObject parentTable) : base(parentTable)
        {
        }

        public override void ResolveReferences()
        {
            DataType = MasterRepository.GetDataTypeById(DataType?.ID ?? Guid.Empty);
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.ID = ParseGuid(json["ID"]?.GetValue<String>());
            this.Label = json["Label"]?.GetValue<String>() ?? "";
            this.Column = json["Column"]?.GetValue<String>() ?? "";
            this.DataType = new DataTypeDefinition(ParseGuid(json["DataType"]?.GetValue<String>()), String.Empty, null);
        }

        public override JsonObject ToJson()
        {
            var json = base.ToJson();
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
