using System;
using UnityEngine;

namespace UniverseEngine
{
    [Serializable]
    public class InitializeArguments
    {
        [Header("Engine")]
        public ELogLevel LogLevel = ELogLevel.Debug;
        public int TargetFrameRate = 60;
        public Vector2 Resolutions = UniverseConstant.DefaultResolution;
        public string UIPackageName = "UI";
        public string ScenePackageName = "Scenes";
        
        public virtual void RegisterGameSystems()
        {

        }
        
        public virtual void RetgisterUIWidgets()
        {

        }

        protected static void RegisterSystem<T>() where T : GameSystem, new()
        {
            UniverseEngine.RegisterGameSystem<T>();
        }

        protected static void RegisterUIWidget<T>(T id, string resourceKey, ECanvasLayer canvasLayer = ECanvasLayer.AUTO_LAYER)
        {
            int typeID = id switch
            {
                Enum enumID => Convert.ToInt32(enumID),
                int intID => intID,
                _ => -1
            };

            if (typeID == -1)
            {
                return;
            }

            UISystem.Register(new() { ID = typeID, ResourceKey = resourceKey, Layer = canvasLayer });
        }
    }
}
