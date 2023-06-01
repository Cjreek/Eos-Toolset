using Eos.Nwn.Tlk;
using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

using static Eos.Models.JsonUtils;

namespace Eos.Models.Tables
{
    public class ItemPropertyTableItem : TableItem
    {
        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public CustomValueInstance CustomColumnValue01 { get; set; }
        public CustomValueInstance CustomColumnValue02 { get; set; }
        public CustomValueInstance CustomColumnValue03 { get; set; }
        public CustomValueInstance CustomColumnValue04 { get; set; }
        public CustomValueInstance CustomColumnValue05 { get; set; }
        public CustomValueInstance CustomColumnValue06 { get; set; }
        public CustomValueInstance CustomColumnValue07 { get; set; }
        public CustomValueInstance CustomColumnValue08 { get; set; }
        public CustomValueInstance CustomColumnValue09 { get; set; }
        public CustomValueInstance CustomColumnValue10 { get; set; }

        public ItemPropertyTableItem() : base() 
        {
            CustomColumnValue01 = new CustomValueInstance(new CustomObjectProperty());
            CustomColumnValue02 = new CustomValueInstance(new CustomObjectProperty());
            CustomColumnValue03 = new CustomValueInstance(new CustomObjectProperty());
            CustomColumnValue04 = new CustomValueInstance(new CustomObjectProperty());
            CustomColumnValue05 = new CustomValueInstance(new CustomObjectProperty());
            CustomColumnValue06 = new CustomValueInstance(new CustomObjectProperty());
            CustomColumnValue07 = new CustomValueInstance(new CustomObjectProperty());
            CustomColumnValue08 = new CustomValueInstance(new CustomObjectProperty());
            CustomColumnValue09 = new CustomValueInstance(new CustomObjectProperty());
            CustomColumnValue10 = new CustomValueInstance(new CustomObjectProperty());
        }

        public ItemPropertyTableItem(ItemPropertyTable parentTable) : base(parentTable)
        {
            CustomColumnValue01 = new CustomValueInstance(parentTable.CustomColumn01);
            CustomColumnValue02 = new CustomValueInstance(parentTable.CustomColumn02);
            CustomColumnValue03 = new CustomValueInstance(parentTable.CustomColumn03);
            CustomColumnValue04 = new CustomValueInstance(parentTable.CustomColumn04);
            CustomColumnValue05 = new CustomValueInstance(parentTable.CustomColumn05);
            CustomColumnValue06 = new CustomValueInstance(parentTable.CustomColumn06);
            CustomColumnValue07 = new CustomValueInstance(parentTable.CustomColumn07);
            CustomColumnValue08 = new CustomValueInstance(parentTable.CustomColumn08);
            CustomColumnValue09 = new CustomValueInstance(parentTable.CustomColumn09);
            CustomColumnValue10 = new CustomValueInstance(parentTable.CustomColumn10);

            if (parentTable.CustomColumn01.DataType?.Type == typeof(TLKStringSet)) CustomColumnValue01.Value = new TLKStringSet();
            if (parentTable.CustomColumn02.DataType?.Type == typeof(TLKStringSet)) CustomColumnValue02.Value = new TLKStringSet();
            if (parentTable.CustomColumn03.DataType?.Type == typeof(TLKStringSet)) CustomColumnValue03.Value = new TLKStringSet();
            if (parentTable.CustomColumn04.DataType?.Type == typeof(TLKStringSet)) CustomColumnValue04.Value = new TLKStringSet();
            if (parentTable.CustomColumn05.DataType?.Type == typeof(TLKStringSet)) CustomColumnValue05.Value = new TLKStringSet();
            if (parentTable.CustomColumn06.DataType?.Type == typeof(TLKStringSet)) CustomColumnValue06.Value = new TLKStringSet();
            if (parentTable.CustomColumn07.DataType?.Type == typeof(TLKStringSet)) CustomColumnValue07.Value = new TLKStringSet();
            if (parentTable.CustomColumn08.DataType?.Type == typeof(TLKStringSet)) CustomColumnValue08.Value = new TLKStringSet();
            if (parentTable.CustomColumn09.DataType?.Type == typeof(TLKStringSet)) CustomColumnValue09.Value = new TLKStringSet();
            if (parentTable.CustomColumn10.DataType?.Type == typeof(TLKStringSet)) CustomColumnValue10.Value = new TLKStringSet();
        }

        private object? ResolveValue(object? value, CustomObjectProperty prop)
        {
            if ((value is VariantValue varValue) && (varValue.Value is BaseModel varModel))
            {
                if ((varModel is CustomObjectInstance coInstance) && (varValue.DataType?.CustomType is CustomObject template))
                    varValue.Value = MasterRepository.Project.CustomObjectRepositories[template].GetByID(coInstance.ID);
                else
                    varValue.Value = MasterRepository.Standard.GetByID(varModel.GetType(), varModel.ID) ?? MasterRepository.Project.GetByID(varModel.GetType(), varModel.ID);
            }
            else if (value is BaseModel model)
            {
                if ((model is CustomObjectInstance coInstance) && (prop.DataType?.CustomType is CustomObject template))
                    return MasterRepository.Project.CustomObjectRepositories[template].GetByID(coInstance.ID);
                else
                    return MasterRepository.Standard.GetByID(model.GetType(), model.ID) ?? MasterRepository.Project.GetByID(model.GetType(), model.ID);
            }

            return value;
        }

        public override void ResolveReferences()
        {
            CustomColumnValue01.Value = ResolveValue(CustomColumnValue01.Value, CustomColumnValue01.Property);
            CustomColumnValue02.Value = ResolveValue(CustomColumnValue02.Value, CustomColumnValue02.Property);
            CustomColumnValue03.Value = ResolveValue(CustomColumnValue03.Value, CustomColumnValue03.Property);
            CustomColumnValue04.Value = ResolveValue(CustomColumnValue04.Value, CustomColumnValue04.Property);
            CustomColumnValue05.Value = ResolveValue(CustomColumnValue05.Value, CustomColumnValue05.Property);
            CustomColumnValue06.Value = ResolveValue(CustomColumnValue06.Value, CustomColumnValue06.Property);
            CustomColumnValue07.Value = ResolveValue(CustomColumnValue07.Value, CustomColumnValue07.Property);
            CustomColumnValue08.Value = ResolveValue(CustomColumnValue08.Value, CustomColumnValue08.Property);
            CustomColumnValue09.Value = ResolveValue(CustomColumnValue09.Value, CustomColumnValue09.Property);
            CustomColumnValue10.Value = ResolveValue(CustomColumnValue10.Value, CustomColumnValue10.Property);
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name.FromJson(json["Name"]?.AsObject());
            this.CustomColumnValue01.Value = CustomColumnValue01?.Property.ValueFromJson(json["CustomColumnValue01"]);
            this.CustomColumnValue02.Value = CustomColumnValue02?.Property.ValueFromJson(json["CustomColumnValue02"]);
            this.CustomColumnValue03.Value = CustomColumnValue03?.Property.ValueFromJson(json["CustomColumnValue03"]);
            this.CustomColumnValue04.Value = CustomColumnValue04?.Property.ValueFromJson(json["CustomColumnValue04"]);
            this.CustomColumnValue05.Value = CustomColumnValue05?.Property.ValueFromJson(json["CustomColumnValue05"]);
            this.CustomColumnValue06.Value = CustomColumnValue06?.Property.ValueFromJson(json["CustomColumnValue06"]);
            this.CustomColumnValue07.Value = CustomColumnValue07?.Property.ValueFromJson(json["CustomColumnValue07"]);
            this.CustomColumnValue08.Value = CustomColumnValue08?.Property.ValueFromJson(json["CustomColumnValue08"]);
            this.CustomColumnValue09.Value = CustomColumnValue09?.Property.ValueFromJson(json["CustomColumnValue09"]);
            this.CustomColumnValue10.Value = CustomColumnValue10?.Property.ValueFromJson(json["CustomColumnValue10"]);
        }

        public override JsonObject ToJson()
        {
            var json = base.ToJson();
            json.Add("Name", this.Name.ToJson());
            json.Add("CustomColumnValue01", CustomColumnValue01.Property.ValueToJson(CustomColumnValue01.Value));
            json.Add("CustomColumnValue02", CustomColumnValue02.Property.ValueToJson(CustomColumnValue02.Value));
            json.Add("CustomColumnValue03", CustomColumnValue03.Property.ValueToJson(CustomColumnValue03.Value));
            json.Add("CustomColumnValue04", CustomColumnValue04.Property.ValueToJson(CustomColumnValue04.Value));
            json.Add("CustomColumnValue05", CustomColumnValue05.Property.ValueToJson(CustomColumnValue05.Value));
            json.Add("CustomColumnValue06", CustomColumnValue06.Property.ValueToJson(CustomColumnValue06.Value));
            json.Add("CustomColumnValue07", CustomColumnValue07.Property.ValueToJson(CustomColumnValue07.Value));
            json.Add("CustomColumnValue08", CustomColumnValue08.Property.ValueToJson(CustomColumnValue08.Value));
            json.Add("CustomColumnValue09", CustomColumnValue09.Property.ValueToJson(CustomColumnValue09.Value));
            json.Add("CustomColumnValue10", CustomColumnValue10.Property.ValueToJson(CustomColumnValue10.Value));

            return json;
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
