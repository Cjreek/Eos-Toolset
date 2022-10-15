using Eos.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using static Eos.Models.JsonUtils;

namespace Eos.Models.Tables
{
    public class BaseTable<T> : BaseModel where T : TableItem, new()
    {
        private Repository<T> _items = new Repository<T>();
        public string Name { get; set; } = "";
        
        public Repository<T> Items => _items;

        public T? this[int index]
        {
            get
            {
                if ((index < _items.Count) && (index >= 0))
                    return _items[index];
                throw new IndexOutOfRangeException();
            }

            set
            {
                if ((index < _items.Count) && (index >= 0))
                    _items[index] = value;
                throw new IndexOutOfRangeException();
            }
        }

        public int Count => _items.Count;

        public BaseTable()
        {
            InitializeData();
            this._items.CollectionChanged += Items_CollectionChanged;
        }

        ~BaseTable()
        {
            this._items.CollectionChanged -= Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Changed();
        }

        protected virtual void InitializeData()
        {

        }

        protected virtual int GetMaximumItems()
        {
            return int.MaxValue;
        }

        public void Add(T item)
        {
            if (Count < GetMaximumItems())
            {
                item.ParentTable = this;
                _items.Add(item);
            }
        }

        public void Remove(T item)
        {
            item.ParentTable = null;
            _items.Remove(item);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public override void ResolveReferences()
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                var item = this[i];
                if (item != null)
                {
                    item.ResolveReferences();
                    if (!item.IsValid()) Remove(item);
                }
            }
        }

        public override void FromJson(JsonObject json)
        {
            Clear();

            this.ID = ParseGuid(json["ID"]?.GetValue<String>());
            this.Name = json["Name"]?.GetValue<String>() ?? "";
            var itemArr = json["Items"]?.AsArray();
            if (itemArr != null)
            {
                foreach (var jsonItemValue in itemArr)
                {
                    if (jsonItemValue is JsonObject jsonItem)
                    {
                        var item = new T();
                        item.ParentTable = this;
                        item.FromJson(jsonItem);
                        Add(item);
                    }
                }
            }
        }

        public override JsonObject ToJson()
        {
            var tableJson = new JsonObject();
            tableJson.Add("ID", this.ID.ToString());
            tableJson.Add("Name", this.Name);

            var itemArr = new JsonArray();
            for (int i = 0; i < Count; i++)
            {
                var item = this[i];
                if (item != null)
                    itemArr.Add(item.ToJson());
            }
            tableJson.Add("Items", itemArr);

            return tableJson;
        }

        protected virtual void Changed()
        {

        }

        protected override String GetLabel()
        {
            return Name;
        }
    }
}
