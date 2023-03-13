using Eos.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories
{
    public class VirtualModelRepository<T> : IReadOnlyList<T?>, INotifyCollectionChanged, IList where T : BaseModel, new()
    {
        private readonly ModelRepository<T>[] repositories;

        public event NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add 
            {
                foreach (var repo in repositories)
                    repo.CollectionChanged += value;
            }
            remove
            {
                foreach (var repo in repositories)
                    repo.CollectionChanged -= value;
            }
        }

        public VirtualModelRepository(params ModelRepository<T>[] repositories)
        {
            this.repositories = repositories;
        }

        public T? this[int index]
        {
            get
            {
                int tmpIndex = index;
                for (int i = 0; i < repositories.Length; i++)
                {
                    if (tmpIndex >= repositories[i].Count)
                        tmpIndex -= repositories[i].Count;
                    else
                        return repositories[i][tmpIndex];
                }

                throw new IndexOutOfRangeException();
            }
        }

        int IReadOnlyCollection<T?>.Count => repositories.Sum(list => list.Count);

        public IEnumerator<T?> GetEnumerator()
        {
            return repositories.SelectMany(list => list).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return repositories.SelectMany(list => list).GetEnumerator();
        }

        public T? GetByID(Guid id)
        {
            T? result = null;
            foreach (var repo in repositories)
            {
                result = repo.GetByID(id);
                if (result != null) break;
            }

            return result;
        }

        public T? GetByIndex(int index)
        {
            T? result = null;
            foreach (var repo in repositories)
            {
                result = repo.GetByIndex(index);
                if (result != null) break;
            }

            return result;
        }

        public bool Contains(Guid id)
        {
            var result = false;
            foreach (var repo in repositories)
            {
                result = repo.Contains(id);
                if (result) break;
            }

            return result;
        }

        public bool Contains(int index)
        {
            var result = false;
            foreach (var repo in repositories)
            {
                result = repo.Contains(index);
                if (result) break;
            }

            return result;
        }

        public T? Resolve(T? modelRef)
        {
            if (modelRef == null) return null;
            return GetByID(modelRef.ID);
        }

        // IList

        public int Add(object? value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(object? value)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(object? value)
        {
            var tmpIndex = 0;
            for (int i = 0; i < repositories.Length; i++)
            {
                for (int j=0; j < repositories[i].Count; j++)
                {
                    if (repositories[i][j] == value) return tmpIndex;
                    tmpIndex++;
                }
            }

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

        public bool IsFixedSize => throw new NotImplementedException();

        public bool IsReadOnly => true;

        public int Count => repositories.Sum(list => list.Count);

        public bool IsSynchronized => throw new NotImplementedException();

        public object SyncRoot => throw new NotImplementedException();

        object? IList.this[int index] { get => this[index]; set { } }
    }
}
