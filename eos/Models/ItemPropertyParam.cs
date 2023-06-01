using Eos.Models.Tables;
using Eos.Nwn.Tlk;
using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using static Eos.Models.JsonUtils;

namespace Eos.Models
{
    public class ItemPropertyParam : BaseModel
    {
        private ItemPropertyTable? _itemPropertyTable;
        private string _tableResRef = "";

        public TLKStringSet Name { get; set; } = new TLKStringSet();

        public ItemPropertyTable? ItemPropertyTable
        {
            get { return _itemPropertyTable; }
            set 
            {
                var oldValue = _itemPropertyTable;

                Set(ref _itemPropertyTable, value);

                if (oldValue != _itemPropertyTable)
                {
                    if (_itemPropertyTable != null)
                        _tableResRef = _itemPropertyTable.Name;
                    else
                        _tableResRef = "";

                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(TableResRef));
                }
            }
        }

        public string TableResRef
        {
            get { return _tableResRef; }
            set
            {
                if ((_itemPropertyTable == null) && (_tableResRef != value))
                {
                    _tableResRef = value;
                    NotifyPropertyChanged();
                }
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
            Name = new TLKStringSet(() => NotifyPropertyChanged(nameof(Name)));
        }

        protected override String GetTypeName()
        {
            return "Item Property Param";
        }

        protected override TLKStringSet? GetTlkDisplayName()
        {
            var modelOverride = (ItemPropertyParam?)MasterRepository.Project.GetOverride(this);
            return modelOverride?.Name ?? this.Name;
        }

        public override String GetLabel()
        {
            return Name;
        }

        protected override void SetDefaultValues()
        {
            Name[MasterRepository.Project.DefaultLanguage].Text = "New Item Property Param";
            Name[MasterRepository.Project.DefaultLanguage].TextF = "New Item Property Param";
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            ItemPropertyTable = Resolve(ItemPropertyTable, MasterRepository.ItemPropertyTables);
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name.FromJson(json["Name"]?.AsObject());
            this.ItemPropertyTable = CreateRefFromJson<ItemPropertyTable>(json["ItemPropertyTable"]?.AsObject());
            this.TableResRef = json["TableResRef"]?.GetValue<string>() ?? "";
        }

        public override JsonObject ToJson()
        {
            var itemPropertyJson = base.ToJson();
            itemPropertyJson.Add("Name", this.Name.ToJson());
            itemPropertyJson.Add("ItemPropertyTable", CreateJsonRef(this.ItemPropertyTable));
            itemPropertyJson.Add("TableResRef", this.TableResRef);

            return itemPropertyJson;
        }
    }
}
