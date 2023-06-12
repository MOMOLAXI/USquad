namespace UniverseEngine
{
    internal class WidgetInstance
    {
        static ulong s_Serial;
        public readonly int TypeID;
        public readonly ulong InstanceID;
        public readonly ECanvasLayer InLayer;
        public UIElement Widget;
        public bool IsActive
        {
            get => Widget != null && Widget.gameObject.activeInHierarchy;
            set
            {
                if (Widget == null)
                {
                    return;
                }

                Widget.gameObject.SetActive(value);
                Function.Run(Widget.OnVisibleChangedDelegate, value);
            }
        }
        
        public static WidgetInstance Create(UIMeta meta)
        {
            return new(meta);
        }

        WidgetInstance(UIMeta meta)
        {
            s_Serial++;
            TypeID = meta.ID;
            InLayer = meta.CanvasLayer;
            InstanceID = s_Serial;
        }
    }
}