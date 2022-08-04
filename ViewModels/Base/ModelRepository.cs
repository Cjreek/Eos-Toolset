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

namespace Eos.ViewModels.Base
{
    internal class ModelRepository<T> : INotifyCollectionChanged, INotifyPropertyChanged, IReadOnlyCollection<T?>, IReadOnlyList<T?>, IEnumerable<T?> where T : BaseModel, new()
    {
        private ObservableCollection<T?> internalList = new ObservableCollection<T?>();
        private Dictionary<Guid, T?> modelLookup = new Dictionary<Guid, T?>();

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

        public T? GetByID(Guid id)
        {
            modelLookup.TryGetValue(id, out T? result);
            return result;
        }

        public bool Contains(Guid id)
        {
            return modelLookup.ContainsKey(id);
        }

        public bool Contains(T model)
        {
            return internalList.Contains(model);
        }

        public void Add(T model)
        {
            if (model.ID == Guid.Empty)
                model.ID = Guid.NewGuid();

            if (!modelLookup.ContainsKey(model.ID))
            {
                internalList.Add(model);
                modelLookup.Add(model.ID, model);
            }
        }
    }
}
