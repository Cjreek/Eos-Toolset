using Eos.Models.Tables;
using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using static Eos.Models.JsonUtils;

namespace Eos.Models
{
    public class ItemPropertySetEntry
    {
        private ItemProperty? _itemProperty;
        private ItemPropertySet _itemPropertySet;

        public ItemProperty? ItemProperty
        {
            get { return _itemProperty; }
            set
            {
                if (_itemProperty != value)
                {
                    _itemProperty?.AddReference(_itemPropertySet, nameof(_itemPropertySet.ItemProperties));
                    _itemProperty = value;
                    _itemProperty?.RemoveReference(_itemPropertySet, nameof(_itemPropertySet.ItemProperties));
                }
            }
        }

        public ItemPropertySetEntry(ItemPropertySet itemPropertySet, ItemProperty? itemProperty = null)
        {
            this._itemPropertySet = itemPropertySet;
            this._itemProperty = itemProperty;
        }
    }

    public class ItemPropertySet : BaseModel
    {
        public string Name { get; set; } = "";
        public ObservableCollection<ItemPropertySetEntry> ItemProperties { get; } = new ObservableCollection<ItemPropertySetEntry>();

        public override String GetLabel()
        {
            return Name;
        }

        protected override string GetTypeName()
        {
            return "Itemproperty Set";
        }

        protected override void SetDefaultValues()
        {
            Name = "New Itemproperty Set";
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            for (int i=0; i < ItemProperties.Count; i++)
                ItemProperties[i].ItemProperty = Resolve(ItemProperties[i].ItemProperty, MasterRepository.ItemProperties);
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name = json["Name"]?.GetValue<String>() ?? "";

            ItemProperties.Clear();
            var jsonPropArr = json["ItemProperties"]?.AsArray();
            if (jsonPropArr != null)
            {
                for (int i = 0; i < jsonPropArr.Count; i++)
                {
                    var item = new ItemPropertySetEntry(this);
                    item.ItemProperty = CreateRefFromJson<ItemProperty>(jsonPropArr[i]?.AsObject());

                    ItemProperties.Add(item);
                }
            }
        }

        public override JsonObject ToJson()
        {
            var itemPropertySetJson = base.ToJson();
            itemPropertySetJson.Add("Name", this.Name);

            var jsonPropArr = new JsonArray();
            for (int i=0; i < ItemProperties.Count; i++)
                jsonPropArr.Add(CreateJsonRef(ItemProperties[i].ItemProperty));
            itemPropertySetJson.Add("ItemProperties", jsonPropArr);

            return itemPropertySetJson;
        }
    }
}
