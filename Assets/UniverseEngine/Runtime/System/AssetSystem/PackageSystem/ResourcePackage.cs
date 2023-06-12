using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace UniverseEngine
{
	public class ResourcePackage
	{
		bool m_IsInitialize;
		string m_InitializeError = string.Empty;
		EOperationStatus m_InitializeStatus = EOperationStatus.None;
		EPlayMode m_PlayMode;
		IBundleServices m_BundleServices;
		IPlayModeServices m_PlayModeServices;
		AssetSystemImpl m_AssetSystemImpl;

		/// <summary>
		/// 包裹名
		/// </summary>
		public readonly string PackageName;

		/// <summary>
		/// 初始化状态
		/// </summary>
		public EOperationStatus InitializeStatus => m_InitializeStatus;

		internal ResourcePackage(string packageName)
		{
			PackageName = packageName;
		}

		/// <summary>
		/// 更新资源包裹
		/// </summary>
		internal void UpdatePackage()
		{
			m_AssetSystemImpl?.Update();
		}

		/// <summary>
		/// 销毁资源包裹
		/// </summary>
		internal void DestroyPackage()
		{
			if (m_IsInitialize)
			{
				m_IsInitialize = false;
				m_InitializeError = string.Empty;
				m_InitializeStatus = EOperationStatus.None;
				m_BundleServices = null;
				m_PlayModeServices = null;

				if (m_AssetSystemImpl != null)
				{
					m_AssetSystemImpl.DestroyAll();
					m_AssetSystemImpl = null;
				}
			}
		}

		/// <summary>
		/// 异步初始化
		/// </summary>
		public InitializationOperation InitializeAsync(InitializeParameters parameters)
		{
			// 注意：WebGL平台因为网络原因可能会初始化失败！
			ResetInitializeAfterFailed();

			// 检测初始化参数合法性
			CheckInitializeParameters(parameters);

			// 初始化资源系统
			InitializationOperation initializeOperation;
			m_AssetSystemImpl = new();
			if (m_PlayMode == EPlayMode.EditorSimulateMode)
			{
				EditorSimulateModeImpl editorSimulateModeImpl = new EditorSimulateModeImpl();
				m_BundleServices = editorSimulateModeImpl;
				m_PlayModeServices = editorSimulateModeImpl;
				m_AssetSystemImpl.Initialize(PackageName, true,
				                             parameters.LoadingMaxTimeSlice, parameters.DownloadFailedTryAgain,
				                             parameters.DecryptionServices, m_BundleServices);

				EditorSimulateModeParameters initializeParameters = parameters as EditorSimulateModeParameters;
				initializeOperation = editorSimulateModeImpl.InitializeAsync(initializeParameters.LocationToLower, initializeParameters.SimulateManifestFilePath);
			}
			else if (m_PlayMode == EPlayMode.OfflinePlayMode)
			{
				OfflinePlayModeImpl offlinePlayModeImpl = new OfflinePlayModeImpl();
				m_BundleServices = offlinePlayModeImpl;
				m_PlayModeServices = offlinePlayModeImpl;
				m_AssetSystemImpl.Initialize(PackageName, false,
				                             parameters.LoadingMaxTimeSlice, parameters.DownloadFailedTryAgain,
				                             parameters.DecryptionServices, m_BundleServices);

				OfflinePlayModeParameters initializeParameters = parameters as OfflinePlayModeParameters;
				initializeOperation = offlinePlayModeImpl.InitializeAsync(PackageName, initializeParameters.LocationToLower);
			}
			else if (m_PlayMode == EPlayMode.HostPlayMode)
			{
				HostPlayModeImpl hostPlayModeImpl = new HostPlayModeImpl();
				m_BundleServices = hostPlayModeImpl;
				m_PlayModeServices = hostPlayModeImpl;
				m_AssetSystemImpl.Initialize(PackageName, false,
				                             parameters.LoadingMaxTimeSlice, parameters.DownloadFailedTryAgain,
				                             parameters.DecryptionServices, m_BundleServices);

				HostPlayModeParameters initializeParameters = parameters as HostPlayModeParameters;
				initializeOperation = hostPlayModeImpl.InitializeAsync(PackageName,
				                                                       initializeParameters.LocationToLower,
				                                                       initializeParameters.DefaultHostServer,
				                                                       initializeParameters.FallbackHostServer,
				                                                       initializeParameters.QueryServices);
			}
			else
			{
				throw new NotImplementedException();
			}

			// 监听初始化结果
			m_IsInitialize = true;
			initializeOperation.Completed += InitializeOperation_Completed;
			return initializeOperation;
		}
		void ResetInitializeAfterFailed()
		{
			if (m_IsInitialize && m_InitializeStatus == EOperationStatus.Failed)
			{
				m_IsInitialize = false;
				m_InitializeStatus = EOperationStatus.None;
				m_InitializeError = string.Empty;
				m_BundleServices = null;
				m_PlayModeServices = null;
				m_AssetSystemImpl = null;
			}
		}
		void CheckInitializeParameters(InitializeParameters parameters)
		{
			if (m_IsInitialize)
				throw new($"{nameof(ResourcePackage)} is initialized yet.");

			if (parameters == null)
				throw new($"{nameof(ResourcePackage)} create parameters is null.");

		#if !UNITY_EDITOR
			if (parameters is EditorSimulateModeParameters)
				throw new Exception($"Editor simulate mode only support unity editor.");
		#endif

			if (parameters is EditorSimulateModeParameters)
			{
				EditorSimulateModeParameters editorSimulateModeParameters = parameters as EditorSimulateModeParameters;
				if (string.IsNullOrEmpty(editorSimulateModeParameters.SimulateManifestFilePath))
					throw new($"{nameof(editorSimulateModeParameters.SimulateManifestFilePath)} is null or empty.");
			}

			if (parameters is HostPlayModeParameters)
			{
				HostPlayModeParameters hostPlayModeParameters = parameters as HostPlayModeParameters;
				if (string.IsNullOrEmpty(hostPlayModeParameters.DefaultHostServer))
					throw new($"${hostPlayModeParameters.DefaultHostServer} is null or empty.");
				if (string.IsNullOrEmpty(hostPlayModeParameters.FallbackHostServer))
					throw new($"${hostPlayModeParameters.FallbackHostServer} is null or empty.");
				if (hostPlayModeParameters.QueryServices == null)
					throw new($"{nameof(IQueryServices)} is null.");
			}

			// 鉴定运行模式
			if (parameters is EditorSimulateModeParameters)
				m_PlayMode = EPlayMode.EditorSimulateMode;
			else if (parameters is OfflinePlayModeParameters)
				m_PlayMode = EPlayMode.OfflinePlayMode;
			else if (parameters is HostPlayModeParameters)
				m_PlayMode = EPlayMode.HostPlayMode;
			else
				throw new NotImplementedException();

			// 检测参数范围
			if (parameters.LoadingMaxTimeSlice < 10)
			{
				parameters.LoadingMaxTimeSlice = 10;
				Log<AssetSystem>.Warning($"{nameof(parameters.LoadingMaxTimeSlice)} minimum value is 10 milliseconds.");
			}
			if (parameters.DownloadFailedTryAgain < 1)
			{
				parameters.DownloadFailedTryAgain = 1;
				Log<AssetSystem>.Warning($"{nameof(parameters.DownloadFailedTryAgain)} minimum value is 1");
			}
		}
		void InitializeOperation_Completed(AsyncOperationBase op)
		{
			m_InitializeStatus = op.Status;
			m_InitializeError = op.Error;
		}

		/// <summary>
		/// 向网络端请求最新的资源版本
		/// </summary>
		/// <param name="appendTimeTicks">在URL末尾添加时间戳</param>
		/// <param name="timeout">超时时间（默认值：60秒）</param>
		public UpdatePackageVersionOperation UpdatePackageVersionAsync(bool appendTimeTicks = true, int timeout = 60)
		{
			DebugCheckInitialize();
			return m_PlayModeServices.UpdatePackageVersionAsync(appendTimeTicks, timeout);
		}

		/// <summary>
		/// 向网络端请求并更新清单
		/// </summary>
		/// <param name="packageVersion">更新的包裹版本</param>
		/// <param name="autoSaveVersion">更新成功后自动保存版本号，作为下次初始化的版本。</param>
		/// <param name="timeout">超时时间（默认值：60秒）</param>
		public UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageVersion, bool autoSaveVersion = true, int timeout = 60)
		{
			DebugCheckInitialize();
			DebugCheckUpdateManifest();
			return m_PlayModeServices.UpdatePackageManifestAsync(packageVersion, autoSaveVersion, timeout);
		}

		/// <summary>
		/// 预下载指定版本的包裹资源
		/// </summary>
		/// <param name="packageVersion">下载的包裹版本</param>
		/// <param name="timeout">超时时间（默认值：60秒）</param>
		public PreDownloadContentOperation PreDownloadContentAsync(string packageVersion, int timeout = 60)
		{
			DebugCheckInitialize();
			return m_PlayModeServices.PreDownloadContentAsync(packageVersion, timeout);
		}

		/// <summary>
		/// 清理包裹未使用的缓存文件
		/// </summary>
		public ClearUnusedCacheFilesOperation ClearUnusedCacheFilesAsync()
		{
			DebugCheckInitialize();
			ClearUnusedCacheFilesOperation operation = new ClearUnusedCacheFilesOperation(this);
			OperationSystem.StartOperation(operation);
			return operation;
		}

		/// <summary>
		/// 清理包裹本地所有的缓存文件
		/// </summary>
		public ClearAllCacheFilesOperation ClearAllCacheFilesAsync()
		{
			DebugCheckInitialize();
			ClearAllCacheFilesOperation operation = new ClearAllCacheFilesOperation(this);
			OperationSystem.StartOperation(operation);
			return operation;
		}

		/// <summary>
		/// 获取本地包裹的版本信息
		/// </summary>
		public string GetPackageVersion()
		{
			DebugCheckInitialize();
			if (m_PlayModeServices.ActiveManifest == null)
				return string.Empty;
			return m_PlayModeServices.ActiveManifest.PackageVersion;
		}

		/// <summary>
		/// 资源回收（卸载引用计数为零的资源）
		/// </summary>
		public void UnloadUnusedAssets()
		{
			DebugCheckInitialize();
			m_AssetSystemImpl.Update();
			m_AssetSystemImpl.UnloadUnusedAssets();
		}

		/// <summary>
		/// 强制回收所有资源
		/// </summary>
		public void ForceUnloadAllAssets()
		{
			DebugCheckInitialize();
			m_AssetSystemImpl.ForceUnloadAllAssets();
		}


	#region 资源信息

		/// <summary>
		/// 是否需要从远端更新下载
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public bool IsNeedDownloadFromRemote(string location)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
			if (assetInfo.IsInvalid)
			{
				Log<AssetSystem>.Warning(assetInfo.Error);
				return false;
			}

			BundleInfo bundleInfo = m_BundleServices.GetBundleInfo(assetInfo);
			if (bundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromRemote)
				return true;
			else
				return false;
		}

		/// <summary>
		/// 是否需要从远端更新下载
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public bool IsNeedDownloadFromRemote(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			if (assetInfo.IsInvalid)
			{
				Log<AssetSystem>.Warning(assetInfo.Error);
				return false;
			}

			BundleInfo bundleInfo = m_BundleServices.GetBundleInfo(assetInfo);
			if (bundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromRemote)
				return true;
			else
				return false;
		}

		/// <summary>
		/// 获取资源信息列表
		/// </summary>
		/// <param name="tag">资源标签</param>
		/// <param name="result"></param>
		public void GetAssetInfos(string tag, List<AssetInfo> result)
		{
			DebugCheckInitialize();
			string[] tags = { tag };
			m_PlayModeServices.ActiveManifest.GetAssetsInfoByTags(tags, result);
		}

		/// <summary>
		/// 获取资源信息列表
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		/// <param name="result"></param>
		public void GetAssetInfos(string[] tags, List<AssetInfo> result)
		{
			DebugCheckInitialize();
			m_PlayModeServices.ActiveManifest.GetAssetsInfoByTags(tags, result);
		}

		/// <summary>
		/// 获取资源信息
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public AssetInfo GetAssetInfo(string location)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
			return assetInfo;
		}

		/// <summary>
		/// 检查资源定位地址是否有效
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public bool CheckLocationValid(string location)
		{
			DebugCheckInitialize();
			string assetPath = m_PlayModeServices.ActiveManifest.TryMappingToAssetPath(location);
			return string.IsNullOrEmpty(assetPath) == false;
		}

	#endregion

	#region 原生文件

		/// <summary>
		/// 同步加载原生文件
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public RawFileOperationHandle LoadRawFileSync(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			return LoadRawFileInternal(assetInfo, true);
		}

		/// <summary>
		/// 同步加载原生文件
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public RawFileOperationHandle LoadRawFileSync(string location)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
			return LoadRawFileInternal(assetInfo, true);
		}

		/// <summary>
		/// 异步加载原生文件
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public RawFileOperationHandle LoadRawFileAsync(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			return LoadRawFileInternal(assetInfo, false);
		}

		/// <summary>
		/// 异步加载原生文件
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public RawFileOperationHandle LoadRawFileAsync(string location)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
			return LoadRawFileInternal(assetInfo, false);
		}


		RawFileOperationHandle LoadRawFileInternal(AssetInfo assetInfo, bool waitForAsyncComplete)
		{
		#if UNITY_EDITOR
			if (assetInfo.IsInvalid == false)
			{
				BundleInfo bundleInfo = m_BundleServices.GetBundleInfo(assetInfo);
				if (bundleInfo.Bundle.IsRawFile == false)
					throw new($"Cannot load asset bundle file using {nameof(LoadRawFileAsync)} method !");
			}
		#endif

			RawFileOperationHandle handle = m_AssetSystemImpl.LoadRawFileAsync(assetInfo);
			if (waitForAsyncComplete)
				handle.WaitForAsyncComplete();
			return handle;
		}

	#endregion

	#region 场景加载

		/// <summary>
		/// 异步加载场景
		/// </summary>
		/// <param name="location">场景的定位地址</param>
		/// <param name="sceneMode">场景加载模式</param>
		/// <param name="activateOnLoad">加载完毕时是否主动激活</param>
		/// <param name="priority">优先级</param>
		public SceneOperationHandle LoadSceneAsync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
			SceneOperationHandle handle = m_AssetSystemImpl.LoadSceneAsync(assetInfo, sceneMode, activateOnLoad, priority);
			return handle;
		}

		/// <summary>
		/// 异步加载场景
		/// </summary>
		/// <param name="assetInfo">场景的资源信息</param>
		/// <param name="sceneMode">场景加载模式</param>
		/// <param name="activateOnLoad">加载完毕时是否主动激活</param>
		/// <param name="priority">优先级</param>
		public SceneOperationHandle LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
		{
			DebugCheckInitialize();
			SceneOperationHandle handle = m_AssetSystemImpl.LoadSceneAsync(assetInfo, sceneMode, activateOnLoad, priority);
			return handle;
		}

	#endregion

	#region 资源加载

		/// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public AssetOperationHandle LoadAssetSync(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			return LoadAssetInternal(assetInfo, true);
		}

		/// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public AssetOperationHandle LoadAssetSync<TObject>(string location) where TObject : UnityEngine.Object
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
			return LoadAssetInternal(assetInfo, true);
		}

		/// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">资源类型</param>
		public AssetOperationHandle LoadAssetSync(string location, System.Type type)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
			return LoadAssetInternal(assetInfo, true);
		}


		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public AssetOperationHandle LoadAssetAsync(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			return LoadAssetInternal(assetInfo, false);
		}

		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public AssetOperationHandle LoadAssetAsync<TObject>(string location) where TObject : UnityEngine.Object
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
			return LoadAssetInternal(assetInfo, false);
		}

		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">资源类型</param>
		public AssetOperationHandle LoadAssetAsync(string location, System.Type type)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
			return LoadAssetInternal(assetInfo, false);
		}


		AssetOperationHandle LoadAssetInternal(AssetInfo assetInfo, bool waitForAsyncComplete)
		{
		#if UNITY_EDITOR
			if (assetInfo.IsInvalid == false)
			{
				BundleInfo bundleInfo = m_BundleServices.GetBundleInfo(assetInfo);
				if (bundleInfo.Bundle.IsRawFile)
					throw new($"Cannot load raw file using {nameof(LoadAssetAsync)} method !");
			}
		#endif

			AssetOperationHandle handle = m_AssetSystemImpl.LoadAssetAsync(assetInfo);
			if (waitForAsyncComplete)
				handle.WaitForAsyncComplete();
			return handle;
		}

	#endregion

	#region 资源加载

		/// <summary>
		/// 同步加载子资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public SubAssetsOperationHandle LoadSubAssetsSync(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			return LoadSubAssetsInternal(assetInfo, true);
		}

		/// <summary>
		/// 同步加载子资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public SubAssetsOperationHandle LoadSubAssetsSync<TObject>(string location) where TObject : UnityEngine.Object
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
			return LoadSubAssetsInternal(assetInfo, true);
		}

		/// <summary>
		/// 同步加载子资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">子对象类型</param>
		public SubAssetsOperationHandle LoadSubAssetsSync(string location, System.Type type)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
			return LoadSubAssetsInternal(assetInfo, true);
		}


		/// <summary>
		/// 异步加载子资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public SubAssetsOperationHandle LoadSubAssetsAsync(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			return LoadSubAssetsInternal(assetInfo, false);
		}

		/// <summary>
		/// 异步加载子资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public SubAssetsOperationHandle LoadSubAssetsAsync<TObject>(string location) where TObject : UnityEngine.Object
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
			return LoadSubAssetsInternal(assetInfo, false);
		}

		/// <summary>
		/// 异步加载子资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">子对象类型</param>
		public SubAssetsOperationHandle LoadSubAssetsAsync(string location, System.Type type)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
			return LoadSubAssetsInternal(assetInfo, false);
		}


		SubAssetsOperationHandle LoadSubAssetsInternal(AssetInfo assetInfo, bool waitForAsyncComplete)
		{
		#if UNITY_EDITOR
			if (assetInfo.IsInvalid == false)
			{
				BundleInfo bundleInfo = m_BundleServices.GetBundleInfo(assetInfo);
				if (bundleInfo.Bundle.IsRawFile)
					throw new($"Cannot load raw file using {nameof(LoadSubAssetsAsync)} method !");
			}
		#endif

			SubAssetsOperationHandle handle = m_AssetSystemImpl.LoadSubAssetsAsync(assetInfo);
			if (waitForAsyncComplete)
				handle.WaitForAsyncComplete();
			return handle;
		}

	#endregion

	#region 资源下载

		/// <summary>
		/// 创建资源下载器，用于下载当前资源版本所有的资源包文件
		/// </summary>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			DebugCheckInitialize();
			return m_PlayModeServices.CreateResourceDownloaderByAll(downloadingMaxNumber, failedTryAgain, timeout);
		}

		/// <summary>
		/// 创建资源下载器，用于下载指定的资源标签关联的资源包文件
		/// </summary>
		/// <param name="tag">资源标签</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public ResourceDownloaderOperation CreateResourceDownloader(string tag, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			DebugCheckInitialize();
			return m_PlayModeServices.CreateResourceDownloaderByTags(new string[] { tag }, downloadingMaxNumber, failedTryAgain, timeout);
		}

		/// <summary>
		/// 创建资源下载器，用于下载指定的资源标签列表关联的资源包文件
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public ResourceDownloaderOperation CreateResourceDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			DebugCheckInitialize();
			return m_PlayModeServices.CreateResourceDownloaderByTags(tags, downloadingMaxNumber, failedTryAgain, timeout);
		}

		/// <summary>
		/// 创建资源下载器，用于下载指定的资源依赖的资源包文件
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public ResourceDownloaderOperation CreateBundleDownloader(string location, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
			AssetInfo[] assetInfos = new AssetInfo[] { assetInfo };
			return m_PlayModeServices.CreateResourceDownloaderByPaths(assetInfos, downloadingMaxNumber, failedTryAgain, timeout);
		}

		/// <summary>
		/// 创建资源下载器，用于下载指定的资源列表依赖的资源包文件
		/// </summary>
		/// <param name="locations">资源的定位地址列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public ResourceDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			DebugCheckInitialize();
			List<AssetInfo> assetInfos = new(locations.Length);
			foreach (string location in locations)
			{
				AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
				assetInfos.Add(assetInfo);
			}
			return m_PlayModeServices.CreateResourceDownloaderByPaths(assetInfos.ToArray(), downloadingMaxNumber, failedTryAgain, timeout);
		}

		/// <summary>
		/// 创建资源下载器，用于下载指定的资源依赖的资源包文件
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public ResourceDownloaderOperation CreateBundleDownloader(AssetInfo assetInfo, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			DebugCheckInitialize();
			AssetInfo[] assetInfos = new AssetInfo[] { assetInfo };
			return m_PlayModeServices.CreateResourceDownloaderByPaths(assetInfos, downloadingMaxNumber, failedTryAgain, timeout);
		}

		/// <summary>
		/// 创建资源下载器，用于下载指定的资源列表依赖的资源包文件
		/// </summary>
		/// <param name="assetInfos">资源信息列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public ResourceDownloaderOperation CreateBundleDownloader(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			DebugCheckInitialize();
			return m_PlayModeServices.CreateResourceDownloaderByPaths(assetInfos, downloadingMaxNumber, failedTryAgain, timeout);
		}

	#endregion

	#region 资源解压

		/// <summary>
		/// 创建内置资源解压器
		/// </summary>
		/// <param name="tag">资源标签</param>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public ResourceUnpackerOperation CreateResourceUnpacker(string tag, int unpackingMaxNumber, int failedTryAgain)
		{
			DebugCheckInitialize();
			return m_PlayModeServices.CreateResourceUnpackerByTags(new string[] { tag }, unpackingMaxNumber, failedTryAgain, int.MaxValue);
		}

		/// <summary>
		/// 创建内置资源解压器
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public ResourceUnpackerOperation CreateResourceUnpacker(string[] tags, int unpackingMaxNumber, int failedTryAgain)
		{
			DebugCheckInitialize();
			return m_PlayModeServices.CreateResourceUnpackerByTags(tags, unpackingMaxNumber, failedTryAgain, int.MaxValue);
		}

		/// <summary>
		/// 创建内置资源解压器
		/// </summary>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public ResourceUnpackerOperation CreateResourceUnpacker(int unpackingMaxNumber, int failedTryAgain)
		{
			DebugCheckInitialize();
			return m_PlayModeServices.CreateResourceUnpackerByAll(unpackingMaxNumber, failedTryAgain, int.MaxValue);
		}

	#endregion

	#region 内部方法

		/// <summary>
		/// 是否包含资源文件
		/// </summary>
		internal bool IsIncludeBundleFile(string cacheGuid)
		{
			// NOTE : 编辑器模拟模式下始终返回TRUE
			if (m_PlayMode == EPlayMode.EditorSimulateMode)
				return true;
			return m_PlayModeServices.ActiveManifest.IsIncludeBundleFile(cacheGuid);
		}

		/// <summary>
		/// 资源定位地址转换为资源信息类
		/// </summary>
		AssetInfo ConvertLocationToAssetInfo(string location, System.Type assetType)
		{
			return m_PlayModeServices.ActiveManifest.ConvertLocationToAssetInfo(location, assetType);
		}

	#endregion

	#region 调试方法

		[Conditional("DEBUG")]
		void DebugCheckInitialize()
		{
			if (m_InitializeStatus == EOperationStatus.None)
				throw new("Package initialize not completed !");
			else if (m_InitializeStatus == EOperationStatus.Failed)
				throw new($"Package initialize failed ! {m_InitializeError}");
		}

		[Conditional("DEBUG")]
		void DebugCheckUpdateManifest()
		{
			List<BundleInfo> loadedBundleInfos = m_AssetSystemImpl.GetLoadedBundleInfos();
			if (loadedBundleInfos.Count > 0)
			{
				Log<AssetSystem>.Warning($"Found loaded bundle before update manifest ! Recommended to call the  {nameof(ForceUnloadAllAssets)} method to release loaded bundle !");
			}
		}

	#endregion

	#region 调试信息

		internal DebugPackageData GetDebugPackageData()
		{
			DebugPackageData data = new();
			data.PackageName = PackageName;
			data.ProviderInfos = m_AssetSystemImpl.GetDebugReportInfos();
			return data;
		}

	#endregion
	}
}
