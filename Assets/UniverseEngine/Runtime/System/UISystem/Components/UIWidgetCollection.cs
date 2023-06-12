using System.Collections.Generic;
using UnityEngine;

namespace UniverseEngine
{
    internal class UIWidgetCollection
    {
        readonly int m_RootOrder;
        readonly int m_OrderStep;

        readonly Dictionary<int, WidgetInstance> m_RunningWidgets = new();
        readonly Dictionary<int, WidgetInstance> m_CacheWidgets = new();

        int TopOrder => m_RootOrder + (m_RunningWidgets.Count + 1) * m_OrderStep;

        public void GetRunningWidgets(List<int> result)
        {
            result.AddRange(m_RunningWidgets.Keys);
        }

        public UIWidgetCollection(int rootOrdet, int orderStep)
        {
            m_RootOrder = rootOrdet;
            m_OrderStep = orderStep;
        }

        public WidgetInstance FindInRunning(int id)
        {
            if (m_RunningWidgets.TryGetValue(id, out WidgetInstance instance))
            {
                return instance;
            }

            return null;
        }

        public WidgetInstance FindInCache(int id)
        {
            if (m_CacheWidgets.TryGetValue(id, out WidgetInstance instance))
            {
                return instance;
            }

            return null;
        }

        public void AddToRunning(WidgetInstance instance)
        {
            int typeID = instance.TypeID;
            if (m_RunningWidgets.ContainsKey(typeID))
            {
                return;
            }

            instance.Widget.Initialize(TopOrder);
            instance.Widget.gameObject.SetGraphicRaycasterMask(LayerMask.NameToLayer("UI"));
            instance.Widget.gameObject.SetGraphicRaycasterState(true);
            m_RunningWidgets[typeID] = instance;
        }

        public void AddToCache(WidgetInstance instance)
        {
            int typeID = instance.TypeID;
            if (m_CacheWidgets.ContainsKey(typeID))
            {
                return;
            }

            m_CacheWidgets[typeID] = instance;
        }

        public void RemoveFromRunning(int typeID)
        {
            if (m_RunningWidgets.ContainsKey(typeID))
            {
                m_RunningWidgets.Remove(typeID);
            }
        }

        public void RemoveFromCache(int typeID)
        {
            if (m_CacheWidgets.ContainsKey(typeID))
            {
                m_CacheWidgets.Remove(typeID);
            }
        }
    }
}
