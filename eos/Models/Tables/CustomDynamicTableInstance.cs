using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using static Eos.Models.JsonUtils;

namespace Eos.Models.Tables
{
    public class CustomDynamicTableInstance : BaseTable<CustomDynamicTableInstanceItem>
    {
        private CustomDynamicTable? _template;

        public CustomObjectProperty CustomColumn01 { get; set; } = new CustomObjectProperty();
        public CustomObjectProperty CustomColumn02 { get; set; } = new CustomObjectProperty();
        public CustomObjectProperty CustomColumn03 { get; set; } = new CustomObjectProperty();
        public CustomObjectProperty CustomColumn04 { get; set; } = new CustomObjectProperty();
        public CustomObjectProperty CustomColumn05 { get; set; } = new CustomObjectProperty();
        public CustomObjectProperty CustomColumn06 { get; set; } = new CustomObjectProperty();
        public CustomObjectProperty CustomColumn07 { get; set; } = new CustomObjectProperty();
        public CustomObjectProperty CustomColumn08 { get; set; } = new CustomObjectProperty();
        public CustomObjectProperty CustomColumn09 { get; set; } = new CustomObjectProperty();
        public CustomObjectProperty CustomColumn10 { get; set; } = new CustomObjectProperty();

        public CustomDynamicTable? Template
        {
            get { return _template; }
            set { Set(ref _template, value); }
        }

        protected override string GetTypeName()
        {
            return "Dynamic Table";
        }

        protected override void SetDefaultValues()
        {
            Name = "DYN_NEWTABLE";
        }

        public override void ResolveReferences()
        {
            CustomColumn01.ResolveReferences();
            CustomColumn02.ResolveReferences();
            CustomColumn03.ResolveReferences();
            CustomColumn04.ResolveReferences();
            CustomColumn05.ResolveReferences();
            CustomColumn06.ResolveReferences();
            CustomColumn07.ResolveReferences();
            CustomColumn08.ResolveReferences();
            CustomColumn09.ResolveReferences();
            CustomColumn10.ResolveReferences();

            base.ResolveReferences();
        }

        public override JsonObject ToJson()
        {
            var json = base.ToJson();
            json.Add("Template", CreateJsonRef(this.Template));

            json.Add("CustomColumn01", CustomColumn01.ToJson());
            json.Add("CustomColumn02", CustomColumn02.ToJson());
            json.Add("CustomColumn03", CustomColumn03.ToJson());
            json.Add("CustomColumn04", CustomColumn04.ToJson());
            json.Add("CustomColumn05", CustomColumn05.ToJson());
            json.Add("CustomColumn06", CustomColumn06.ToJson());
            json.Add("CustomColumn07", CustomColumn07.ToJson());
            json.Add("CustomColumn08", CustomColumn08.ToJson());
            json.Add("CustomColumn09", CustomColumn09.ToJson());
            json.Add("CustomColumn10", CustomColumn10.ToJson());

            return json;
        }

        public override void FromJson(JsonObject json)
        {
            this.Template = MasterRepository.CustomDynamicTables.GetByID(ParseGuid(json["Template"]?["ID"]?.GetValue<String>()));

            var column = json["CustomColumn01"]?.AsObject();
            if (column != null) CustomColumn01.FromJson(column);
            column = json["CustomColumn02"]?.AsObject();
            if (column != null) CustomColumn02.FromJson(column);
            column = json["CustomColumn03"]?.AsObject();
            if (column != null) CustomColumn03.FromJson(column);
            column = json["CustomColumn04"]?.AsObject();
            if (column != null) CustomColumn04.FromJson(column);
            column = json["CustomColumn05"]?.AsObject();
            if (column != null) CustomColumn05.FromJson(column);
            column = json["CustomColumn06"]?.AsObject();
            if (column != null) CustomColumn06.FromJson(column);
            column = json["CustomColumn07"]?.AsObject();
            if (column != null) CustomColumn07.FromJson(column);
            column = json["CustomColumn08"]?.AsObject();
            if (column != null) CustomColumn08.FromJson(column);
            column = json["CustomColumn09"]?.AsObject();
            if (column != null) CustomColumn09.FromJson(column);
            column = json["CustomColumn10"]?.AsObject();
            if (column != null) CustomColumn10.FromJson(column);

            ResolveReferences();

            base.FromJson(json);
        }
    }
}
