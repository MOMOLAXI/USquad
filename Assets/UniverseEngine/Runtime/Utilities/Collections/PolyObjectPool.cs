using System;
using System.Collections.Generic;
using System.Linq;

namespace UniverseEngine
{
    /// <summary>
    /// 多态对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PolyObjectPool<T> where T : class, ICacheAble
    {
        readonly Dictionary<Type, Stack<T>> m_TypeMapping = new();
        
        /// <summary>
        /// 当前缓存内数量
        /// </summary>
        public int TotalCachedCount => m_TypeMapping.Values.Sum(value => value.Count);

        /// <summary>
        /// Max Count
        /// </summary>
        public int MaxCacheCount { get; }

        /// <summary>
        /// Construct with count
        /// </summary>
        /// <param name="maxCacheCount"></param>
        public PolyObjectPool(int maxCacheCount = Const.N_1024)
        {
            MaxCacheCount = maxCacheCount;
        }

        /// <summary>
        /// Get Generic
        /// </summary>
        /// <typeparam name="TE"></typeparam>
        /// <returns></returns>
        public TE Get<TE>() where TE : class, T, new()
        {
            if (!m_TypeMapping.TryGetValue(typeof(TE), out Stack<T> stack))
            {
                return new();
            }

            if (stack.Count <= 0)
            {
                return new();
            }
            
            T element = stack.Pop();
            if (element is not ICacheAble cacheAble)
            {
                return element as TE;
            }

            cacheAble.IsInCache = false;
            cacheAble.Reset();

            return element as TE;
        }

        /// <summary>
        /// Get By Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public T Get(Type type)
        {
            if (m_TypeMapping.TryGetValue(type, out Stack<T> stack) && stack.Count > 0)
            {
                T element = stack.Pop();
                if (element is ICacheAble cacheAble)
                {
                    cacheAble.IsInCache = false;
                }

                return element;
            }
            
            T t = type.Assembly.CreateInstance(type.Name) as T;
            return t;
        }

        /// <summary>
        /// Release Generic
        /// </summary>
        /// <param name="tp"></param>
        /// <typeparam name="TE"></typeparam>
        public void Release<TE>(TE tp) where TE : T
        {
            if (tp == null)
            {
                return;
            }

            //只缓存继承了ICacheAble接口的元素
            ICacheAble cacheAble = tp;
            if (cacheAble.IsInCache)
            {
                return;
            }

            //缓存数量超过限制不缓存, 直接丢掉
            if (TotalCachedCount >= MaxCacheCount)
            {
                cacheAble.IsInCache = true;
                return;
            }

            Type type = cacheAble.GetType();

            if (!m_TypeMapping.TryGetValue(type, out Stack<T> stack))
            {
                stack = new();
                m_TypeMapping.Add(type, stack);
            }
            
            cacheAble.IsInCache = true;
            cacheAble.Reset();
            stack.Push(tp);
        }
    }
}
