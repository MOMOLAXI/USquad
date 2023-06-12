using System.Collections.Generic;

namespace UniverseEngine
{
    internal class UIRegistryComponent : SystemComponent
    {
        readonly Dictionary<int, UIMeta> m_UIRegistry = new();

        public void Register(UIMeta meta)
        {
            if (meta == null)
            {
                Log.Error("UIMeta is invalid while register");
                return;
            }

            int id = meta.ID;
            if (m_UIRegistry.ContainsKey(id))
            {
                Log.Error($"Registered duplicated ui : {id.ToString()}");
                return;
            }

            m_UIRegistry[id] = meta;
        }

        public UIMeta GetUIMeta(int id)
        {
            return m_UIRegistry.TryGetValue(id, out UIMeta data) ? data : null;
        }
    }
}