using System.IO;

namespace UniverseEngine
{
	internal class LoadCacheManifestOperation : AsyncOperationBase
	{
		readonly string m_PackageName;
		readonly string m_PackageVersion;

		/// <summary>
		/// 加载的清单实例
		/// </summary>
		public PackageManifest Manifest { set; get; }

		public LoadCacheManifestOperation(string packageName, string packageVersion)
		{
			m_PackageName = packageName;
			m_PackageVersion = packageVersion;
		}

		internal override void Start()
		{
			AssetSystem.LoadCachePackageManifest(m_PackageName, m_PackageVersion).Forget();
		}

		internal override void Update() { }
	}
}
