using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Eos.Repositories
{
    public class Repository<T> : INotifyCollectionChanged, INotifyPropertyChanged, IReadOnlyCollection<T?>, IReadOnlyList<T?>, IEnumerable<T?> where T : new()
    {
        protected ObservableCollection<T?> internalList = new ObservableCollection<T?>();
        private bool fireChangedEvent = true;

        public Repository()
        {
            CollectionChanged += InternalListChanged;
        }

        protected void RaisePropertyChanged(string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add
            {
                internalList.CollectionChanged += value;
            }
            remove
            {
                internalList.CollectionChanged -= value;
            }
        }

        private void InternalListChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (fireChangedEvent)
                Changed();
        }

        protected virtual void Changed()
        {

        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public IEnumerator<T?> GetEnumerator()
        {
            return internalList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return internalList.GetEnumerator();
        }

        public void BeginUpdate()
        {
            fireChangedEvent = false;
        }

        public void EndUpdate()
        {
            fireChangedEvent = true;
            Changed();
        }

        public int Count => internalList.Count;
        public T? this[int index]
        {
            get { return internalList[index]; }
            set { internalList[index] = value; }
        }

        public bool Contains(T model)
        {
            return internalList.Contains(model);
        }

        public virtual void Add(T model)
        {
            internalList.Add(model);
        }

        public virtual void Clear()
        {
            internalList.Clear();
        }

        public void Sort<U>(Func<T?, U> compareFunc)
        {
            var list = internalList.OrderBy(compareFunc).ToList();

            internalList.Clear();
            foreach (var item in list)
                internalList.Add(item);
        }
    }
}
