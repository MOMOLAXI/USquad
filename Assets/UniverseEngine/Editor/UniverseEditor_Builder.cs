// using System;
// using System.IO;
// using UnityEditor;
// using UniverseEngine;
//
// namespace UniverseEditor
// {
//     /// <summary>
//     /// 编辑器工具类
//     /// </summary>
//     public static partial class UniverseEditor
//     {
//         /// <summary>
//         /// Build from command line
//         /// </summary>
//         public static void BuildPlayer()
//         {
//
//         }
//
//         /// <summary>
//         /// 根据现有配置快速打包
//         /// </summary>
//         public static void FastBuildPlayer()
//         {
//             PlayerBuildOptions buildOptions = Setting.BuildOptions;
//             if (string.IsNullOrEmpty(buildOptions.OutputDirectory))
//             {
//                 EditorLog.Error("Please choose output directory before build player");
//                 return;
//             }
//
//             if (string.IsNullOrEmpty(buildOptions.OutputName))
//             {
//                 buildOptions.OutputName = nameof(UniverseEngine);
//             }
//
//             string platformDirectory = buildOptions.Target.ToString().Replace(" ", string.Empty);
//             string outputDirectory = $"{Path.Combine(buildOptions.OutputDirectory, platformDirectory, buildOptions.OutputName)}";
//             FileSystem.DeleteDirectory(outputDirectory);
//
//             AssetBuildResult bundleBuildResult = AssetBuildSystem.BuildAssetBundles(FileSystem.GetBundleOutputDirectory(),
//                                                                                      buildOptions.Target,
//                                                                                      AssetBuildSystem.CreatePackageBuildVersion(),
//                                                                                      EEncryptType.None);
//             if (!bundleBuildResult.Success)
//             {
//                 EditorLog.Error(bundleBuildResult.ErrorInfo);
//                 return;
//             }
//
//             string extension = buildOptions.Extension.StartsWith(".") ? buildOptions.Extension : $".{buildOptions.Extension}";
//             BuildPlayerOptions targetOptions = new()
//             {
//                 options = buildOptions.Options,
//                 target = buildOptions.Target,
//                 targetGroup = buildOptions.TargetGroup,
//                 scenes = buildOptions.Scenes,
//                 locationPathName = $"{Path.Combine(outputDirectory, $"{buildOptions.OutputName}{extension}")}",
//                 assetBundleManifestPath = buildOptions.AssetBundleManifestPath,
//                 extraScriptingDefines = buildOptions.ExtraScriptingDefines
//             };
//
//             PlayerSettings.SetScriptingBackend(buildOptions.TargetGroup, buildOptions.ScriptingImplementation);
//             PlayerSettings.SetApiCompatibilityLevel(buildOptions.TargetGroup, buildOptions.ApiCompatibilityLevel);
//
//             PlayerBuildSetting buildSetting = Setting.BuildSetting;
//             PlayerSettings.companyName = buildSetting.CompanyName;
//             PlayerSettings.productName = buildSetting.ProductName;
//             PlayerSettings.SplashScreen.show = buildSetting.ShowUnitySplashScreen;
//             PlayerSettings.colorSpace = buildSetting.TargetColorSpace;
//             PlayerSettings.defaultScreenWidth = buildSetting.DefaultScreenWidth;
//             PlayerSettings.defaultScreenHeight = buildSetting.DefaultScreenHeight;
//             PlayerSettings.runInBackground = buildSetting.RunInBackground;
//             PlayerSettings.captureSingleScreen = buildSetting.CaptureSingleScreen;
//             PlayerSettings.usePlayerLog = buildSetting.WriteDebugLog;
//             PlayerSettings.resizableWindow = buildSetting.ResizableWindow;
//             PlayerSettings.resetResolutionOnWindowResize = buildSetting.ResetResolutionOnWindowResize;
//             PlayerSettings.bakeCollisionMeshes = buildSetting.PrebakeCollisionMeshes;
//             PlayerSettings.useHDRDisplay = buildSetting.UseHDRDisplay;
//             PlayerSettings.allowFullscreenSwitch = buildSetting.AllowFullScreenSwitch;
//             PlayerSettings.forceSingleInstance = buildSetting.ForceSingletonInstance;
//             PlayerSettings.useFlipModelSwapchain = buildSetting.UseFlipModelSwapChainForD3D11;
//
//             UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(targetOptions);
//             if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
//             {
//                 string absPath = Path.GetFullPath(outputDirectory);
//                 EditorLog.Info(absPath);
//                 EditorUtility.RevealInFinder(absPath);
//                 string burstInfoPath = Path.Combine(absPath, $"{buildOptions.OutputName}_BurstDebugInformation_DoNotShip");
//                 FileSystem.DeleteDirectory(burstInfoPath);
//             }
//         }
//
//         public static BuildTarget ConvertBuildTarget(EBuildTarget buildTarget)
//         {
//             return buildTarget switch
//             {
//                 EBuildTarget.NoTarget => BuildTarget.NoTarget,
//                 EBuildTarget.StandaloneOSX => BuildTarget.StandaloneOSX,
//                 EBuildTarget.StandaloneWindows => BuildTarget.StandaloneWindows,
//                 EBuildTarget.Android => BuildTarget.Android,
//                 EBuildTarget.StandaloneWindows64 => BuildTarget.StandaloneWindows64,
//                 EBuildTarget.WebGL => BuildTarget.WebGL,
//                 EBuildTarget.WSAPlayer => BuildTarget.WSAPlayer,
//                 EBuildTarget.StandaloneLinux64 => BuildTarget.StandaloneLinux64,
//                 EBuildTarget.Tvos => BuildTarget.tvOS,
//                 EBuildTarget.Switch => BuildTarget.Switch,
//                 EBuildTarget.Stadia => BuildTarget.Stadia,
//                 EBuildTarget.GameCoreXboxOne => BuildTarget.GameCoreXboxOne,
//                 EBuildTarget.PS5 => BuildTarget.PS5,
//                 _ => throw new ArgumentOutOfRangeException(nameof(buildTarget), buildTarget, null)
//             };
//         }
//
//         public static EBuildTarget ConvertBuildTarget(BuildTarget buildTarget)
//         {
//             return buildTarget switch
//             {
//                 BuildTarget.NoTarget => EBuildTarget.NoTarget,
//                 BuildTarget.StandaloneOSX => EBuildTarget.StandaloneOSX,
//                 BuildTarget.StandaloneWindows => EBuildTarget.StandaloneWindows,
//                 BuildTarget.Android => EBuildTarget.Android,
//                 BuildTarget.StandaloneWindows64 => EBuildTarget.StandaloneWindows64,
//                 BuildTarget.WebGL => EBuildTarget.WebGL,
//                 BuildTarget.WSAPlayer => EBuildTarget.WSAPlayer,
//                 BuildTarget.StandaloneLinux64 => EBuildTarget.StandaloneLinux64,
//                 BuildTarget.tvOS => EBuildTarget.Tvos,
//                 BuildTarget.Switch => EBuildTarget.Switch,
//                 BuildTarget.Stadia => EBuildTarget.Stadia,
//                 BuildTarget.GameCoreXboxOne => EBuildTarget.GameCoreXboxOne,
//                 BuildTarget.PS5 => EBuildTarget.PS5,
//                 _ => throw new ArgumentOutOfRangeException(nameof(buildTarget), buildTarget, null)
//             };
//         }
//     }
// }
