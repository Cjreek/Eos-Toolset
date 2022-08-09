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

namespace Eos.Models.Base
{
    public class BaseTable<T> : Repository<T> where T : new()
    {
        public Guid ID { get; set; }
        public string Name { get; set; } = "";

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

        public override void Add(T model)
        {
            if (this.Count < GetMaximumItems())
                base.Add(model);
        }
    }
}
