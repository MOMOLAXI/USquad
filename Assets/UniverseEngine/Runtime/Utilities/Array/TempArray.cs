using System.Collections.Generic;

namespace UniverseEngine
{
    /// <summary>
    /// 实例全局唯一
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct TempArray<T>
    {
        public readonly T[] Instance;
        static readonly Dictionary<int, T[]> s_ArrayInstance = new();
        
        public TempArray(int capacity) : this()
        {
            if (capacity <= 0)
            {
                return;
            }

            if (s_ArrayInstance.TryGetValue(capacity, out T[] exist))
            {
                Instance = exist;
                return;
            }

            T[] array = new T[capacity];
            Instance = array;
            s_ArrayInstance[capacity] = array;
        }

        TempArray(T[] instance)
        {
            Instance = instance;
        }

        public T this[int index]
        {
            get => Instance[index];
            set => Instance[index] = value;
        }

        public static implicit operator T[](TempArray<T> other)
        {
            return other.Instance;
        }

        public static explicit operator TempArray<T>(T[] other)
        {
            return new(other);
        }
    }
}