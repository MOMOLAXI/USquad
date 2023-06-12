using System.Collections.Generic;

namespace UniverseEngine
{
	internal class HostPlayModeImpl : IPlayModeServices, IBundleServices, IRemoteServices
	{
		private PackageManifest m_ActiveManifest;

		// 参数相关
		private string m_PackageName;
		private bool m_LocationToLower;
		private string m_DefaultHostServer;
		private string m_FallbackHostServer;
		private IQueryServices m_QueryServices;

		/// <summary>
		/// 异步初始化
		/// </summary>
		public InitializationOperation InitializeAsync(string packageName, bool locationToLower, string defaultHostServer, string fallbackHostServer, IQueryServices queryServices)
		{
			m_PackageName = packageName;
			m_LocationToLower = locationToLower;
			m_DefaultHostServer = defaultHostServer;
			m_FallbackHostServer = fallbackHostServer;
			m_QueryServices = queryServices;

			HostPlayModeInitializationOperation operation = new(this, packageName);
			OperationSystem.StartOperation(operation);
			return operation;
		}

		private BundleInfo ConvertToDownloadInfo(PackageBundle packageBundle)
		{
			string remoteMainURL = GetRemoteMainURL(packageBundle.FileName);
			string remoteFallbackURL = GetRemoteFallbackURL(packageBundle.FileName);
			BundleInfo bundleInfo = new(packageBundle, BundleInfo.ELoadMode.LoadFromRemote, remoteMainURL, remoteFallbackURL);
			return bundleInfo;
		}

		// 解压相关
		private List<BundleInfo> ConvertToUnpackList(List<PackageBundle> unpackList)
		{
			List<BundleInfo> result = new(unpackList.Count);
			foreach (PackageBundle packageBundle in unpackList)
			{
				BundleInfo bundleInfo = packageBundle.GetUnpackInfo();
				result.Add(bundleInfo);
			}
			return result;
		}

	#region IRemoteServices接口

		public string GetRemoteMainURL(string fileName)
		{
			return $"{m_DefaultHostServer}/{fileName}";
		}
		public string GetRemoteFallbackURL(string fileName)
		{
			return $"{m_FallbackHostServer}/{fileName}";
		}

	#endregion

	#region IPlayModeServices接口

		public PackageManifest ActiveManifest
		{
			set
			{
				m_ActiveManifest = value;
				m_ActiveManifest.InitAssetPathMapping();
			}
			get => m_ActiveManifest;
		}
		public void FlushManifestVersionFile()
		{
			if (m_ActiveManifest != null)
				FileSystem.SaveCachedPackageVersionFile(m_PackageName, m_ActiveManifest.PackageVersion);
		}

		UpdatePackageVersionOperation IPlayModeServices.UpdatePackageVersionAsync(bool appendTimeTicks, int timeout)
		{
			HostPlayModeUpdatePackageVersionOperation operation = new(this, m_PackageName, appendTimeTicks, timeout);
			OperationSystem.StartOperation(operation);
			return operation;
		}
		UpdatePackageManifestOperation IPlayModeServices.UpdatePackageManifestAsync(string packageVersion, bool autoSaveVersion, int timeout)
		{
			HostPlayModeUpdatePackageManifestOperation operation = new(this, m_PackageName, packageVersion, autoSaveVersion, timeout);
			OperationSystem.StartOperation(operation);
			return operation;
		}
		PreDownloadContentOperation IPlayModeServices.PreDownloadContentAsync(string packageVersion, int timeout)
		{
			HostPlayModePreDownloadContentOperation operation = new(this, m_PackageName, packageVersion, timeout);
			OperationSystem.StartOperation(operation);
			return operation;
		}

		ResourceDownloaderOperation IPlayModeServices.CreateResourceDownloaderByAll(int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			List<BundleInfo> downloadList = m_ActiveManifest.GetDownloadList();
			;
			ResourceDownloaderOperation operation = new(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
			return operation;
		}
		ResourceDownloaderOperation IPlayModeServices.CreateResourceDownloaderByTags(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			List<BundleInfo> downloadList = m_ActiveManifest.GetDownloadListByTags(tags);
			ResourceDownloaderOperation operation = new(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
			return operation;
		}

		ResourceDownloaderOperation IPlayModeServices.CreateResourceDownloaderByPaths(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			List<BundleInfo> downloadList = m_ActiveManifest.GetDownloadListByPaths(assetInfos);
			ResourceDownloaderOperation operation = new(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
			return operation;
		}

		ResourceUnpackerOperation IPlayModeServices.CreateResourceUnpackerByAll(int upackingMaxNumber, int failedTryAgain, int timeout)
		{
			List<BundleInfo> unpcakList = m_ActiveManifest.GetUnpackListByAll();
			ResourceUnpackerOperation operation = new(unpcakList, upackingMaxNumber, failedTryAgain, timeout);
			return operation;
		}

		ResourceUnpackerOperation IPlayModeServices.CreateResourceUnpackerByTags(string[] tags, int upackingMaxNumber, int failedTryAgain, int timeout)
		{
			List<BundleInfo> unpcakList = m_ActiveManifest.GetUnpackListByTags(tags);
			ResourceUnpackerOperation operation = new(unpcakList, upackingMaxNumber, failedTryAgain, timeout);
			return operation;
		}

	#endregion

	#region IBundleServices接口

		private BundleInfo CreateBundleInfo(PackageBundle packageBundle)
		{
			if (packageBundle == null)
				throw new("Should never get here !");

			// 查询沙盒资源
			if (PackageManifest.IsCachedPackageBundle(packageBundle))
			{
				BundleInfo bundleInfo = new(packageBundle, BundleInfo.ELoadMode.LoadFromCache);
				return bundleInfo;
			}

			// 查询APP资源
			if (PackageManifest.QueryStreamingAssets(packageBundle.FileName))
			{
				BundleInfo bundleInfo = new(packageBundle, BundleInfo.ELoadMode.LoadFromStreaming);
				return bundleInfo;
			}

			// 从服务端下载
			return ConvertToDownloadInfo(packageBundle);
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
