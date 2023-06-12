// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using Object = UnityEngine.Object;
//
// namespace UniverseEngine
// {
//     public static partial class Engine
//     {
//         /// <summary>
//         /// 注册资源系统下载失败事件
//         /// </summary>
//         /// <param name="listener"></param>
//         public static void RegisterAssetSystemDownloadErrorEvent(Action<string, string> listener)
//         {
//             if (listener == null)
//             {
//                 return;
//             }
//
//             EngineSystem<AssetSystem>.System.OnDownloadError += listener;
//         }
//
//         /// <summary>
//         /// 批量初始化资源包
//         /// </summary>
//         /// <param name="args"></param>
//         /// <param name="progressHandler"></param>
//         /// <returns></returns>
//         public static async UniTask<bool> BulkInitializePackage(IList<InitializeParameters> args, Action<string, float> progressHandler)
//         {
//             using UProgress<float> progress = UProgress<float>.Create(progressHandler);
//             return await BulkInitializePackage(args, progress);
//         }
//
//         /// <summary>
//         /// 批量初始化资源包
//         /// </summary>
//         /// <param name="args"></param>
//         /// <param name="progress"></param>
//         /// <returns></returns>
//         public static async UniTask<bool> BulkInitializePackage(IList<InitializeParameters> args, UProgress<float> progress)
//         {
//             if (args == null)
//             {
//                 Log.Error("AssetsPackage initialize args is invalid");
//                 return false;
//             }
//
//             UniTask<(bool, string)>[] tasks = new UniTask<(bool, string)>[args.Count];
//             for (int i = 0; i < args.Count; i++)
//             {
//                 InitializeParameters param = args[i];
//                 tasks[i] = InitializePackage(param.PackageName, param, progress);
//             }
//
//             (bool, string)[] result = await UniTask.WhenAll(tasks);
//             return result.AllTrue();
//         }
//
//         public static async UniTask<(bool, string)> InitializePackage(string packageName, InitializeParameters args, UProgress<float> progress)
//         {
//             return await EngineSystem<AssetSystem>.System.Initialize(packageName, args, progress);
//         }
//
//         /// <summary>
//         /// 获取资源包
//         /// </summary>
//         /// <param name="packageName"></param>
//         /// <param name="package"></param>
//         /// <returns></returns>
//         public static bool GetAssetsPackage(string packageName, out UAssetsPackage package)
//         {
//             return AssetSystem.TryGetAssetsPackage(packageName, out package);
//         }
//
//         /// <summary>
//         /// 资源是否需要从远端更新下载
//         /// </summary>
//         /// <param name="packageName">资源包名称</param>
//         /// <param name="location">资源的定位地址</param>
//         /// <returns></returns>
//         public static bool IsNeedDownloadFromRemote(string packageName, string location)
//         {
//             return EngineSystem<AssetSystem>.System.IsNeedDownloadFromRemote(packageName, location);
//         }
//
//         /// <summary>
//         /// 获取资源信息列表
//         /// </summary>
//         /// <param name="packageName"></param>
//         /// <param name="tag">资源标签</param>
//         public static AssetInfo[] GetAssetInfos(string packageName, string tag)
//         {
//             return EngineSystem<AssetSystem>.System.GetAssetInfos(packageName, tag);
//         }
//
//         /// <summary>
//         /// 获取资源信息列表
//         /// </summary>
//         /// <param name="packageName"></param>
//         /// <param name="tags">资源标签列表</param>
//         public static AssetInfo[] GetAssetInfos(string packageName, string[] tags)
//         {
//             return EngineSystem<AssetSystem>.System.GetAssetInfos(packageName, tags);
//         }
//
//         /// <summary>
//         /// 获取资源信息
//         /// </summary>
//         /// <param name="packageName"></param>
//         /// <param name="location">资源的定位地址</param>
//         public static AssetInfo GetAssetInfo(string packageName, string location)
//         {
//             return EngineSystem<AssetSystem>.System.GetAssetInfo(packageName, location);
//         }
//
//         /// <summary>
//         /// 检查资源定位地址是否有效
//         /// </summary>
//         /// <param name="packageName"></param>
//         /// <param name="location">资源的定位地址</param>
//         public static bool CheckLocationValid(string packageName, string location)
//         {
//             return EngineSystem<AssetSystem>.System.CheckLocationValid(packageName, location);
//         }
//
//         /// <summary>
//         /// 异步加载原生文件
//         /// </summary>
//         /// <param name="packageName"></param>
//         /// <param name="assetInfo">资源信息</param>
//         /// <param name="progress"></param>
//         public static async UniTask<string> LoadRawFileTextAsync(string packageName,
//                                                                  AssetInfo assetInfo,
//                                                                  UProgress<float> progress = default(UProgress<float>))
//         {
//             RawFileOperationHandle handle = EngineSystem<AssetSystem>.System.LoadRawFileAsync(packageName, assetInfo, progress);
//             await handle.ToUniTask(Root);
//             return handle.GetRawFileText();
//         }
//
//         /// <summary>
//         /// 异步加载原生文件
//         /// </summary>
//         /// <param name="packageName"></param>
//         /// <param name="assetInfo">资源信息</param>
//         /// <param name="progress"></param>
//         public static async UniTask<byte[]> LoadRawFileBinaryAsync(string packageName,
//                                                                    AssetInfo assetInfo,
//                                                                    UProgress<float> progress = default(UProgress<float>))
//         {
//             RawFileOperationHandle handle = EngineSystem<AssetSystem>.System.LoadRawFileAsync(packageName, assetInfo, progress);
//             await handle.ToUniTask(Root);
//             return handle.GetRawFileData();
//         }
//
//         /// <summary>
//         /// 异步加载原生文件
//         /// </summary>
//         /// <param name="packageName"></param>
//         /// <param name="location">资源的定位地址</param>
//         /// <param name="progress"></param>
//         public static async UniTask<byte[]> LoadRawFileBinaryAsync(string packageName,
//                                                                    string location,
//                                                                    UProgress<float> progress = default(UProgress<float>))
//         {
//             RawFileOperationHandle handle = EngineSystem<AssetSystem>.System.LoadRawFileAsync(packageName, location, progress);
//             await handle.ToUniTask(Root);
//             return handle.GetRawFileData();
//         }
//
//         /// <summary>
//         /// 异步加载原生文件
//         /// </summary>
//         /// <param name="packageName"></param>
//         /// <param name="location">资源的定位地址</param>
//         /// <param name="progress"></param>
//         public static async UniTask<string> LoadRawFileTextAsync(string packageName,
//                                                                  string location,
//                                                                  UProgress<float> progress = default(UProgress<float>))
//         {
//             RawFileOperationHandle handle = EngineSystem<AssetSystem>.System.LoadRawFileAsync(packageName, location, progress);
//             await handle.ToUniTask(Root);
//             return handle.GetRawFileText();
//         }
//
//         /// <summary>
//         /// 异步加载场景
//         /// </summary>
//         /// <param name="packageName"></param>
//         /// <param name="location">场景的定位地址</param>
//         /// <param name="sceneMode">场景加载模式</param>
//         /// <param name="activateOnLoad">加载完毕时是否主动激活</param>
//         /// <param name="priority">优先级</param>
//         /// <param name="progress"></param>
//         public static async UniTask<Scene> LoadSceneAsync(string packageName,
//                                                           string location,
//                                                           LoadSceneMode sceneMode = LoadSceneMode.Single,
//                                                           bool activateOnLoad = true,
//                                                           int priority = 100,
//                                                           UProgress<float> progress = default(UProgress<float>))
//         {
//             SceneOperationHandle handle = EngineSystem<AssetSystem>.System.LoadSceneAsync(packageName, location, sceneMode, activateOnLoad, priority, progress);
//             await handle.ToUniTask(Root);
//             return handle.SceneObject;
//         }
//
//         /// <summary>
//         /// 异步加载场景
//         /// </summary>
//         /// <param name="packageName"></param>
//         /// <param name="assetInfo">场景的资源信息</param>
//         /// <param name="sceneMode">场景加载模式</param>
//         /// <param name="activateOnLoad">加载完毕时是否主动激活</param>
//         /// <param name="priority">优先级</param>
//         /// <param name="progress"></param>
//         public static async UniTask<Scene> LoadSceneAsync(string packageName,
//                                                           AssetInfo assetInfo,
//                                                           LoadSceneMode sceneMode = LoadSceneMode.Single,
//                                                           bool activateOnLoad = true,
//                                                           int priority = 100,
//                                                           UProgress<float> progress = default(UProgress<float>))
//         {
//             SceneOperationHandle handle = EngineSystem<AssetSystem>.System.LoadSceneAsync(packageName, assetInfo, sceneMode, activateOnLoad, priority, progress);
//             await handle.ToUniTask(Root);
//             return handle.SceneObject;
//         }
//
//         /// <summary>
//         /// 异步加载资源对象
//         /// </summary>
//         /// <param name="packageName"></param>
//         /// <param name="assetInfo">资源信息</param>
//         /// <param name="progress"></param>
//         public static async UniTask<Object> LoadAssetAsync(string packageName,
//                                                            AssetInfo assetInfo,
//                                                            UProgress<float> progress = default(UProgress<float>))
//         {
//             AssetOperationHandle handle = EngineSystem<AssetSystem>.System.LoadAssetAsync(packageName, assetInfo, progress);
//             await handle.ToUniTask(Root);
//             return handle.AssetObject;
//         }
//
//         /// <summary>
//         /// 异步加载资源对象
//         /// </summary>
//         /// <typeparam name="TObject">资源类型</typeparam>
//         /// <param name="packageName"></param>
//         /// <param name="location">资源的定位地址</param>
//         /// <param name="progress"></param>
//         public static async UniTask<TObject> LoadAssetAsync<TObject>(string packageName,
//                                                                      string location,
//                                                                      UProgress<float> progress = default(UProgress<float>))
//             where TObject : Object
//         {
//             AssetOperationHandle handle = EngineSystem<AssetSystem>.System.LoadAssetAsync<TObject>(packageName, location, progress);
//             await handle.ToUniTask(Root);
//             return handle.AssetObject as TObject;
//         }
//
//         /// <summary>
//         /// 异步实例化
//         /// </summary>
//         /// <param name="packageName"></param>
//         /// <param name="location"></param>
//         /// <param name="progress"></param>
//         /// <returns></returns>
//         public static async UniTask<GameObject> InstantiateAsync(string packageName,
//                                                                  string location,
//                                                                  UProgress<float> progress = default(UProgress<float>))
//         {
//             AssetOperationHandle handle = EngineSystem<AssetSystem>.System.LoadAssetAsync<GameObject>(packageName, location, progress);
//             await handle.ToUniTask(Root);
//             InstantiateOperation instantiateHandle = handle.InstantiateAsync(progress);
//             await instantiateHandle.ToUniTask(Root);
//             return instantiateHandle.Result;
//         }
//
//         /// <summary>
//         /// 异步加载资源对象
//         /// </summary>
//         /// <param name="packageName"></param>
//         /// <param name="location">资源的定位地址</param>
//         /// <param name="type">资源类型</param>
//         /// <param name="progress"></param>
//         public static async UniTask<Object> LoadAssetAsync(string packageName,
//                                                            string location,
//                                                            Type type,
//                                                            UProgress<float> progress = default(UProgress<float>))
//         {
//             AssetOperationHandle handle = EngineSystem<AssetSystem>.System.LoadAssetAsync(packageName, location, type, progress);
//             await handle.ToUniTask(Root);
//             return handle.AssetObject;
//         }
//
//         /// <summary>
//         /// 异步加载子资源对象
//         /// </summary>
//         /// <param name="packageName"></param>
//         /// <param name="assetInfo">资源信息</param>
//         /// <param name="progress"></param>
//         public static async UniTask<Object[]> LoadSubAssetsAsync(string packageName,
//                                                                  AssetInfo assetInfo,
//                                                                  UProgress<float> progress = default(UProgress<float>))
//         {
//             SubAssetsOperationHandle handle = EngineSystem<AssetSystem>.System.LoadSubAssetsAsync(packageName, assetInfo, progress);
//             await handle.ToUniTask(Root);
//             return handle.AllAssetObjects;
//         }
//
//         /// <summary>
//         /// 异步加载子资源对象
//         /// </summary>
//         /// <typeparam name="TObject">资源类型</typeparam>
//         /// <param name="packageName"></param>
//         /// <param name="location">资源的定位地址</param>
//         /// <param name="progress"></param>
//         public static async UniTask<TObject[]> LoadSubAssetAsync<TObject>(string packageName,
//                                                                           string location,
//                                                                           UProgress<float> progress = default(UProgress<float>))
//             where TObject : Object
//         {
//             SubAssetsOperationHandle handle = EngineSystem<AssetSystem>.System.LoadSubAssetsAsync<TObject>(packageName, location, progress);
//             await handle.ToUniTask(Root);
//             return handle.GetSubAssetObjects<TObject>();
//         }
//
//         /// <summary>
//         /// 异步加载子资源对象
//         /// </summary>
//         /// <param name="packageName"></param>
//         /// <param name="location">资源的定位地址</param>
//         /// <param name="type">子对象类型</param>
//         /// <param name="progress"></param>
//         public static async UniTask<Object[]> LoadSubAssetsAsync(string packageName,
//                                                                  string location,
//                                                                  Type type,
//                                                                  UProgress<float> progress)
//         {
//             SubAssetsOperationHandle handle = EngineSystem<AssetSystem>.System.LoadSubAssetsAsync(packageName, location, type, progress);
//             await handle.ToUniTask(Root);
//             return handle.AllAssetObjects;
//         }
//
//         /// <summary>
//         /// 获取资源包调试信息
//         /// </summary>
//         /// <returns></returns>
//         internal static DebugReport GetAssetDebugReport()
//         {
//             return EngineSystem<AssetSystem>.System.GetDebugReport();
//         }
//     }
// }
