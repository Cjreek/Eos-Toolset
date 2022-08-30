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
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class BaseTable<T> : BaseModel where T : TableItem, new()
    {
        private Repository<T> _items = new Repository<T>();
        public string Name { get; set; } = "";
        
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
                _items.Add(item);
        }

        public void Clear()
        {
            _items.Clear();
        }
    }
}
