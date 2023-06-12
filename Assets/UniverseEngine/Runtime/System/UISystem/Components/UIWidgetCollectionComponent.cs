using System.Collections.Generic;

namespace UniverseEngine
{
    internal class UIWidgetCollectionComponent : SystemComponent
    {
        readonly Dictionary<ECanvasLayer, UIWidgetCollection> m_Collections = new();

        protected override void OnInit()
        {
            m_Collections[ECanvasLayer.TOP_LAYER] = new((int)ECanvasLayer.TOP_LAYER, 10);
            m_Collections[ECanvasLayer.AUTO_LAYER] = new((int)ECanvasLayer.AUTO_LAYER, 10);
            m_Collections[ECanvasLayer.BOTTOM_LAYER] = new((int)ECanvasLayer.BOTTOM_LAYER, 10);
        }

        public void GetRunningWidgets(List<int> result)
        {
            foreach (UIWidgetCollection collection in m_Collections.Values)
            {
                collection.GetRunningWidgets(result);
            }
        }

        public void AddToRunning(WidgetInstance instance)
        {
            if (m_Collections.TryGetValue(instance.InLayer, out UIWidgetCollection collection))
            {
                collection.AddToRunning(instance);
            }
        }

        public void AddToCache(WidgetInstance instance)
        {
            if (m_Collections.TryGetValue(instance.InLayer, out UIWidgetCollection collection))
            {
                collection.AddToCache(instance);
            }
        }

        public WidgetInstance FindInRunning(UIMeta meta)
        {
            if (m_Collections.TryGetValue(meta.CanvasLayer, out UIWidgetCollection collection))
            {
                return collection.FindInRunning(meta.ID);
            }

            return null;
        }

        public WidgetInstance FindInCache(UIMeta meta)
        {
            if (m_Collections.TryGetValue(meta.CanvasLayer, out UIWidgetCollection collection))
            {
                return collection.FindInCache(meta.ID);
            }

            return null;
        }

        public void RemoveFromRunning(WidgetInstance instance)
        {
            if (m_Collections.TryGetValue(instance.InLayer, out UIWidgetCollection collection))
            {
                collection.RemoveFromRunning(instance.TypeID);
            }
        }

        public void RemoveFromCache(WidgetInstance instance)
        {
            if (m_Collections.TryGetValue(instance.InLayer, out UIWidgetCollection collection))
            {
                collection.RemoveFromCache(instance.TypeID);
            }
        }
    }
}