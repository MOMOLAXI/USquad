using System.Collections.Generic;

namespace UniverseEngine
{
    internal static class EntityIDGenerator
    {
        static readonly Dictionary<int, int> s_TypeSerials = new();

        public static EntityID Get(int type)
        {
            EntityID id;
            if (s_TypeSerials.TryGetValue(type, out int serial))
            {
                serial++;
                id = new(type, serial);
            }
            else
            {
                serial = 0;
                id = new(type, serial);
            }

            s_TypeSerials[type] = serial;

            return id;
        }
    }
}