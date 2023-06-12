using System;
using System.Collections.Generic;

namespace UniverseEngine
{
    internal class MessageEvent : ICacheAble
    {
        readonly List<MessageEventCallback> m_Callbacks = new();

        public int Count => m_Callbacks.Count;
        
        public void Register(MessageEventCallback function)
        {
            if (function == null)
            {
                return;
            }

            m_Callbacks.Add(function);
        }

        public void Invoke(Args args)
        {
            for (int i = 0; i < m_Callbacks.Count; i++)
            {
                MessageEventCallback callback = m_Callbacks[i];
                try
                {
                    callback?.Invoke(args);
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
        }

        public void Remove(MessageEventCallback callback)
        {
            m_Callbacks.Remove(callback);
        }

        public bool IsInCache { get; set; }

        public void Reset()
        {
            m_Callbacks.Clear();
        }
    }
}
