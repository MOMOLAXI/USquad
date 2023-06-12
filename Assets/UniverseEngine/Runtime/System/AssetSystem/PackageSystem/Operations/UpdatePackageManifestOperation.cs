namespace UniverseEngine
{
	/// <summary>
	/// 向远端请求并更新清单
	/// </summary>
	public abstract class UpdatePackageManifestOperation : AsyncOperationBase
	{
		/// <summary>
		/// 保存当前清单的版本，用于下次启动时自动加载的版本。
		/// </summary>
		public virtual void SavePackageVersion() { }
	}

	/// <summary>
	/// 编辑器下模拟运行的更新清单操作
	/// </summary>
	internal sealed class EditorPlayModeUpdatePackageManifestOperation : UpdatePackageManifestOperation
	{
		public EditorPlayModeUpdatePackageManifestOperation()
		{
		}
		internal override void Start()
		{
			Status = EOperationStatus.Succeed;
		}
		internal override void Update()
		{
		}
	}

	/// <summary>
	/// 离线模式的更新清单操作
	/// </summary>
	internal sealed class OfflinePlayModeUpdatePackageManifestOperation : UpdatePackageManifestOperation
	{
		public OfflinePlayModeUpdatePackageManifestOperation()
		{
		}
		internal override void Start()
		{
			Status = EOperationStatus.Succeed;
		}
		internal override void Update()
		{
		}
	}

	/// <summary>
	/// 联机模式的更新清单操作
	/// 注意：优先加载沙盒里缓存的清单文件，如果缓存没找到就下载远端清单文件，并保存到本地。
	/// </summary>
	internal sealed class HostPlayModeUpdatePackageManifestOperation : UpdatePackageManifestOperation
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
		private readonly bool m_AutoSaveVersion;
		private readonly int m_Timeout;
		private LoadCacheManifestOperation m_TryLoadCacheManifestOp;
		private LoadCacheManifestOperation m_LoadCacheManifestOp;
		private DownloadManifestOperation m_DownloadManifestOp;
		private ESteps m_Steps = ESteps.None;


		internal HostPlayModeUpdatePackageManifestOperation(HostPlayModeImpl impl, string packageName, string packageVersion, bool autoSaveVersion, int timeout)
		{
			m_Impl = impl;
			m_PackageName = packageName;
			m_PackageVersion = packageVersion;
			m_AutoSaveVersion = autoSaveVersion;
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
				if (m_Impl.ActiveManifest != null && m_Impl.ActiveManifest.PackageVersion == m_PackageVersion)
				{
					m_Steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
				else
				{
					m_Steps = ESteps.TryLoadCacheManifest;
				}
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
					m_Impl.ActiveManifest = m_TryLoadCacheManifestOp.Manifest;
					if (m_AutoSaveVersion)
						SavePackageVersion();
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
					m_Impl.ActiveManifest = m_LoadCacheManifestOp.Manifest;
					if (m_AutoSaveVersion)
						SavePackageVersion();
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

		public override void SavePackageVersion() 
		{
			m_Impl.FlushManifestVersionFile();
		}
	}
}