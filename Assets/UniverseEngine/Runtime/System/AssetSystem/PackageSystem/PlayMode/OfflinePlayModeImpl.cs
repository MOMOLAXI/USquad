using System.Collections.Generic;

namespace UniverseEngine
{
	internal class OfflinePlayModeImpl : IPlayModeServices, IBundleServices
	{
		private PackageManifest m_ActiveManifest;
		private bool m_LocationToLower;

		/// <summary>
		/// 异步初始化
		/// </summary>
		public InitializationOperation InitializeAsync(string packageName, bool locationToLower)
		{
			m_LocationToLower = locationToLower;
			OfflinePlayModeInitializationOperation operation = new(this, packageName);
			OperationSystem.StartOperation(operation);
			return operation;
		}

	#region IPlayModeServices接口

		public PackageManifest ActiveManifest
		{
			set
			{
				m_ActiveManifest = value;
				m_ActiveManifest.InitAssetPathMapping();
			}
			get { return m_ActiveManifest; }
		}
		public void FlushManifestVersionFile()
		{
		}

		UpdatePackageVersionOperation IPlayModeServices.UpdatePackageVersionAsync(bool appendTimeTicks, int timeout)
		{
			OfflinePlayModeUpdatePackageVersionOperation operation = new();
			OperationSystem.StartOperation(operation);
			return operation;
		}
		UpdatePackageManifestOperation IPlayModeServices.UpdatePackageManifestAsync(string packageVersion, bool autoSaveVersion, int timeout)
		{
			OfflinePlayModeUpdatePackageManifestOperation operation = new();
			OperationSystem.StartOperation(operation);
			return operation;
		}
		PreDownloadContentOperation IPlayModeServices.PreDownloadContentAsync(string packageVersion, int timeout)
		{
			OfflinePlayModePreDownloadContentOperation operation = new();
			OperationSystem.StartOperation(operation);
			return operation;
		}

		ResourceDownloaderOperation IPlayModeServices.CreateResourceDownloaderByAll(int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			return ResourceDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
		}
		ResourceDownloaderOperation IPlayModeServices.CreateResourceDownloaderByTags(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			return ResourceDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
		}
		ResourceDownloaderOperation IPlayModeServices.CreateResourceDownloaderByPaths(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			return ResourceDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
		}

		ResourceUnpackerOperation IPlayModeServices.CreateResourceUnpackerByAll(int upackingMaxNumber, int failedTryAgain, int timeout)
		{
			return ResourceUnpackerOperation.CreateEmptyUnpacker(upackingMaxNumber, failedTryAgain, timeout);
		}
		ResourceUnpackerOperation IPlayModeServices.CreateResourceUnpackerByTags(string[] tags, int upackingMaxNumber, int failedTryAgain, int timeout)
		{
			return ResourceUnpackerOperation.CreateEmptyUnpacker(upackingMaxNumber, failedTryAgain, timeout);
		}

	#endregion

	#region IBundleServices接口

		private BundleInfo CreateBundleInfo(PackageBundle packageBundle)
		{
			if (packageBundle == null)
				throw new("Should never get here !");

			// 查询沙盒资源
			if (AssetCacheSystem.IsCached(packageBundle.PackageName, packageBundle.CacheGuid))
			{
				BundleInfo bundleInfo = new(packageBundle, BundleInfo.ELoadMode.LoadFromCache);
				return bundleInfo;
			}

			// 查询APP资源
			{
				BundleInfo bundleInfo = new(packageBundle, BundleInfo.ELoadMode.LoadFromStreaming);
				return bundleInfo;
			}
		}
		BundleInfo IBundleServices.GetBundleInfo(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
				throw new("Should never get here !");

			// 注意：如果清单里未找到资源包会抛出异常！
			PackageBundle packageBundle = m_ActiveManifest.GetMainPackageBundle(assetInfo.AssetPath);
			return CreateBundleInfo(packageBundle);
		}
		BundleInfo[] IBundleServices.GetAllDependBundleInfos(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
				throw new("Should never get here !");

			// 注意：如果清单里未找到资源包会抛出异常！
			List<PackageBundle> depends = new();
			m_ActiveManifest.GetAllDependencies(assetInfo.AssetPath, depends);
			List<BundleInfo> result = new(depends.Count);
			foreach (PackageBundle packageBundle in depends)
			{
				BundleInfo bundleInfo = CreateBundleInfo(packageBundle);
				result.Add(bundleInfo);
			}
			return result.ToArray();
		}
		string IBundleServices.GetBundleName(int bundleID)
		{
			return m_ActiveManifest.GetBundleName(bundleID);
		}
		bool IBundleServices.IsServicesValid()
		{
			return m_ActiveManifest != null;
		}

	#endregion
	}
}
