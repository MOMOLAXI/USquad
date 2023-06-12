namespace UniverseEngine
{
    public static partial class UniverseEngine
    {
        /// <summary>
        /// 订阅全局消息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        public static void Subscribe(ulong id, MessageEventCallback callback)
        {
            if (callback == null)
            {
                Log.Error($"callback is null while subscribe dynamic message for {id.ToString()}");
                return;
            }

            EngineSystem<MessageSystem>.System.Subscribe(id, callback);
        }

        /// <summary>
        /// 广播全局消息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        public static void BroadCast(ulong id, Args args)
        {
            EngineSystem<MessageSystem>.System.BroadCast(id, args);
        }

        /// <summary>
        /// 移除全局消息
        /// </summary>
        /// <param name="id"></param>
        public static void RemoveDynamicMessage(ulong id)
        {
            EngineSystem<MessageSystem>.System.RemoveMessage(id);
        }

        /// <summary>
        /// 移除全局消息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        public static void RemoveDynamicMessage(ulong id, MessageEventCallback callback)
        {
            EngineSystem<MessageSystem>.System.RemoveMessage(id, callback);
        }
    }
}
