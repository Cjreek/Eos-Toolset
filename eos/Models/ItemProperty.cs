using Eos.Models.Tables;
using Eos.Nwn.Tlk;
using Eos.Repositories;
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
    public class ItemProperty : BaseModel
    {
        private ItemPropertyTable? _subType;
        private ItemPropertyCostTable? _costTable;
        private ItemPropertyParam? _param;
        private string _subTypeResRef = "";

        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public TLKStringSet PropertyText { get; set; } = new TLKStringSet();
        public TLKStringSet Description { get; set; } = new TLKStringSet();
        public double Cost { get; set; }

        public ItemPropertyTable? SubType
        {
            get { return _subType; }
            set 
            {
                var oldValue = _subType;

                Set(ref _subType, value);

                if (oldValue != _subType)
                {
                    if (_subType != null)
                        _subTypeResRef = _subType.Name;
                    else
                        _subTypeResRef = "";

                    NotifyPropertyChanged(nameof(SubTypeResRef));
                    NotifyPropertyChanged();
                }

            }
        }

        public string SubTypeResRef
        {
            get { return _subTypeResRef; }
            set
            {
                if ((_subType == null) && (_subTypeResRef != value))
                {
                    _subTypeResRef = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ItemPropertyCostTable? CostTable
        {
            get { return _costTable; }
            set { Set(ref _costTable, value); }
        }

        public ItemPropertyParam? Param
        {
            get { return _param; }
            set { Set(ref _param, value); }
        }

        protected override void Initialize()
        {
            base.Initialize();
            Name = new TLKStringSet(() => NotifyPropertyChanged(nameof(Name)));
        }

        protected override String GetTypeName()
        {
            return "Item Property";
        }

        protected override TLKStringSet? GetTlkDisplayName()
        {
            var modelOverride = (ItemProperty?)MasterRepository.Project.GetOverride(this);
            return modelOverride?.Name ?? this.Name;
        }

        public override String GetLabel()
        {
            return Name;
        }

        protected override void SetDefaultValues()
        {
            Name[MasterRepository.Project.DefaultLanguage].Text = "New Item Property";
            Name[MasterRepository.Project.DefaultLanguage].TextF = "New Item Property";
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            SubType = Resolve(SubType, MasterRepository.ItemPropertyTables);
            CostTable = Resolve(CostTable, MasterRepository.ItemPropertyCostTables);
            Param = Resolve(Param, MasterRepository.ItemPropertyParams);
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name.FromJson(json["Name"]?.AsObject());
            this.PropertyText.FromJson(json["PropertyText"]?.AsObject());
            this.Description.FromJson(json["Description"]?.AsObject());
            this.Cost = json["Cost"]?.GetValue<double>() ?? 1.00;
            this.SubType = CreateRefFromJson<ItemPropertyTable>(json["SubType"]?.AsObject());
            this.SubTypeResRef = json["SubTypeResRef"]?.GetValue<string>() ?? "";
            this.CostTable = CreateRefFromJson<ItemPropertyCostTable>(json["CostTable"]?.AsObject());
            this.Param = CreateRefFromJson<ItemPropertyParam>(json["Param"]?.AsObject());
        }

        public override JsonObject ToJson()
        {
            var itemPropertyJson = base.ToJson();
            itemPropertyJson.Add("Name", this.Name.ToJson());
            itemPropertyJson.Add("PropertyText", this.PropertyText.ToJson());
            itemPropertyJson.Add("Description", this.Description.ToJson());
            itemPropertyJson.Add("Cost", this.Cost);
            itemPropertyJson.Add("SubType", CreateJsonRef(this.SubType));
            itemPropertyJson.Add("SubTypeResRef", this.SubTypeResRef);
            itemPropertyJson.Add("CostTable", CreateJsonRef(this.CostTable));
            itemPropertyJson.Add("Param", CreateJsonRef(this.Param));

            return itemPropertyJson;
        }
    }
}
