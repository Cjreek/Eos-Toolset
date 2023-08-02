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
using System.Reflection;
using Eos.Types;

namespace Eos.Repositories
{
    public class Repository<T> : INotifyCollectionChanged, INotifyPropertyChanged, IReadOnlyCollection<T?>, IReadOnlyList<T?>, IEnumerable<T?>, IList where T : INotifyPropertyChanged, new()
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
            // Works better without.. But why did I have it in in the first place? It used to fix *something*...
            //RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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

        public bool IsFixedSize => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public bool IsSynchronized => throw new NotImplementedException();

        public object SyncRoot => throw new NotImplementedException();

        object? IList.this[int index] { get => internalList[index]; set { if (value is T) internalList[index] = (T)value; } }

        public T? this[int index]
        {
            get { return internalList[index]; }
            set { internalList[index] = value; }
        }

        public int IndexOf(T? model)
        {
            return internalList.IndexOf(model);
        }

        public void Move(int fromIndex, int toIndex)
        {
            var element = internalList[fromIndex];
            BeginUpdate();
            try
            {
                internalList.RemoveAt(fromIndex);
                internalList.Insert(toIndex, element);
            }
            finally
            {
                EndUpdate();
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public bool Contains(T? model)
        {
            return internalList.Contains(model);
        }

        public virtual void Add(T? model)
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

        // IList // TODOX: Implement rest, // !

        public int Add(object? value)
        {
            throw new NotImplementedException();
        }

        public bool Contains(object? value)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(object? value)
        {
            if (value is T model)
                return internalList.IndexOf(model);
            return -1;
        }

        public void Insert(int index, object? value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object? value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }
    }
}
