using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniverseEngine
{
	public class AssetSystem : EngineSystem
	{
		static readonly Dictionary<string, ResourcePackage> s_Packages = new();

		public static ResourcePackage CreatePackage(string packageName)
		{
			if (string.IsNullOrEmpty(packageName))
			{
				Log<AssetSystem>.Error("Package name is null or empty !");
				return null;
			}

			if (TryGetPackage(packageName, out ResourcePackage package))
			{
				Log<AssetSystem>.Error($"Package {packageName} already existed !");
				return package;
			}

			Log<AssetSystem>.Info($"Create resource package : {packageName}");
			package = new(packageName);
			s_Packages[packageName] = package;
			return package;
		}

		public static bool TryGetPackage(string packageName, out ResourcePackage package)
		{
			package = null;
			if (string.IsNullOrEmpty(packageName))
			{
				Log<AssetSystem>.Error("Package name is null or empty !");
				return false;
			}

			return s_Packages.TryGetValue(packageName, out package);
		}

		/// <summary>
		/// 销毁资源包
		/// </summary>
		/// <param name="packageName">资源包名称</param>
		public static void DestroyPackage(string packageName)
		{
			if (!TryGetPackage(packageName, out ResourcePackage package))
			{
				return;
			}

			Log<AssetSystem>.Info($"Destroy resource package : {packageName}");
			s_Packages.Remove(packageName);
			package.DestroyPackage();
			// 清空缓存
			AssetCacheSystem.ClearPackage(packageName);
		}

		/// <summary>
		/// 检测资源包是否存在
		/// </summary>
		/// <param name="packageName">资源包名称</param>
		public static bool ExistPackage(string packageName)
		{
			return !string.IsNullOrEmpty(packageName) && s_Packages.ContainsKey(packageName);
		}

		public static bool IsNeedDownloadFromRemote(string packageName, string location)
		{
			if (!TryGetPackage(packageName, out ResourcePackage package))
			{
				return false;
			}

			return package.IsNeedDownloadFromRemote(location);
		}

		public static bool IsNeedDownloadFromRemote(string packageName, AssetInfo assetInfo)
		{
			if (!TryGetPackage(packageName, out ResourcePackage package))
			{
				return false;
			}

			return package.IsNeedDownloadFromRemote(assetInfo);
		}

		public static void GetAssetInfos(string packageName, string tag, List<AssetInfo> result)
		{
			if (!TryGetPackage(packageName, out ResourcePackage package))
			{
				return;
			}

			package.GetAssetInfos(tag, result);
		}

		public static void GetAssetInfos(string packageName, string[] tags, List<AssetInfo> result)
		{
			if (!TryGetPackage(packageName, out ResourcePackage package))
			{
				return;
			}

			package.GetAssetInfos(tags, result);
		}

		public static AssetInfo GetAssetInfo(string packageName, string location)
		{
			if (!TryGetPackage(packageName, out ResourcePackage package))
			{
				return default;
			}

			return package.GetAssetInfo(location);
		}

		public static bool CheckLocationValid(string packageName, string location)
		{
			if (!TryGetPackage(packageName, out ResourcePackage package))
			{
				return false;
			}

			return package.CheckLocationValid(location);
		}

		public override void OnInit()
		{
			OperationSystem.Initialize();
			DownloadSystem.Initialize();
		}

		public override void OnDestroy()
		{
			OperationSystem.DestroyAll();
			DownloadSystem.DestroyAll();

			foreach (ResourcePackage package in s_Packages.Values)
			{
				package.DestroyPackage();
			}
			s_Packages.Clear();
		}

		public override void OnUpdate(float dt)
		{
			OperationSystem.Update();
			DownloadSystem.Update();

			foreach (ResourcePackage package in s_Packages.Values)
			{
				package.UpdatePackage();
			}
		}

	#region 系统参数

		/// <summary>
		/// 设置下载系统参数，启用断点续传功能文件的最小字节数
		/// </summary>
		public static void SetDownloadSystemBreakpointResumeFileSize(int fileBytes)
		{
			DownloadSystem.BreakpointResumeFileSize = fileBytes;
		}

		/// <summary>
		/// 设置下载系统参数，下载失败后清理文件的HTTP错误码
		/// </summary>
		public static void SetDownloadSystemClearFileResponseCode(List<long> codes)
		{
			DownloadSystem.ClearFileResponseCodes = codes;
		}

		/// <summary>
		/// 设置下载系统参数，自定义的证书认证实例
		/// </summary>
		public static void SetDownloadSystemCertificateHandler(UnityEngine.Networking.CertificateHandler instance)
		{
			DownloadSystem.CertificateHandlerInstance = instance;
		}

		/// <summary>
		/// 设置下载系统参数，自定义下载请求
		/// </summary>
		public static void SetDownloadSystemUnityWebRequest(DownloadRequestDelegate requestDelegate)
		{
			DownloadSystem.RequestDelegate = requestDelegate;
		}

		/// <summary>
		/// 设置异步系统参数，每帧执行消耗的最大时间切片（单位：毫秒）
		/// </summary>
		public static void SetOperationSystemMaxTimeSlice(long milliseconds)
		{
			if (milliseconds < 10)
			{
				milliseconds = 10;
				Log<AssetSystem>.Warning($"MaxTimeSlice minimum value is 10 milliseconds.");
			}
			OperationSystem.MaxTimeSlice = milliseconds;
		}

		/// <summary>
		/// 设置缓存系统参数，已经缓存文件的校验等级
		/// </summary>
		public static void SetCacheSystemCachedFileVerifyLevel(EVerifyLevel verifyLevel)
		{
			AssetCacheSystem.CacheFileVerifyLevel = verifyLevel;
		}

	#endregion

	#region 调试信息

		internal static DebugReport GetDebugReport()
		{
			DebugReport report = new()
			{
				FrameCount = Time.frameCount
			};

			foreach (ResourcePackage package in s_Packages.Values)
			{
				DebugPackageData packageData = package.GetDebugPackageData();
				report.PackageDatas.Add(packageData);
			}
			return report;
		}

	#endregion

		private static ResourcePackage _defaultPackage;

		/// <summary>
		/// 设置默认的资源包
		/// </summary>
		public static void SetDefaultPackage(ResourcePackage package)
		{
			_defaultPackage = package;
		}


	#region 原生文件

		public static RawFileOperationHandle LoadRawFileSync(AssetInfo assetInfo)
		{

			return _defaultPackage.LoadRawFileSync(assetInfo);
		}

		public static RawFileOperationHandle LoadRawFileSync(string location)
		{

			return _defaultPackage.LoadRawFileSync(location);
		}

		public static RawFileOperationHandle LoadRawFileAsync(AssetInfo assetInfo)
		{

			return _defaultPackage.LoadRawFileAsync(assetInfo);
		}

		public static RawFileOperationHandle LoadRawFileAsync(string location)
		{

			return _defaultPackage.LoadRawFileAsync(location);
		}

	#endregion

	#region 场景加载

		public static SceneOperationHandle LoadSceneAsync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
		{

			return _defaultPackage.LoadSceneAsync(location, sceneMode, activateOnLoad, priority);
		}

		public static SceneOperationHandle LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
		{

			return _defaultPackage.LoadSceneAsync(assetInfo, sceneMode, activateOnLoad, priority);
		}

	#endregion

	#region 资源加载

		public static AssetOperationHandle LoadAssetSync(AssetInfo assetInfo)
		{

			return _defaultPackage.LoadAssetSync(assetInfo);
		}

		public static AssetOperationHandle LoadAssetSync<TObject>(string location) where TObject : UnityEngine.Object
		{

			return _defaultPackage.LoadAssetSync<TObject>(location);
		}

		public static AssetOperationHandle LoadAssetSync(string location, System.Type type)
		{

			return _defaultPackage.LoadAssetSync(location, type);
		}

		public static AssetOperationHandle LoadAssetAsync(AssetInfo assetInfo)
		{

			return _defaultPackage.LoadAssetAsync(assetInfo);
		}

		public static AssetOperationHandle LoadAssetAsync<TObject>(string location) where TObject : UnityEngine.Object
		{

			return _defaultPackage.LoadAssetAsync<TObject>(location);
		}

		public static AssetOperationHandle LoadAssetAsync(string location, System.Type type)
		{

			return _defaultPackage.LoadAssetAsync(location, type);
		}

	#endregion

	#region 资源加载

		public static SubAssetsOperationHandle LoadSubAssetsSync(AssetInfo assetInfo)
		{

			return _defaultPackage.LoadSubAssetsSync(assetInfo);
		}

		public static SubAssetsOperationHandle LoadSubAssetsSync<TObject>(string location) where TObject : UnityEngine.Object
		{

			return _defaultPackage.LoadSubAssetsSync<TObject>(location);
		}

		public static SubAssetsOperationHandle LoadSubAssetsSync(string location, System.Type type)
		{

			return _defaultPackage.LoadSubAssetsSync(location, type);
		}

		public static SubAssetsOperationHandle LoadSubAssetsAsync(AssetInfo assetInfo)
		{

			return _defaultPackage.LoadSubAssetsAsync(assetInfo);
		}

		public static SubAssetsOperationHandle LoadSubAssetsAsync<TObject>(string location) where TObject : UnityEngine.Object
		{

			return _defaultPackage.LoadSubAssetsAsync<TObject>(location);
		}

		public static SubAssetsOperationHandle LoadSubAssetsAsync(string location, System.Type type)
		{

			return _defaultPackage.LoadSubAssetsAsync(location, type);
		}

	#endregion

	#region 资源下载

		public static ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain)
		{

			return _defaultPackage.CreateResourceDownloader(downloadingMaxNumber, failedTryAgain);
		}

		public static ResourceDownloaderOperation CreateResourceDownloader(string tag, int downloadingMaxNumber, int failedTryAgain)
		{

			return _defaultPackage.CreateResourceDownloader(new string[] { tag }, downloadingMaxNumber, failedTryAgain);
		}

		public static ResourceDownloaderOperation CreateResourceDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain)
		{

			return _defaultPackage.CreateResourceDownloader(tags, downloadingMaxNumber, failedTryAgain);
		}

		public static ResourceDownloaderOperation CreateBundleDownloader(string location, int downloadingMaxNumber, int failedTryAgain)
		{

			return _defaultPackage.CreateBundleDownloader(location, downloadingMaxNumber, failedTryAgain);
		}

		public static ResourceDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain)
		{

			return _defaultPackage.CreateBundleDownloader(locations, downloadingMaxNumber, failedTryAgain);
		}

		public static ResourceDownloaderOperation CreateBundleDownloader(AssetInfo assetInfo, int downloadingMaxNumber, int failedTryAgain)
		{

			return _defaultPackage.CreateBundleDownloader(assetInfo, downloadingMaxNumber, failedTryAgain);
		}

		public static ResourceDownloaderOperation CreateBundleDownloader(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain)
		{

			return _defaultPackage.CreateBundleDownloader(assetInfos, downloadingMaxNumber, failedTryAgain);
		}

	#endregion

	#region 资源解压

		public static ResourceUnpackerOperation CreateResourceUnpacker(string tag, int unpackingMaxNumber, int failedTryAgain)
		{

			return _defaultPackage.CreateResourceUnpacker(tag, unpackingMaxNumber, failedTryAgain);
		}

		public static ResourceUnpackerOperation CreateResourceUnpacker(string[] tags, int unpackingMaxNumber, int failedTryAgain)
		{

			return _defaultPackage.CreateResourceUnpacker(tags, unpackingMaxNumber, failedTryAgain);
		}

		public static ResourceUnpackerOperation CreateResourceUnpacker(int unpackingMaxNumber, int failedTryAgain)
		{

			return _defaultPackage.CreateResourceUnpacker(unpackingMaxNumber, failedTryAgain);
		}

	#endregion

		internal static async UniTask<PackageManifest> LoadCachePackageManifest(string packageName, string packageVersion)
		{
			if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(packageVersion))
			{
				return null;
			}

			string manifestFilePath = FileSystem.GetCachedManifestFilePath(packageName, packageVersion);
			string packageHash = await QueryCachePackageHash(packageName, packageVersion);
			if (string.IsNullOrEmpty(packageHash))
			{
				ClearCacheFile(manifestFilePath, packageName, packageVersion);
				return null;
			}

			if (!File.Exists(manifestFilePath))
			{
				Log<AssetSystem>.Error($"Cache Package [{packageName}-{packageVersion}] Not found cache manifest file : {manifestFilePath}");
				return null;
			}

			string fileHash = HashUtilities.FileMD5(manifestFilePath);
			if (fileHash != packageHash)
			{
				Log<AssetSystem>.Error($"Cache Package [{packageName}-{packageVersion}] Failed to verify cache manifest file hash");
				ClearCacheFile(manifestFilePath, packageName, packageVersion);
				return null;
			}

			PackageManifest manifest = await DeserializeManifest(manifestFilePath);
			if (manifest == null)
			{
				ClearCacheFile(manifestFilePath, packageName, packageVersion);
				return null;
			}

			Log<AssetSystem>.Info($"Cache Package [{packageName}-{packageVersion}] load cache package manifest at : {manifestFilePath}");
			return manifest;
		}

		internal static async UniTask<string> QueryBuiltInPackageVersion(string packageName)
		{
			if (string.IsNullOrEmpty(packageName))
			{
				return string.Empty;
			}

			string filePath = FileSystem.GetBuiltInPackageVersionFilePath(packageName);
			string url = FileSystem.ToWWWPath(filePath);
			string version = await HttpSystem.RequestText(url);
			if (string.IsNullOrEmpty(version))
			{
				Log<AssetSystem>.Error($"Package [{packageName}] built-in version file content is empty ");
			}

			Log<AssetSystem>.Info($"Package [{packageName}] query built-in version : {version}");
			return version;
		}

		internal static async UniTask<string> QueryCachePackageVersion(string packageName)
		{
			if (string.IsNullOrEmpty(packageName))
			{
				return string.Empty;
			}

			string filePath = FileSystem.GetCachedPackageVersionFilePath(packageName);
			if (!File.Exists(filePath))
			{
				Log<AssetSystem>.Error($"Cache package [{packageName}] version file not found : {filePath}");
				return string.Empty;
			}

			string version = await FileSystem.ReadAllTextAsync(filePath);
			if (string.IsNullOrEmpty(version))
			{
				Log<AssetSystem>.Error($"Cache package [{packageName}] version file content is empty !");
				return string.Empty;
			}

			Log<AssetSystem>.Info($"Cache Package [{packageName}] query cached version : {version}");
			return version;
		}

		internal static async UniTask<string> QueryCachePackageHash(string packageName, string packageVersion)
		{
			if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(packageVersion))
			{
				return string.Empty;
			}

			string filePath = FileSystem.GetCachedPackageHashFilePath(packageName, packageVersion);
			if (!File.Exists(filePath))
			{
				Log<AssetSystem>.Error($"Cache package [{packageName}] hash file not found : {filePath}");
				return string.Empty;
			}

			string hash = await FileSystem.ReadAllTextAsync(filePath);
			if (string.IsNullOrEmpty(hash))
			{
				Log<AssetSystem>.Error($"Cache package [{packageName}-{packageVersion}] hash file content is empty");
				return string.Empty;
			}

			Log<AssetSystem>.Info($"Cache Package [{packageName}-{packageVersion}] query cached hash : {hash}");
			return hash;
		}

		internal static async UniTask<bool> UnpackBuiltInManifest(string packageName, string packageVersion)
		{
			if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(packageVersion))
			{
				return false;
			}

			string hashSavePath = FileSystem.GetCachedPackageHashFilePath(packageName, packageVersion);
			string builtInHashFilePath = FileSystem.GetBuiltInPackageHashFilePath(packageName, packageVersion);
			string hashUrl = FileSystem.ToWWWPath(builtInHashFilePath);
			string hash = await HttpSystem.DownloadFileText(hashUrl, hashSavePath);
			if (string.IsNullOrEmpty(hash))
			{
				return false;
			}

			string manifestSavePath = FileSystem.GetCachedManifestFilePath(packageName, packageVersion);
			string builtInManifestFilePath = FileSystem.GetBuiltInManifestBinaryFilePath(packageName, packageVersion);
			string manifestUrl = FileSystem.ToWWWPath(builtInManifestFilePath);
			byte[] bytes = await HttpSystem.DownloadFileBytes(manifestUrl, manifestSavePath);
			if (bytes == null)
			{
				return false;
			}

			Log<AssetSystem>.Info($"Package [{packageName}-{packageVersion}] unpack built-in manifest");
			return true;
		}

		internal static async UniTask<PackageManifest> DeserializeManifest(string packageName, string packageVersion)
		{
			if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(packageVersion))
			{
				return null;
			}

			string filePath = FileSystem.GetBuiltInManifestBinaryFilePath(packageName, packageVersion);
			string url = FileSystem.ToWWWPath(filePath);
			byte[] bytes = await HttpSystem.RequestBytes(url);
			PackageManifest manifest = await DeserializeManifest(bytes);
			if (manifest != null)
			{
				Log<AssetSystem>.Info($"Package [{packageName}-{packageVersion}] deserialize manifest with url : {url}");
			}

			return manifest;
		}

		internal static async UniTask<PackageManifest> DeserializeManifest(string filePath)
		{
			byte[] bytesData = await FileSystem.ReadAllBytesAsync(filePath);
			return await DeserializeManifest(bytesData);
		}

		internal static async UniTask<PackageManifest> DeserializeManifest(byte[] bytes)
		{
			PackageManifest manifest = new();
			BufferReader buffer = new(bytes);
			(bool result, string error) = await manifest.ReadFromBufferAsync(buffer);
			if (result)
			{
				return manifest;
			}
			
			Log<AssetSystem>.Error(error);
			return null;
		}

		internal static void ClearCacheFile(string manifestFilePath, string packageName, string packageVersion)
		{
			// 注意：如果加载沙盒内的清单报错，为了避免流程被卡住，主动把损坏的文件删除。
			if (File.Exists(manifestFilePath))
			{
				Log<AssetSystem>.Warning($"Invalid cache manifest file have been removed : {manifestFilePath}");
				File.Delete(manifestFilePath);
			}

			string hashFilePath = FileSystem.GetCachedPackageHashFilePath(packageName, packageVersion);
			if (File.Exists(hashFilePath))
			{
				File.Delete(hashFilePath);
			}
		}
	}
}
