using System;
#if ODIN_INSPECTOR || ODIN_INSPECTOR_3
using Sirenix.OdinInspector;
#endif
using UnityEditor;
using UnityEngine;

namespace UniverseEngine.Editor
{
    [Serializable]
    public class PlayerBuildOptions
    {
        public string OutputName = nameof(UniverseEngine);

        public string Extension = ".exe";

    #if ODIN_INSPECTOR || ODIN_INSPECTOR_3
        [ReadOnly]
    #endif
        public string[] Scenes;

    #if ODIN_INSPECTOR || ODIN_INSPECTOR_3
        [FolderPath]
    #endif
        public string OutputDirectory;

        [HideInInspector]
        public string AssetBundleManifestPath;

        [Space(5)]
        public BuildTargetGroup TargetGroup;
        public BuildTarget Target = BuildTarget.StandaloneWindows64;

        [Space(5)]
    #if ODIN_INSPECTOR || ODIN_INSPECTOR_3
        [EnumToggleButtons]
    #endif
        public FullScreenMode ScreenMode = FullScreenMode.Windowed;

        [Space(5)]
    #if ODIN_INSPECTOR || ODIN_INSPECTOR_3
        [EnumToggleButtons]
    #endif
        public BuildOptions Options;

        [Space(5)]
    #if ODIN_INSPECTOR || ODIN_INSPECTOR_3
        [EnumToggleButtons]
    #endif
        public ScriptingImplementation ScriptingImplementation;

        [Space(5)]
    #if ODIN_INSPECTOR || ODIN_INSPECTOR_3
        [EnumToggleButtons]
    #endif
        public ApiCompatibilityLevel ApiCompatibilityLevel;

        public string[] ExtraScriptingDefines;
    }
}
