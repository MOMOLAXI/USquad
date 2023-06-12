#if ODIN_INSPECTOR || ODIN_INSPECTOR_3
using Sirenix.OdinInspector;
#endif

namespace UniverseEngine.Editor
{
    public partial class UniverseSetting : UniverseManagedSetting
    {
        /// <summary>
        /// 打包设置
        /// </summary>
    #if ODIN_INSPECTOR || ODIN_INSPECTOR_3
        [TabGroup("PlayerBuilder")]
        [HideLabel]
    #endif
        public PlayerBuildOptions BuildOptions = new();

    #if ODIN_INSPECTOR || ODIN_INSPECTOR_3
        [TabGroup("PlayerSetting")]
        [HideLabel]
    #endif
        public PlayerBuildSetting BuildSetting = new();
    }
}
