using System.IO;
using UnityEngine;

namespace UniverseEngine
{
	/// <summary>
	/// 初始化操作
	/// </summary>
	public abstract class InitializationOperation : AsyncOperationBase
	{
		public string PackageVersion { protected set; get; }
	}

	/// <summary>
	/// 编辑器下模拟模式的初始化操作
	/// </summary>
	internal sealed class EditorSimulateModeInitializationOperation : InitializationOperation
	{
		readonly EditorSimulateModeImpl m_Impl;
		readonly string m_SimulateManifestFilePath;

		internal EditorSimulateModeInitializationOperation(EditorSimulateModeImpl impl, string simulateManifestFilePath)
		{
			m_Impl = impl;
			m_SimulateManifestFilePath = simulateManifestFilePath;
		}

		internal override void Start()
		{
			DeserializeManifest().Forget();
		}

		async UniTaskVoid DeserializeManifest()
		{
			m_Impl.ActiveManifest = await AssetSystem.DeserializeManifest(m_SimulateManifestFilePath);
			PackageVersion = m_Impl.ActiveManifest.PackageVersion;
			Status = EOperationStatus.Succeed;
		}

		internal override void Update() { }
	}

	/// <summary>
	/// 离线运行模式的初始化操作
	/// </summary>
	internal sealed class OfflinePlayModeInitializationOperation : InitializationOperation
	{
		readonly OfflinePlayModeImpl m_Impl;
		readonly string m_PackageName;
		string m_PackageVersion;

		internal OfflinePlayModeInitializationOperation(OfflinePlayModeImpl impl, string packageName)
		{
			m_Impl = impl;
			m_PackageName = packageName;
		}
		internal override void Start()
		{
			InitializeLocalPackage().Forget();
		}

		internal override void Update() { }

		async UniTaskVoid InitializeLocalPackage()
		{
			m_PackageVersion = await AssetSystem.QueryBuiltInPackageVersion(m_PackageName);
			if (string.IsNullOrEmpty(m_PackageVersion))
			{
				Status = EOperationStatus.Failed;
			}

			m_Impl.ActiveManifest = await AssetSystem.DeserializeManifest(m_PackageName, m_PackageVersion);
			if (m_Impl.ActiveManifest == null)
			{
				Status = EOperationStatus.Failed;
			}

			await AssetCacheSystem.CacheAssetPackageAsync(m_PackageName);
			Status = EOperationStatus.Succeed;
		}
	}

	/// <summary>
	/// 联机运行模式的初始化操作
	/// 注意：优先从沙盒里加载清单，如果沙盒里不存在就尝试把内置清单拷贝到沙盒并加载该清单。
	/// </summary>
	sealed internal class HostPlayModeInitializationOperation : InitializationOperation
	{
		readonly HostPlayModeImpl m_Impl;
		readonly string m_PackageName;
		LoadCacheManifestOperation m_LoadCacheManifestOp;

		internal HostPlayModeInitializationOperation(HostPlayModeImpl impl, string packageName)
		{
			m_Impl = impl;
			m_PackageName = packageName;
		}
		internal override void Start()
		{
			InitializeRemotePackage(m_PackageName).Forget();
		}

		async UniTaskVoid InitializeRemotePackage(string packageName)
		{
			PackageVersion = await AssetSystem.QueryCachePackageVersion(packageName);
			if (string.IsNullOrEmpty(PackageVersion))
			{
				await QueryBuiltInPackageVersion(packageName);
				return;
			}

			await LoadCachePackageManifest(packageName, PackageVersion);
		}

		async UniTask QueryBuiltInPackageVersion(string packageName)
		{
			PackageVersion = await AssetSystem.QueryBuiltInPackageVersion(packageName);
			if (string.IsNullOrEmpty(PackageVersion))
			{
				await AssetCacheSystem.CacheAssetPackageAsync(packageName);
			}
			else
			{
				bool result = await AssetSystem.UnpackBuiltInManifest(packageName, PackageVersion);
				if (!result)
				{
					return;
				}

				await LoadBuiltInManifest(packageName, PackageVersion);
			}
		}

		async UniTask LoadBuiltInManifest(string packageName, string packageVersion)
		{
			m_Impl.ActiveManifest = await AssetSystem.DeserializeManifest(packageName, packageVersion);
			if (m_Impl.ActiveManifest == null)
			{
				return;
			}

			PackageVersion = m_Impl.ActiveManifest.PackageVersion;
			await AssetCacheSystem.CacheAssetPackageAsync(packageName);
		}

		async UniTask LoadCachePackageManifest(string packageName, string packageVersion)
		{
			m_Impl.ActiveManifest = await AssetSystem.LoadCachePackageManifest(packageName, packageVersion);
			if (m_Impl.ActiveManifest == null)
			{
				await QueryBuiltInPackageVersion(packageName);
				return;
			}

			PackageVersion = m_Impl.ActiveManifest.PackageVersion;
		}

		internal override void Update() { }
	}
}
