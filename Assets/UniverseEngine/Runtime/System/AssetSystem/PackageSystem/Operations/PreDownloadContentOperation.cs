using System.Collections.Generic;

namespace UniverseEngine
{
	public abstract class PreDownloadContentOperation : AsyncOperationBase
	{
		/// <summary>
		/// 创建资源下载器，用于下载当前资源版本所有的资源包文件
		/// </summary>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public virtual ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			return ResourceDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
		}

		/// <summary>
		/// 创建资源下载器，用于下载指定的资源标签关联的资源包文件
		/// </summary>
		/// <param name="tag">资源标签</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public virtual ResourceDownloaderOperation CreateResourceDownloader(string tag, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			return ResourceDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
		}

		/// <summary>
		/// 创建资源下载器，用于下载指定的资源标签列表关联的资源包文件
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public virtual ResourceDownloaderOperation CreateResourceDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			return ResourceDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
		}

		/// <summary>
		/// 创建资源下载器，用于下载指定的资源依赖的资源包文件
		/// </summary>
		/// <param name="location">资源定位地址</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public virtual ResourceDownloaderOperation CreateBundleDownloader(string location, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			return ResourceDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
		}

		/// <summary>
		/// 创建资源下载器，用于下载指定的资源列表依赖的资源包文件
		/// </summary>
		/// <param name="locations">资源定位地址列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public virtual ResourceDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			return ResourceDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
		}
	}

	internal class EditorPlayModePreDownloadContentOperation : PreDownloadContentOperation
	{
		internal override void Start()
		{
			Status = EOperationStatus.Succeed;
		}
		internal override void Update()
		{
		}
	}

	internal class OfflinePlayModePreDownloadContentOperation : PreDownloadContentOperation
	{
		internal override void Start()
		{
			Status = EOperationStatus.Succeed;
		}
		internal override void Update()
		{
		}
	}

	internal class HostPlayModePreDownloadContentOperation : PreDownloadContentOperation
	{
		private enum ESteps
		{
			None,
			CheckActiveManifest,
			TryLoadCacheManifest,
			DownloadManifest,
			LoadCacheManifest,
			CheckDeserializeManifest,
			Done,
		}

		private readonly HostPlayModeImpl m_Impl;
		private readonly string m_PackageName;
		private readonly string m_PackageVersion;
		private readonly int m_Timeout;
		private LoadCacheManifestOperation m_TryLoadCacheManifestOp;
		private LoadCacheManifestOperation m_LoadCacheManifestOp;
		private DownloadManifestOperation m_DownloadManifestOp;
		private PackageManifest m_Manifest;
		private ESteps m_Steps = ESteps.None;


		internal HostPlayModePreDownloadContentOperation(HostPlayModeImpl impl, string packageName, string packageVersion, int timeout)
		{
			m_Impl = impl;
			m_PackageName = packageName;
			m_PackageVersion = packageVersion;
			m_Timeout = timeout;
		}
		internal override void Start()
		{
			m_Steps = ESteps.CheckActiveManifest;
		}
		internal override void Update()
		{
			if (m_Steps == ESteps.None || m_Steps == ESteps.Done)
				return;

			if (m_Steps == ESteps.CheckActiveManifest)
			{
				// 检测当前激活的清单对象
				if (m_Impl.ActiveManifest != null)
				{
					if (m_Impl.ActiveManifest.PackageVersion == m_PackageVersion)
					{
						m_Manifest = m_Impl.ActiveManifest;
						m_Steps = ESteps.Done;
						Status = EOperationStatus.Succeed;
						return;
					}
				}
				m_Steps = ESteps.TryLoadCacheManifest;
			}

			if (m_Steps == ESteps.TryLoadCacheManifest)
			{
				if (m_TryLoadCacheManifestOp == null)
				{
					m_TryLoadCacheManifestOp = new(m_PackageName, m_PackageVersion);
					OperationSystem.StartOperation(m_TryLoadCacheManifestOp);
				}

				if (m_TryLoadCacheManifestOp.IsDone == false)
					return;

				if (m_TryLoadCacheManifestOp.Status == EOperationStatus.Succeed)
				{
					m_Manifest = m_TryLoadCacheManifestOp.Manifest;
					m_Steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
				else
				{
					m_Steps = ESteps.DownloadManifest;
				}
			}

			if (m_Steps == ESteps.DownloadManifest)
			{
				if (m_DownloadManifestOp == null)
				{
					m_DownloadManifestOp = new(m_Impl, m_PackageName, m_PackageVersion, m_Timeout);
					OperationSystem.StartOperation(m_DownloadManifestOp);
				}

				if (m_DownloadManifestOp.IsDone == false)
					return;

				if (m_DownloadManifestOp.Status == EOperationStatus.Succeed)
				{
					m_Steps = ESteps.LoadCacheManifest;
				}
				else
				{
					m_Steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = m_DownloadManifestOp.Error;
				}
			}

			if (m_Steps == ESteps.LoadCacheManifest)
			{
				if (m_LoadCacheManifestOp == null)
				{
					m_LoadCacheManifestOp = new(m_PackageName, m_PackageVersion);
					OperationSystem.StartOperation(m_LoadCacheManifestOp);
				}

				if (m_LoadCacheManifestOp.IsDone == false)
					return;

				if (m_LoadCacheManifestOp.Status == EOperationStatus.Succeed)
				{
					m_Manifest = m_LoadCacheManifestOp.Manifest;
					m_Steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
				else
				{
					m_Steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = m_LoadCacheManifestOp.Error;
				}
			}
		}

		public override ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			if (Status != EOperationStatus.Succeed)
			{
				Log<AssetSystem>.Warning($"{nameof(PreDownloadContentOperation)} status is not succeed !");
				return ResourceDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
			}

			List<BundleInfo> downloadList = m_Manifest.GetDownloadList();
			ResourceDownloaderOperation operation = new ResourceDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
			return operation;
		}
		public override ResourceDownloaderOperation CreateResourceDownloader(string tag, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			if (Status != EOperationStatus.Succeed)
			{
				Log<AssetSystem>.Warning($"{nameof(PreDownloadContentOperation)} status is not succeed !");
				return ResourceDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
			}

			List<BundleInfo> downloadList = m_Manifest.GetDownloadListByTags(new[] { tag });
			ResourceDownloaderOperation operation = new ResourceDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
			return operation;
		}
		public override ResourceDownloaderOperation CreateResourceDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			if (Status != EOperationStatus.Succeed)
			{
				Log<AssetSystem>.Warning($"{nameof(PreDownloadContentOperation)} status is not succeed !");
				return ResourceDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
			}

			List<BundleInfo> downloadList = m_Manifest.GetDownloadListByTags(tags);
			ResourceDownloaderOperation operation = new ResourceDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
			return operation;
		}

		public override ResourceDownloaderOperation CreateBundleDownloader(string location, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			if (Status != EOperationStatus.Succeed)
			{
				Log<AssetSystem>.Warning($"{nameof(PreDownloadContentOperation)} status is not succeed !");
				return ResourceDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
			}

			List<AssetInfo> assetInfos = new();
			AssetInfo assetInfo = m_Manifest.ConvertLocationToAssetInfo(location, null);
			assetInfos.Add(assetInfo);

			List<BundleInfo> downloadList = m_Manifest.GetDownloadListByPaths(assetInfos);
			ResourceDownloaderOperation operation = new ResourceDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
			return operation;
		}
		public override ResourceDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			if (Status != EOperationStatus.Succeed)
			{
				Log<AssetSystem>.Warning($"{nameof(PreDownloadContentOperation)} status is not succeed !");
				return ResourceDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
			}

			List<AssetInfo> assetInfos = new(locations.Length);
			foreach (string location in locations)
			{
				AssetInfo assetInfo = m_Manifest.ConvertLocationToAssetInfo(location, null);
				assetInfos.Add(assetInfo);
			}

			List<BundleInfo> downloadList = m_Manifest.GetDownloadListByPaths(assetInfos);
			ResourceDownloaderOperation operation = new ResourceDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
			return operation;
		}
	}
}
