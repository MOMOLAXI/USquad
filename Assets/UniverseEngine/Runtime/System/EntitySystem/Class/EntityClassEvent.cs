using System;
using System.Collections.Generic;

namespace UniverseEngine
{
    /// <summary>
    /// 逻辑事件回调
    /// </summary>
    /// <param name="self"></param>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void EntityEventCallBack(EntityID self, EntityID sender, Args args);

    [Serializable]
    public class EntityClassEvent : ICacheAble
    {
        public List<EntityEventCallBack> Events = new();

        public int Count => Events.Count;

        public EntityEventCallBack Get(int index)
        {
            return !Collections.IsValidIndex(Events, index) ? NoneInvoke : Events[index];
        }

        public bool IsInCache { get; set; }

        public void Reset()
        {
            Events.Clear();
        }

        public void Execute(EntityID self, EntityID sender, Args args)
        {
            for (int i = 0; i < Events.Count; i++)
            {
                EntityEventCallBack hookWrapper = Events[i];
                if (hookWrapper == null)
                {
                    continue;
                }

                try
                {
                    hookWrapper.Invoke(self, sender, args);
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
        }

        public void Add(EntityClassEvent other)
        {
            if (other == null)
            {
                return;
            }

            Events.AddRange(other.Events);
        }

        public void Add(EntityEventCallBack eventCallBack)
        {
            if (eventCallBack == null)
            {
                return;
            }

            Events.Add(eventCallBack);
        }

        public void Remove(int index)
        {
            if (!Collections.IsValidIndex(Events, index))
            {
                return;
            }

            Events.RemoveAt(index);
        }

        static void NoneInvoke(EntityID self, EntityID sender, Args args)
        {
        }
    }
}
