using System.Collections.Generic;

namespace UniverseEngine
{
    internal class MessageSystem : EngineSystem
    {
        readonly Dictionary<ulong, MessageEvent> m_Events = new();

        readonly LinkedPool<MessageEvent> m_EventPool = new(() => new(), null, OnMessageRelease);

        /// <summary>
        /// 注册动态消息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        public void Subscribe(ulong id, MessageEventCallback callback)
        {
            if (callback == null)
            {
                Log.Error($"callback for {id} is null. can not register in FunctionLibrary");
                return;
            }

            if (m_Events.TryGetValue(id, out MessageEvent message))
            {
                message.Register(callback);
                return;
            }

            message = m_EventPool.Get();
            message.Register(callback);
            m_Events[id] = message;
        }

        /// <summary>
        /// 消息是否注册
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ContainsMessage(ulong id)
        {
            return m_Events.ContainsKey(id);
        }

        /// <summary>
        /// 移除消息
        /// </summary>
        /// <param name="id"></param>
        public void RemoveMessage(ulong id)
        {
            if (!m_Events.TryGetValue(id, out MessageEvent message))
            {
                Log.Warning($"Message Id : {id} is not found while remove message");
                return;
            }

            m_Events.Remove(id);
            m_EventPool.Release(message);
        }

        /// <summary>
        /// 移除消息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        public void RemoveMessage(ulong id, MessageEventCallback callback)
        {
            if (!m_Events.TryGetValue(id, out MessageEvent message))
            {
                Log.Warning($"Message Id : {id} is not found while remove message");
                return;
            }

            message.Remove(callback);
            if (message.Count <= 0)
            {
                m_EventPool.Release(message);
            }
        }

        /// <summary>
        /// 广播消息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        public void BroadCast(ulong id, Args args)
        {
            if (!m_Events.TryGetValue(id, out MessageEvent message))
            {
                return;
            }

            message.Invoke(args);
            args.Release();
        }

        static void OnMessageRelease(MessageEvent messageEvent)
        {
            messageEvent.Reset();
        }
    }
}
