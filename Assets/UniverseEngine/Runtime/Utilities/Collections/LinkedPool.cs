using System;

namespace UniverseEngine
{
    /// <summary>
    /// 链表实现池, 对于频繁存取的对象来说，内存更友好，没有list大小的频繁变化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LinkedPool<T> where T : class
    {
        const string INVALID_MAX_SIZE_MESSAGE = "Max size must be greater than 0";

        internal class LinkedPoolItem
        {
            internal LinkedPoolItem PoolNext;
            internal T Value;
        }

        readonly Func<T> m_CreateFunc;
        readonly Action<T> m_ActionOnGet;
        readonly Action<T> m_ActionOnRelease;
        readonly Action<T> m_ActionOnDestroy;
        readonly int m_Limit;                          // Used to prevent catastrophic memory retention.
        internal LinkedPoolItem PoolFirst;             // The pool of available T objects
        internal LinkedPoolItem NextAvailableListItem; // When Get is called we place the node here for reuse and to prevent GC
        readonly bool m_CollectionCheck;

        public LinkedPool(Func<T> createFunc,
                          Action<T> actionOnGet = null,
                          Action<T> actionOnRelease = null,
                          Action<T> actionOnDestroy = null,
                          bool collectionCheck = true,
                          int maxSize = 10000)
        {

            if (maxSize <= 0)
            {
                throw new ArgumentException(INVALID_MAX_SIZE_MESSAGE, nameof(maxSize));
            }

            m_CreateFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
            m_ActionOnDestroy = actionOnDestroy;
            m_Limit = maxSize;
            m_CollectionCheck = collectionCheck;
        }

        public int CountInactive { get; private set; }
        public int CountAll { get; private set; }

        public T Get()
        {
            T item = default(T);
            if (PoolFirst == null)
            {
                item = m_CreateFunc();
                CountAll++;
            }
            else
            {
                LinkedPoolItem first = PoolFirst;
                item = first.Value;
                PoolFirst = first.PoolNext;

                // Add the empty node to our pool for reuse and to prevent GC
                first.PoolNext = NextAvailableListItem;
                NextAvailableListItem = first;
                NextAvailableListItem.Value = null;
                --CountInactive;
            }
            m_ActionOnGet?.Invoke(item);
            return item;
        }

        public ObjectHandle<T> Get(out T v) => new(v = Get(), this);

        public void Release(T item)
        {
            if (m_CollectionCheck)
            {
                LinkedPoolItem listItem = PoolFirst;
                while (listItem != null)
                {
                    if (ReferenceEquals(listItem.Value, item))
                    {
                        throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");
                    }

                    listItem = listItem.PoolNext;
                }
            }

            m_ActionOnRelease?.Invoke(item);

            if (CountInactive < m_Limit)
            {
                LinkedPoolItem poolItem = NextAvailableListItem;
                if (poolItem == null)
                {
                    poolItem = new();
                }
                else
                {
                    NextAvailableListItem = poolItem.PoolNext;
                }

                poolItem.Value = item;
                poolItem.PoolNext = PoolFirst;
                PoolFirst = poolItem;
                ++CountInactive;
            }
            else
            {
                m_ActionOnDestroy?.Invoke(item);
            }
        }

        public void Clear()
        {
            if (m_ActionOnDestroy != null)
            {
                for (LinkedPoolItem itr = PoolFirst; itr != null; itr = itr.PoolNext)
                {
                    m_ActionOnDestroy(itr.Value);
                }
            }

            PoolFirst = null;
            NextAvailableListItem = null;
            CountInactive = 0;
            CountAll = 0;
        }

        public void Dispose()
        {
            Clear();
        }

        public readonly struct ObjectHandle<TObject> : IDisposable where TObject : class
        {
            readonly TObject m_ToReturn;
            readonly LinkedPool<TObject> m_Pool;

            internal ObjectHandle(TObject value, LinkedPool<TObject> pool)
            {
                m_ToReturn = value;
                m_Pool = pool;
            }

            void IDisposable.Dispose() => m_Pool.Release(m_ToReturn);
        }
    }


}
