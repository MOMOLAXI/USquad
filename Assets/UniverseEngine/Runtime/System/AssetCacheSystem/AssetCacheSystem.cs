using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace UniverseEngine
{
	internal class AssetCacheSystem : EngineSystem
	{
		/// <summary>
		/// AssetPackageName -> Cache
		/// </summary>
		static readonly Dictionary<string, AssetPackageCache> s_CachePackages = new();

		public static EVerifyLevel CacheFileVerifyLevel { set; get; } = EVerifyLevel.Middle;

		public static async UniTask CacheAssetPackageAsync(string packageName)
		{
			if (string.IsNullOrEmpty(packageName))
			{
				return;
			}

			ThreadPool.GetMaxThreads(out int workerThreadsCount, out int ioThreadsCount);
			Log<AssetCacheSystem>.Info($"Cache AssetPackage [{packageName}] with "
			                         + $"worker threads [{workerThreadsCount.ToString()}] "
			                         + $"io threads [{ioThreadsCount.ToString()}]");

			await UniTask.SwitchToThreadPool();

			CacheBundleFiles(packageName);
			CacheRawFiles(packageName);

			await UniTask.SwitchToMainThread();
			
			int totalCount = GetCachePackageFileCount(packageName);
			Log<AssetCacheSystem>.Info($"Package '{packageName}' cached files count : {totalCount}");
		}

		public static async UniTask ClearUnusedCacheFiles(ResourcePackage package)
		{
			if (package == null)
			{
				return;
			}

			await UniTask.SwitchToThreadPool();
			using (Collections.AllocAutoList(out List<string> cacheGuids))
			{
				GetUnusedCacheGuids(package, cacheGuids);
				foreach (string guid in cacheGuids)
				{
					RemoveCacheFile(package.PackageName, guid);
				}
			}

			await UniTask.SwitchToMainThread();
		}

		public static async UniTask ClearAllCacheFiles(string packageName)
		{
			if (string.IsNullOrEmpty(packageName))
			{
				return;
			}

			AssetPackageCache packageCache = GetCache(packageName);
			using (Collections.AllocAutoList(out List<string> guids))
			{
				packageCache.GetCacheGuids(guids);

				await UniTask.SwitchToThreadPool();
				foreach (string guid in guids)
				{
					RemoveCacheFile(packageName, guid);
				}
				await UniTask.SwitchToMainThread();
			}
		}

		public static void GetUnusedCacheGuids(ResourcePackage package, List<string> result)
		{
			if (package == null || result == null)
			{
				return;
			}

			result.Clear();
			AssetPackageCache packageCache = GetCache(package.PackageName);
			using (Collections.AllocAutoList(out List<string> guids))
			{
				packageCache.GetCacheGuids(guids);
				for (int i = 0; i < guids.Count; i++)
				{
					string guid = guids[i];
					if (!package.IsIncludeBundleFile(guid))
					{
						continue;
					}

					result.Add(guid);
				}
			}
		}

		public static EAssetFileCacheResult VerifyingCacheFile(string packageName, string cacheGuid)
		{
			if (string.IsNullOrEmpty(packageName))
			{
				Log<AssetCacheSystem>.Error("Internal error... caught null or empty AssetPackage name while verify recorded cache file");
				return EAssetFileCacheResult.Exception;
			}

			AssetPackageCache packageCache = GetCache(packageName);
			if (!packageCache.TryGetMeta(cacheGuid, out FileCacheMeta meta))
			{
				Log<AssetCacheSystem>.Error($"Internal error... package {packageName} didn't cache file with guid : {cacheGuid}");
				return EAssetFileCacheResult.CacheNotFound;
			}

			return Verify(meta.DataFilePath, meta.DataFileSize, meta.DataFileCRC, EVerifyLevel.High);
		}

		public static EAssetFileCacheResult VerifyingCacheFile(FAssetCacheFile cacheFile)
		{
			if (CacheFileVerifyLevel == EVerifyLevel.Low)
			{
				return cacheFile.LowLevelVerify();
			}

			if (!File.Exists(cacheFile.InfoFilePath))
			{
				return EAssetFileCacheResult.InfoFileNotExisted;
			}

			try
			{
				cacheFile.ReadFromFileBuffer();
			}
			catch (Exception exception)
			{
				Log<AssetCacheSystem>.Exception(exception);
				return EAssetFileCacheResult.Exception;
			}

			return Verify(cacheFile.DataFilePath, cacheFile.DataFileSize, cacheFile.DataFileCRC, CacheFileVerifyLevel);
		}

		public static EAssetFileCacheResult VerifyTempFile(FAssetCacheFile cacheFile)
		{
			return Verify(cacheFile.DataFilePath, cacheFile.DataFileSize, cacheFile.DataFileCRC, EVerifyLevel.High);
		}

		public static void CacheFile(string packageName, string cacheGuid, FileCacheMeta meta)
		{
			if (string.IsNullOrEmpty(packageName))
			{
				return;
			}

			GetCache(packageName).Add(cacheGuid, meta);
		}

		public static void RemoveCacheFile(string packageName, string cacheGuid)
		{
			if (string.IsNullOrEmpty(packageName))
			{
				return;
			}

			AssetPackageCache packageCache = GetCache(packageName);
			if (!packageCache.TryGetMeta(cacheGuid, out FileCacheMeta meta))
			{
				return;
			}

			packageCache.Remove(cacheGuid);

			try
			{
				meta.Delete();
			}
			catch (Exception exception)
			{
				Log<AssetCacheSystem>.Exception(exception);
			}
		}

		public static bool IsCached(string packageName, string cacheGuid)
		{
			if (string.IsNullOrEmpty(packageName))
			{
				return false;
			}

			return GetCache(packageName).IsCached(cacheGuid);
		}

		public static int GetCachePackageFileCount(string packageName)
		{
			if (string.IsNullOrEmpty(packageName))
			{
				return 0;
			}

			return GetCache(packageName).GetFileCount();
		}

		public static void ClearPackage(string packageName)
		{
			if (string.IsNullOrEmpty(packageName))
			{
				return;
			}

			GetCache(packageName).Clear();
		}

		static AssetPackageCache GetCache(string packageName)
		{
			if (s_CachePackages.TryGetValue(packageName, out AssetPackageCache packageCache))
			{
				return packageCache;
			}

			packageCache = new(packageName);
			s_CachePackages[packageName] = packageCache;
			return packageCache;
		}

		static void CacheBundleFiles(string packageName)
		{
			string bundleRootFolder = FileSystem.GetCachedBundleFileFolderPath(packageName);
			DirectoryInfo bundleDirectoryInfo = new(bundleRootFolder);
			if (!bundleDirectoryInfo.Exists)
			{
				return;
			}

			Log<AssetCacheSystem>.Info($"Start cache bundle files in : {bundleDirectoryInfo.FullName}");
			IEnumerable<DirectoryInfo> directorieInfos = bundleDirectoryInfo.EnumerateDirectories();
			using IEnumerator<DirectoryInfo> bundleDirectoryEnumrator = directorieInfos.GetEnumerator();
			while (bundleDirectoryEnumrator.MoveNext())
			{
				DirectoryInfo bundleDirectory = bundleDirectoryEnumrator.Current;
				DirectoryInfo[] directories = bundleDirectory.GetDirectories();
				foreach (DirectoryInfo directory in directories)
				{
					string cacheGuid = directory.Name;
					if (IsCached(packageName, cacheGuid))
					{
						Log<AssetCacheSystem>.Info($"[AssetPackage-{packageName}] Bundle folder {cacheGuid} already cached");
						continue;
					}

					// 创建验证元素类
					string fileRootPath = directory.FullName;
					string dataFilePath = FileSystem.GetCachedBundleDataFilePath(fileRootPath);
					string infoFilePath = FileSystem.GetCachedBundleInfoFilePath(fileRootPath);
					FAssetCacheFile verifyInfo = new(packageName, cacheGuid, fileRootPath, dataFilePath, infoFilePath);
					EAssetFileCacheResult result = VerifyingCacheFile(verifyInfo);
					if (result == EAssetFileCacheResult.Succeed)
					{
						Log<AssetCacheSystem>.Info($"[AssetPackage-{packageName}] Bundle folder verify success : {fileRootPath}");
					}
					else
					{
						Log<AssetCacheSystem>.Error($"[AssetPackage-{packageName}] Bundle folder verify failed : {fileRootPath}, delete files...");
						verifyInfo.Delete();
					}
				}
			}
		}

		static void CacheRawFiles(string packageName)
		{
			string rawFileFolder = FileSystem.GetCachedRawFileFolderPath(packageName);
			DirectoryInfo rawFileDirectoryInfo = new(rawFileFolder);
			if (rawFileDirectoryInfo.Exists)
			{
				Log<AssetCacheSystem>.Info($"Start cache bundle files in : {rawFileDirectoryInfo.FullName}");
				IEnumerable<DirectoryInfo> directorieInfos = rawFileDirectoryInfo.EnumerateDirectories();
				using IEnumerator<DirectoryInfo> rawFileEnumrator = directorieInfos.GetEnumerator();
				while (rawFileEnumrator.MoveNext())
				{
					DirectoryInfo rawFileDirectory = rawFileEnumrator.Current;
					DirectoryInfo[] directories = rawFileDirectory.GetDirectories();
					foreach (DirectoryInfo directory in directories)
					{
						string cacheGuid = directory.Name;
						if (IsCached(packageName, cacheGuid))
						{
							Log<AssetCacheSystem>.Info($"[AssetPackage-{packageName}] RawFile folder {cacheGuid} already cached");
							continue;
						}

						string dataFileExtension = string.Empty;
						FileInfo[] fileInfos = directory.GetFiles();
						foreach (FileInfo fileInfo in fileInfos)
						{
							if (fileInfo.Extension == ".temp")
							{
								continue;
							}

							if (fileInfo.Name.StartsWith(UniverseConstant.CACHE_BUNDLE_DATA_FILE_NAME))
							{
								dataFileExtension = fileInfo.Extension;
								break;
							}
						}

						string fileRootPath = directory.FullName;
						string dataFilePath = FileSystem.GetCachedBundleDataFilePath(fileRootPath, dataFileExtension);
						string infoFilePath = FileSystem.GetCachedBundleInfoFilePath(fileRootPath);
						FAssetCacheFile verifyInfo = new(packageName, cacheGuid, fileRootPath, dataFilePath, infoFilePath);
						EAssetFileCacheResult result = VerifyingCacheFile(verifyInfo);
						if (result == EAssetFileCacheResult.Succeed)
						{
							Log<AssetCacheSystem>.Info($"[AssetPackage-{packageName}] RawFile folder verify success : {fileRootPath}");
						}
						else
						{
							Log<AssetCacheSystem>.Error($"[AssetPackage-{packageName}] RawFile folder verify failed : {fileRootPath}, delete files...");
							verifyInfo.Delete();
						}
					}
				}
			}
		}

		public static EAssetFileCacheResult Verify(string filePath, long fileSize, string fileCRC, EVerifyLevel verifyLevel)
		{
			try
			{
				if (!File.Exists(filePath))
				{
					return EAssetFileCacheResult.DataFileNotExisted;
				}

				long size = FileSystem.GetFileSize(filePath);
				if (size < fileSize)
				{
					return EAssetFileCacheResult.FileNotComplete;
				}

				if (size > fileSize)
				{
					return EAssetFileCacheResult.FileOverflow;
				}

				if (verifyLevel == EVerifyLevel.High)
				{
					string crc = HashUtilities.FileCRC32(filePath);
					if (crc == fileCRC)
					{
						return EAssetFileCacheResult.Succeed;
					}

					return EAssetFileCacheResult.FileCrcError;
				}

				return EAssetFileCacheResult.Succeed;
			}
			catch (Exception)
			{
				return EAssetFileCacheResult.Exception;
			}
		}
	}
}
