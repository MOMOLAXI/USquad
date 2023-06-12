using System;
using System.Collections.Generic;

namespace UniverseEngine
{
    public abstract class UniverseSystem : ISystem
    {
        readonly Dictionary<Type, ISystem> m_Components = new();
        readonly List<ISystem> m_ComponentList = new();

        internal virtual int Priority { get; } = 0;

        int ISystem.Priority => Priority;

        void ISystem.Init()
        {
            try
            {
                OnRegisterComponents();
                OnComponentsInitialize();
                OnInit();
            }
            catch (Exception exception)
            {
                Log.Exception(exception);
            }
        }

        void ISystem.Update(float dt)
        {
            try
            {
                OnCompoenntUpdate(dt);
                OnUpdate(dt);
            }
            catch (Exception exception)
            {
                Log.Exception(exception);
            }
        }

        void ISystem.FixedUpdate(float dt)
        {
            try
            {
                OnComponentFixedUpdate(dt);
                OnFixedUpdate(dt);
            }
            catch (Exception exception)
            {
                Log.Exception(exception);
            }
        }

        void ISystem.LateUpdate(float dt)
        {
            try
            {
                OnComponentLateUpdate(dt);
                OnLateUpdate(dt);
            }
            catch (Exception exception)
            {
                Log.Exception(exception);
            }
        }

        void ISystem.Reset()
        {
            try
            {
                OnComponentReset();
                OnReset();
            }
            catch (Exception exception)
            {
                Log.Exception(exception);
            }
        }

        void ISystem.Destroy()
        {
            try
            {
                OnComponentDestroy();
                OnDestroy();
            }
            catch (Exception exception)
            {
                Log.Exception(exception);
            }
        }

        void ISystem.ApplicationFocus(bool hasFocus)
        {
            try
            {
                OnApplicationFocus(hasFocus);
            }
            catch (Exception exception)
            {
                Log.Exception(exception);
            }
        }

        void ISystem.ApplicationPause(bool pauseStatus)
        {
            try
            {
                OnApplicationPause(pauseStatus);
            }
            catch (Exception exception)
            {
                Log.Exception(exception);
            }
        }

        void ISystem.ApplicationQuit()
        {
            try
            {
                OnApplicationQuit();
            }
            catch (Exception exception)
            {
                Log.Exception(exception);
            }
        }

        public virtual void OnInit()
        {
        }
        public virtual void OnUpdate(float dt)
        {
        }
        public virtual void OnFixedUpdate(float dt)
        {
        }
        public virtual void OnLateUpdate(float dt)
        {
        }
        public virtual void OnReset()
        {
        }
        public virtual void OnDestroy()
        {
        }
        public virtual void OnApplicationFocus(bool hasFocus)
        {
        }
        public virtual void OnApplicationPause(bool pauseStatus)
        {
        }
        public virtual void OnApplicationQuit()
        {
        }

        public virtual void OnRegisterComponents()
        {
        }

        public void OnComponentsInitialize()
        {
            m_ComponentList.Sort((c1, c2) => c1.Priority - c2.Priority);
            for (int i = 0; i < m_ComponentList.Count; i++)
            {
                ISystem component = m_ComponentList[i];
                component.Init();
            }
        }

        public void OnCompoenntUpdate(float dt)
        {
            for (int i = 0; i < m_ComponentList.Count; i++)
            {
                ISystem component = m_ComponentList[i];
                component.Update(dt);
            }
        }

        public void OnComponentFixedUpdate(float dt)
        {
            for (int i = 0; i < m_ComponentList.Count; i++)
            {
                ISystem component = m_ComponentList[i];
                component.FixedUpdate(dt);
            }
        }

        public void OnComponentLateUpdate(float dt)
        {
            for (int i = 0; i < m_ComponentList.Count; i++)
            {
                ISystem component = m_ComponentList[i];
                component.LateUpdate(dt);
            }
        }

        public void OnComponentReset()
        {
            for (int i = 0; i < m_ComponentList.Count; i++)
            {
                ISystem component = m_ComponentList[i];
                component.Reset();
            }
        }

        public void OnComponentDestroy()
        {
            for (int i = 0; i < m_ComponentList.Count; i++)
            {
                ISystem component = m_ComponentList[i];
                component.Destroy();
            }
        }

        protected T RegisterComponent<T>() where T : SystemComponent, new()
        {
            if (m_Components.ContainsKey(typeof(T)))
            {
                return null;
            }

            ComponentGetter<T>.Component = Kernel.Create<T>();
            m_Components[typeof(T)] = ComponentGetter<T>.Component;
            m_ComponentList.Add(ComponentGetter<T>.Component);
            return ComponentGetter<T>.Component;
        }

        protected static T GetComponent<T>() where T : SystemComponent
        {
            return ComponentGetter<T>.Component;
        }

        private static class ComponentGetter<TComponent> where TComponent : SystemComponent
        {
            public static TComponent Component;
        }
    }

}
