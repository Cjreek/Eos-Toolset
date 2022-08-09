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

namespace Eos.Models.Base
{
    public class Repository<T> : INotifyCollectionChanged, INotifyPropertyChanged, IReadOnlyCollection<T?>, IReadOnlyList<T?>, IEnumerable<T?> where T : new()
    {
        private ObservableCollection<T?> internalList = new ObservableCollection<T?>();

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
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

        public event PropertyChangedEventHandler? PropertyChanged;

        public IEnumerator<T?> GetEnumerator()
        {
            return internalList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return internalList.GetEnumerator();
        }

        public int Count => internalList.Count;
        public T? this[int index] => internalList[index];

        public bool Contains(T model)
        {
            return internalList.Contains(model);
        }

        public virtual void Add(T model)
        {
            internalList.Add(model);
        }

        
    }
}
