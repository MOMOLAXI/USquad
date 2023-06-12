using System;

namespace UniverseEngine.Editor
{
    [Serializable]
    public abstract class UniverseFileSetting
    {
        public static T Create<T>() where T : UniverseFileSetting, new()
        {
            return new();
        }
    }
}
