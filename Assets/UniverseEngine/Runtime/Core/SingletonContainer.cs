using System;
using System.Collections.Generic;

namespace UniverseEngine
{
    /// <summary>
    /// 单例组件容器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonContainer<T> where T : class
    {
        readonly Dictionary<Type, T> m_Instance = new();

        public List<T> Instance { get; } = new();

        public TE Register<TE>(Action<TE> onRegister = null) where TE : class, T, new()
        {
            Type type = typeof(TE);
            if (m_Instance.ContainsKey(type))
            {
                return null;
            }

            TE instance = new();
            m_Instance[type] = instance;
            Instance.Add(instance);
            GenericGetter<TE>.Getter = InternalGet<TE>;
            onRegister?.Invoke(instance);
            return instance;
        }

        public TE Get<TE>() where TE : class, T
        {
            return GenericGetter<TE>.Instance;
        }
        
        TE InternalGet<TE>() where TE : class, T
        {
            if (m_Instance.TryGetValue(typeof(TE), out T instance))
            {
                return instance as TE;
            }

            return null;
        }

        private static class GenericGetter<TE> where TE : class, T
        {
            static TE s_Instance;
            public static Func<TE> Getter;
            public static TE Instance => s_Instance ??= Getter?.Invoke();
        }
    }
}