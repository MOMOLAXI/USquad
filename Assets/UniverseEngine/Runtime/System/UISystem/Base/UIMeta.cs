//using LuaInterface;

using System;

namespace UniverseEngine
{
    [Serializable]
    internal class UIMeta
    {
        public string ResourceKey;
        public int ID;
        public ECanvasLayer CanvasLayer;

        public static UIMeta Create(int id,
                                    string resourceKey,
                                    ECanvasLayer canvasLayer = ECanvasLayer.AUTO_LAYER)
        {
            return new(id, resourceKey, canvasLayer);
        }

        UIMeta(int uiID, string key, ECanvasLayer canvasLayer)
        {
            ID = uiID;
            ResourceKey = key;
            CanvasLayer = canvasLayer;
        }
    }
}