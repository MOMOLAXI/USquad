using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;

namespace UniverseEngine
{
	/// <summary>
	/// 清单文件
	/// </summary>
	internal partial class PackageManifest
	{
		public List<BundleInfo> GetDownloadList()
		{
			List<PackageBundle> downloadList = new();
			foreach (PackageBundle packageBundle in m_BundleList)
			{
				// 忽略缓存文件
				if (IsCachedPackageBundle(packageBundle))
					continue;

				// 忽略APP资源
				if (QueryStreamingAssets(packageBundle.FileName))
					continue;

				downloadList.Add(packageBundle);
			}

			return ConvertToDownloadList(downloadList);
		}

		public List<BundleInfo> GetDownloadListByTags(string[] tags)
		{
			List<PackageBundle> downloadList = new();
			foreach (PackageBundle packageBundle in m_BundleList)
			{
				// 忽略缓存文件
				if (IsCachedPackageBundle(packageBundle))
					continue;

				// 忽略APP资源
				if (QueryStreamingAssets(packageBundle.FileName))
					continue;

				// 如果未带任何标记，则统一下载
				if (packageBundle.HasAnyTags() == false)
				{
					downloadList.Add(packageBundle);
				}
				else
				{
					// 查询DLC资源
					if (packageBundle.HasTag(tags))
					{
						downloadList.Add(packageBundle);
					}
				}
			}

			return ConvertToDownloadList(downloadList);
		}

		public List<BundleInfo> GetDownloadListByPaths(IReadOnlyCollection<AssetInfo> assetInfos)
		{
			// 获取资源对象的资源包和所有依赖资源包
			List<PackageBundle> checkList = new();
			foreach (AssetInfo assetInfo in assetInfos)
			{
				if (assetInfo.IsInvalid)
				{
					Log<AssetSystem>.Warning(assetInfo.Error);
					continue;
				}

				// 注意：如果清单里未找到资源包会抛出异常！
				PackageBundle mainBundle = GetMainPackageBundle(assetInfo.AssetPath);
				if (checkList.Contains(mainBundle) == false)
					checkList.Add(mainBundle);

				// 注意：如果清单里未找到资源包会抛出异常！
				List<PackageBundle> dependBundles = new();
				GetAllDependencies(assetInfo.AssetPath, dependBundles);
				foreach (PackageBundle dependBundle in dependBundles)
				{
					if (!checkList.Contains(dependBundle))
					{
						checkList.Add(dependBundle);
					}
				}
			}

			List<PackageBundle> downloadList = new(1000);
			foreach (PackageBundle packageBundle in checkList)
			{
				// 忽略缓存文件
				if (IsCachedPackageBundle(packageBundle))
					continue;

				// 忽略APP资源
				if (QueryStreamingAssets(packageBundle.FileName))
					continue;

				downloadList.Add(packageBundle);
			}

			return ConvertToDownloadList(downloadList);
		}

		public List<BundleInfo> GetUnpackListByAll()
		{
			List<PackageBundle> downloadList = new(1000);
			foreach (PackageBundle packageBundle in m_BundleList)
			{
				// 忽略缓存文件
				if (IsCachedPackageBundle(packageBundle))
					continue;

				if (QueryStreamingAssets(packageBundle.FileName))
				{
					downloadList.Add(packageBundle);
				}
			}

			return ConvertToUnpackList(downloadList);
		}

		public List<BundleInfo> GetUnpackListByTags(string[] tags)
		{
			List<PackageBundle> downloadList = new(1000);
			foreach (PackageBundle packageBundle in m_BundleList)
			{
				// 忽略缓存文件
				if (IsCachedPackageBundle(packageBundle))
					continue;

				// 查询DLC资源
				if (QueryStreamingAssets(packageBundle.FileName))
				{
					if (packageBundle.HasTag(tags))
					{
						downloadList.Add(packageBundle);
					}
				}
			}

			return ConvertToUnpackList(downloadList);
		}

		List<BundleInfo> ConvertToUnpackList(List<PackageBundle> unpackList)
		{
			List<BundleInfo> result = new(unpackList.Count);
			foreach (PackageBundle packageBundle in unpackList)
			{
				BundleInfo bundleInfo = packageBundle.GetUnpackInfo();
				result.Add(bundleInfo);
			}
			return result;
		}

		BundleInfo ConvertToDownloadInfo(PackageBundle packageBundle)
		{
			string remoteMainURL = GetRemoteMainURL(packageBundle.FileName);
			string remoteFallbackURL = GetRemoteFallbackURL(packageBundle.FileName);
			BundleInfo bundleInfo = new(packageBundle, BundleInfo.ELoadMode.LoadFromRemote, remoteMainURL, remoteFallbackURL);
			return bundleInfo;
		}

		List<BundleInfo> ConvertToDownloadList(List<PackageBundle> downloadList)
		{
			List<BundleInfo> result = new(downloadList.Count);
			foreach (PackageBundle packageBundle in downloadList)
			{
				BundleInfo bundleInfo = ConvertToDownloadInfo(packageBundle);
				result.Add(bundleInfo);
			}
			return result;
		}

		public string GetRemoteMainURL(string fileName)
		{
			return $"{Info.DefaultHostServer}/{fileName}";
		}

		public string GetRemoteFallbackURL(string fileName)
		{
			return $"{Info.FallbackHostServer}/{fileName}";
		}

		public static bool IsCachedPackageBundle(PackageBundle packageBundle)
		{
			return AssetCacheSystem.IsCached(packageBundle.PackageName, packageBundle.CacheGuid);
		}

		public static bool QueryStreamingAssets(string fileName)
		{
			return FileSystem.FileExistsWithAndroid($"{FileSystem.STREAMING_ASSETS_BUILTIN_FOLDER}/{fileName}");
		}
	}
}
