using UniverseEngine;

namespace USquad
{
    public enum EMessage
    {
    }

    public static class MessageHelper
    {
        public static void BroadCast(EMessage message, Args messageArgs = default(Args))
        {
            UniverseEngine.UniverseEngine.BroadCast((ulong)message, messageArgs);
        }

        public static void Subscribe(EMessage message, MessageEventCallback callback)
        {
            UniverseEngine.UniverseEngine.Subscribe((ulong)message, callback);
        }

        public static void Remove(EMessage message)
        {
            UniverseEngine.UniverseEngine.RemoveDynamicMessage((ulong)message);
        }

        public static void Remove(EMessage message, MessageEventCallback callback)
        {
            UniverseEngine.UniverseEngine.RemoveDynamicMessage((ulong)message, callback);
        }
    }
}
