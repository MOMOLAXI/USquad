using System;
#if ODIN_INSPECTOR || ODIN_INSPECTOR_3
using Sirenix.OdinInspector;
#endif
using UnityEditor;
using UnityEngine;

namespace UniverseEngine.Editor
{
    [Serializable]
    public class PlayerBuildSetting
    {
        public string CompanyName = "UniverseStudio";
        public string ProductName = nameof(UniverseEngine);
        public bool ShowUnitySplashScreen;
        public PlayerSettings.SplashScreen.UnityLogoStyle LogoStyle;
        public ColorSpace TargetColorSpace = ColorSpace.Linear;
        [Space(5)]
        public int DefaultScreenWidth = (int)UniverseConstant.DefaultResolution.x;
        public int DefaultScreenHeight = (int)UniverseConstant.DefaultResolution.y;
        public bool RunInBackground = true;
        public bool CaptureSingleScreen;
        public bool WriteDebugLog = true;
        public bool ResizableWindow = true;
        public bool ResetResolutionOnWindowResize = true;
        public bool PrebakeCollisionMeshes = true;
        public bool UseHDRDisplay;
        public bool AllowFullScreenSwitch = true;
        public bool ForceSingletonInstance = true;
        public bool UseFlipModelSwapChainForD3D11 = true;
    }
}
