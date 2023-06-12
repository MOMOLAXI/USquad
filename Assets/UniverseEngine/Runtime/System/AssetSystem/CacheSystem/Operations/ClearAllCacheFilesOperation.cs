using System.Collections.Generic;

namespace UniverseEngine
{
	/// <summary>
	/// 清理本地包裹所有的缓存文件
	/// </summary>
	public sealed class ClearAllCacheFilesOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			GetAllCacheFiles,
			ClearAllCacheFiles,
			Done,
		}

		private readonly ResourcePackage m_Package;
		// private List<string> m_AllCacheGUIDs;
		// private int m_FileTotalCount = 0;
		// private ESteps m_Steps = ESteps.None;

		internal ClearAllCacheFilesOperation(ResourcePackage package)
		{
			m_Package = package;
		}
		internal override void Start()
		{
			// m_Steps = ESteps.GetAllCacheFiles;
			ClearAllCacheFiles().Forget();
		}

		async UniTaskVoid ClearAllCacheFiles()
		{
			await AssetCacheSystem.ClearAllCacheFiles(m_Package.PackageName);
			Status = EOperationStatus.Succeed;
		}

		internal override void Update()
		{
			// if (m_Steps == ESteps.None || m_Steps == ESteps.Done)
			// 	return;
			//
			// if (m_Steps == ESteps.GetAllCacheFiles)
			// {
			// 	m_AllCacheGUIDs = CacheSystem.GetAllCacheGUIDs(m_Package);
			// 	m_FileTotalCount = m_AllCacheGUIDs.Count;
			// 	Log<AssetSystem>.Info($"Found all cache file count : {m_FileTotalCount}");
			// 	m_Steps = ESteps.ClearAllCacheFiles;
			// }
			//
			// if (m_Steps == ESteps.ClearAllCacheFiles)
			// {
			// 	for (int i = m_AllCacheGUIDs.Count - 1; i >= 0; i--)
			// 	{
			// 		string cacheGuid = m_AllCacheGUIDs[i];
			// 		CacheSystem.DiscardFile(m_Package.PackageName, cacheGuid);
			// 		m_AllCacheGUIDs.RemoveAt(i);
			//
			// 		if (OperationSystem.IsBusy)
			// 			break;
			// 	}
			//
			// 	if (m_FileTotalCount == 0)
			// 		Progress = 1.0f;
			// 	else
			// 		Progress = 1.0f - (m_AllCacheGUIDs.Count / m_FileTotalCount);
			//
			// 	if (m_AllCacheGUIDs.Count == 0)
			// 	{
			// 		m_Steps = ESteps.Done;
			// 		Status = EOperationStatus.Succeed;
			// 	}
			// }
		}
	}
}
