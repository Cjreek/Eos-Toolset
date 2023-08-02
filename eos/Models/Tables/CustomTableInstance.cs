using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Eos.Repositories;

using static Eos.Models.JsonUtils;

namespace Eos.Models.Tables
{
    public class CustomTableInstance : BaseTable<CustomTableInstanceItem>
    {
        private CustomTable? _template;

        public CustomTable? Template
        {
            get { return _template; }
            set
            {
                Set(ref _template, value);

                foreach (var item in Items)
                {
                    if (item == null) continue;
                    item.Template = _template;
                }
            }
        }

        protected override void SetDefaultValues()
        {
            Name = "NEW_TABLE";
        }

        protected override void OnItemAdd(CustomTableInstanceItem item)
        {
            if (item.Template == null)
                item.Template = Template;
        }

        public override void FromJson(JsonObject json)
        {
            this.Name = json["Label"]?.GetValue<String>() ?? "";
            this.Template = MasterRepository.CustomTables.GetByID(ParseGuid(json["Template"]?["ID"]?.GetValue<String>()));

            base.FromJson(json);
        }

        public override JsonObject ToJson()
        {
            var customTableJson = base.ToJson();
            customTableJson.Add("Label", this.Name);
            customTableJson.Add("Template", CreateJsonRef(this.Template));

            return customTableJson;
        }
    }
}
