using System;
using System.Collections.Generic;

namespace Carotaa.Code
{
    public abstract class ObjectPool<T> where T : class
    {
        private readonly Stack<T> _buffer;
        private readonly Func<T> _factory;

        private readonly int _maxSize;

        protected ObjectPool(Func<T> factory, int size)
        {
            _buffer = new Stack<T>();
            _factory = factory;
            _maxSize = size;
        }

        protected T Rent()
        {
            var item = _buffer.Count > 0 ? _buffer.Pop() : _factory.Invoke();

            OnRentItem(item);

            return item;
        }

        protected void Back(T item)
        {
            if (item == null) return;

            OnBackItem(item);

            if (_buffer.Count < _maxSize)
            {
                _buffer.Push(item);
            }
            else
            {
                DestroyItem(item);
            }
        }

        protected virtual void OnRentItem(T item)
        {
        }

        protected virtual void OnBackItem(T item)
        {
        }

        protected abstract void DestroyItem(T item);
    }

    public class ListPool<T> : ObjectPool<List<T>>
    {
        private static readonly ListPool<T> Pool = new ListPool<T>(256);

        public ListPool(int size) : base(() => new List<T>(), size)
        {
        }

        protected override void OnBackItem(List<T> item)
        {
            item.Clear();
        }

        protected override void DestroyItem(List<T> item)
        {
        }

        public static List<T> Get()
        {
            lock (Pool)
            {
                return Pool.Rent();
            }
        }

        public static void Release(List<T> list)
        {
            lock (Pool)
            {
                Pool.Back(list);
            }
        }
    }
}