using System.Collections.Generic;

namespace UniverseEngine
{
    internal static class EngineUnits
    {
        internal static readonly List<EngineSystem> CoreUnits = new()
        {
            Kernel.Create<FileSystem>(),
            Kernel.Create<HttpSystem>(),
            Kernel.Create<AndroidSystem>(),
            Kernel.Create<FunctionSystem>(),
            Kernel.Create<EntitySystem>(),
            Kernel.Create<HeartBeatSystem>(),
            Kernel.Create<MessageSystem>(),
            Kernel.Create<AssetSystem>(),
            Kernel.Create<AssetCacheSystem>(),
            Kernel.Create<AssetEncryptionSystem>(),
        };
        
        internal static readonly List<EngineSystem> CustomUnits = new()
        {
            Kernel.Create<SceneSystem>(),
            Kernel.Create<UISystem>(),
        };
    }
}
