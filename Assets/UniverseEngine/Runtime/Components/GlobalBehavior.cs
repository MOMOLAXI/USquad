using UnityEngine;

namespace UniverseEngine
{
    public class GlobalBehavior : UniverseBehavior
    {
        string m_Name = string.Empty;

        protected virtual string Name { get; }

        protected sealed override void Awake()
        {
            if (string.IsNullOrEmpty(m_Name))
            {
                m_Name = name;
            }

            GameObject go = gameObject;
            go.name = $"[{Name}]";
            DontDestroyOnLoad(go);
            OnAwake();
        }

        protected virtual void OnAwake()
        {

        }
    }
}
