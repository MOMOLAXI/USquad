using System;
using System.Collections.Generic;

namespace UniverseEngine
{
    public static class GuidHelper
    {
        static readonly Dictionary<string, Guid> s_DynamicGuids = new();
        static readonly Dictionary<string, Guid> s_StaticGuids = new();

        public static Guid NewStatic(string randomString = "")
        {
            if (string.IsNullOrEmpty(randomString))
            {
                return Guid.NewGuid();
            }

            if (s_StaticGuids.TryGetValue(randomString, out Guid guid))
            {
                return guid;
            }

            string hash = HashUtilities.StringMD5(randomString);
            guid = new(hash);
            s_StaticGuids[randomString] = guid;
            return guid;
        }
        
        public static Guid NewDynamic(string randomString = "")
        {
            if (string.IsNullOrEmpty(randomString))
            {
                return Guid.NewGuid();
            }

            if (s_DynamicGuids.TryGetValue(randomString, out Guid guid))
            {
                return guid;
            }

            guid = Guid.NewGuid();
            s_DynamicGuids[randomString] = guid;
            return guid;
        }
    }
}
