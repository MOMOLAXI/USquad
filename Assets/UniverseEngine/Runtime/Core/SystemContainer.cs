using System;
using System.Collections.Generic;

namespace UniverseEngine
{
    public class SystemContainer<T> where T : ISystem, new()
    {
        readonly Dictionary<Type, T> m_Type2System = new();
        readonly List<T> m_SystemInstance = new();

        internal void Register(IList<T> systems)
        {
            if (systems == null)
            {
                Log.Error("System Collection is null which being registered");
                return;
            }

            for (int i = 0; i < systems.Count; ++i)
            {
                T system = systems[i];
                Register(system);
            }
        }

        public TSystem Register<TSystem>() where TSystem : T, new()
        {
            TSystem module = Kernel.Create<TSystem>();
            Register(module);
            return module;
        }

        internal void Register(T system)
        {
            if (system == null)
            {
                return;
            }

            Type type = system.GetType();
            //类型映射
            m_Type2System[type] = system;

            //实例缓存
            m_SystemInstance.Add(system);
        }

        internal TSystem Get<TSystem>() where TSystem : class, T, new()
        {
            Type type = typeof(TSystem);
            return m_Type2System.TryGetValue(type, out T module) ? module as TSystem : default;
        }

        internal void PreInit()
        {
            m_SystemInstance.Sort((s1, s2) => s1.Priority - s2.Priority);
        }

        internal void Init()
        {
            for (int i = 0; i < m_SystemInstance.Count; i++)
            {
                T system = m_SystemInstance[i];
                system.Init();
            }
        }

        internal void Update(float dt)
        {
            for (int i = 0; i < m_SystemInstance.Count; ++i)
            {
                T system = m_SystemInstance[i];
                system.Update(dt);
            }
        }

        internal void FixedUpdate(float dt)
        {
            for (int i = 0; i < m_SystemInstance.Count; ++i)
            {
                T system = m_SystemInstance[i];
                system.FixedUpdate(dt);
            }
        }

        internal void LateUpdate(float dt)
        {
            for (int i = 0; i < m_SystemInstance.Count; ++i)
            {
                T system = m_SystemInstance[i];
                system.LateUpdate(dt);
            }
        }

        internal void Reset()
        {
            for (int i = 0; i < m_SystemInstance.Count; ++i)
            {
                T system = m_SystemInstance[i];
                system.Reset();
            }
        }

        internal void Destroy()
        {
            for (int i = 0; i < m_SystemInstance.Count; ++i)
            {
                T system = m_SystemInstance[i];
                system.Destroy();
            }
        }

        internal void ApplicationFocus(bool hasFocus)
        {
            for (int i = 0; i < m_SystemInstance.Count; ++i)
            {
                T system = m_SystemInstance[i];
                system.ApplicationFocus(hasFocus);
            }
        }

        internal void ApplicationPause(bool pauseStatus)
        {
            for (int i = 0; i < m_SystemInstance.Count; ++i)
            {
                T system = m_SystemInstance[i];
                system.ApplicationPause(pauseStatus);
            }
        }

        internal void ApplicationQuit()
        {
            for (int i = 0; i < m_SystemInstance.Count; ++i)
            {
                T system = m_SystemInstance[i];
                system.ApplicationQuit();
            }
        }
    }
}
