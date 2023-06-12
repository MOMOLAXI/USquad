using System.Collections.Generic;

namespace UniverseEngine.Editor
{
	public class BuildMapContext : IContextObject
	{
		readonly Dictionary<string, BuildBundleInfo> m_BundleInfoQuery = new(10000);

		/// <summary>
		/// 参与构建的资源总数
		/// 说明：包括主动收集的资源以及其依赖的所有资源
		/// </summary>
		public int AssetFileCount;

		/// <summary>
		/// 是否启用可寻址资源定位
		/// </summary>
		public bool EnableAddressable;

		/// <summary>
		/// 资源包名唯一化
		/// </summary>
		public bool UniqueBundleName;

		/// <summary>
		/// 着色器统一的全名称
		/// </summary>
		public string ShadersBundleName;

		/// <summary>
		/// 资源包信息列表
		/// </summary>
		public Dictionary<string, BuildBundleInfo>.ValueCollection BundleCollection => m_BundleInfoQuery.Values;

		/// <summary>
		/// 添加一个打包资源
		/// </summary>
		public void PackAsset(BuildAssetInfo assetInfo)
		{
			string bundleName = assetInfo.BundleName;
			if (string.IsNullOrEmpty(bundleName))
				throw new("Should never get here !");

			if (m_BundleInfoQuery.TryGetValue(bundleName, out BuildBundleInfo bundleInfo))
			{
				bundleInfo.PackAsset(assetInfo);
			}
			else
			{
				BuildBundleInfo newBundleInfo = new(bundleName);
				newBundleInfo.PackAsset(assetInfo);
				m_BundleInfoQuery.Add(bundleName, newBundleInfo);
			}
		}

		/// <summary>
		/// 是否包含资源包
		/// </summary>
		public bool IsContainsBundle(string bundleName)
		{
			return m_BundleInfoQuery.ContainsKey(bundleName);
		}

		/// <summary>
		/// 获取资源包信息，如果没找到返回NULL
		/// </summary>
		public BuildBundleInfo GetBundleInfo(string bundleName)
		{
			if (m_BundleInfoQuery.TryGetValue(bundleName, out BuildBundleInfo result))
			{
				return result;
			}

			throw new($"Not found bundle : {bundleName}");
		}

		internal List<Asset> GetBuildAssets(PackageManifest manifest)
		{
			List<Asset> result = new();
			foreach (BuildBundleInfo bundleInfo in BundleCollection)
			{
				IEnumerable<Asset> assets = bundleInfo.GetBuildAssets(manifest, EnableAddressable);
				result.AddRange(assets);
			}

			return result;
		}

		/// <summary>
		/// 获取构建管线里需要的数据
		/// </summary>
		public UnityEditor.AssetBundleBuild[] GetPipelineBuilds()
		{
			List<UnityEditor.AssetBundleBuild> builds = new(m_BundleInfoQuery.Count);
			foreach (BuildBundleInfo bundleInfo in m_BundleInfoQuery.Values)
			{
				if (bundleInfo.IsRawFile)
				{
					continue;
				}

				builds.Add(bundleInfo.CreatePipelineBuild());
			}

			return builds.ToArray();
		}

		/// <summary>
		/// 创建着色器信息类
		/// </summary>
		public void CreateShadersBundleInfo(string shadersBundleName)
		{
			if (!IsContainsBundle(shadersBundleName))
			{
				BuildBundleInfo shaderBundleInfo = new(shadersBundleName);
				m_BundleInfoQuery.Add(shadersBundleName, shaderBundleInfo);
			}
		}
	}
}
