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
using Eos.Models;

namespace Eos.Repositories
{
    public class Repository<T> : INotifyCollectionChanged, INotifyPropertyChanged, IReadOnlyCollection<T?>, IReadOnlyList<T?>, IEnumerable<T?> where T : INotifyPropertyChanged, new()
    {
        protected ObservableCollection<T?> internalList = new ObservableCollection<T?>();
        private bool fireChangedEvent = true;

        public Repository()
        {
            internalList.CollectionChanged += InternalListChanged;
        }

        protected void RaisePropertyChanged(string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        private void InternalListChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var items = e.OldItems;
                if (items != null)
                {
                    foreach (var item in items.Cast<INotifyPropertyChanged>())
                        item.PropertyChanged -= Item_PropertyChanged;
                }
            }
            else
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var items = e.NewItems;
                if (items != null)
                {
                    foreach (var item in items.Cast<INotifyPropertyChanged>())
                        item.PropertyChanged += Item_PropertyChanged;
                }
            }
            else
            if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                var oldItems = e.OldItems;
                if (oldItems != null)
                {
                    foreach (var item in oldItems.Cast<INotifyPropertyChanged>())
                        item.PropertyChanged -= Item_PropertyChanged;
                }

                var newItems = e.NewItems;
                if (newItems != null)
                {
                    foreach (var item in newItems.Cast<INotifyPropertyChanged>())
                        item.PropertyChanged += Item_PropertyChanged;
                }
            }

            if (fireChangedEvent)
                RaiseCollectionChanged(e);
        }

        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, e);
            Changed();
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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

        public virtual void Remove(T item)
        {
            internalList.Remove(item);
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
