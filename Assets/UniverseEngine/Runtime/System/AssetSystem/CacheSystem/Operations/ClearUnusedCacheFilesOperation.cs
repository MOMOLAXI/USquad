using System.Collections.Generic;

namespace UniverseEngine
{
	/// <summary>
	/// 清理本地包裹未使用的缓存文件
	/// </summary>
	public sealed class ClearUnusedCacheFilesOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			GetUnusedCacheFiles,
			ClearUnusedCacheFiles,
			Done,
		}

		private readonly ResourcePackage m_Package;
		// private List<string> m_UnusedCacheGUIDs;
		// private int m_UnusedFileTotalCount = 0;
		// private ESteps m_Steps = ESteps.None;

		internal ClearUnusedCacheFilesOperation(ResourcePackage package)
		{
			m_Package = package;
		}
		internal override void Start()
		{
			// m_Steps = ESteps.GetUnusedCacheFiles;
			ClearUnusedCacheFiles().Forget();
		}

		async UniTaskVoid ClearUnusedCacheFiles()
		{
			await AssetCacheSystem.ClearUnusedCacheFiles(m_Package);
			Status = EOperationStatus.Succeed;
		}

		internal override void Update()
		{
			// if (m_Steps == ESteps.None || m_Steps == ESteps.Done)
			// 	return;
			//
			// if (m_Steps == ESteps.GetUnusedCacheFiles)
			// {
			// 	m_UnusedCacheGUIDs = CacheSystem.GetUnusedCacheGUIDs(m_Package);
			// 	m_UnusedFileTotalCount = m_UnusedCacheGUIDs.Count;
			// 	Log<AssetSystem>.Info($"Found unused cache file count : {m_UnusedFileTotalCount}");
			// 	m_Steps = ESteps.ClearUnusedCacheFiles;
			// }
			//
			// if (m_Steps == ESteps.ClearUnusedCacheFiles)
			// {
			// 	for (int i = m_UnusedCacheGUIDs.Count - 1; i >= 0; i--)
			// 	{
			// 		string cacheGuid = m_UnusedCacheGUIDs[i];
			// 		CacheSystem.DiscardFile(m_Package.PackageName, cacheGuid);
			// 		m_UnusedCacheGUIDs.RemoveAt(i);
			//
			// 		if (OperationSystem.IsBusy)
			// 			break;
			// 	}
			//
			// 	if (m_UnusedFileTotalCount == 0)
			// 		Progress = 1.0f;
			// 	else
			// 		Progress = 1.0f - (m_UnusedCacheGUIDs.Count / m_UnusedFileTotalCount);
			//
			// 	if (m_UnusedCacheGUIDs.Count == 0)
			// 	{
			// 		m_Steps = ESteps.Done;
			// 		Status = EOperationStatus.Succeed;
			// 	}
			// }
		}
	}
}
