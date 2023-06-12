using System.Linq;
using System.Collections.Generic;
using UnityEditor;

namespace UniverseEngine.Editor
{
	public class BuildBundleInfo
	{
		/// <summary>
		/// 构建内容的哈希值
		/// </summary>
		public string ContentHash { set; get; }

		/// <summary>
		/// 文件哈希值
		/// </summary>
		public string FileHash { set; get; }

		/// <summary>
		/// 文件哈希值
		/// </summary>
		public string FileCRC { set; get; }

		/// <summary>
		/// 文件哈希值
		/// </summary>
		public long FileSize { set; get; }

		/// <summary>
		/// 构建输出的文件路径
		/// </summary>
		public string BuildOutputFilePath { set; get; }

		/// <summary>
		/// 补丁包输出文件路径
		/// </summary>
		public string PackageOutputFilePath { set; get; }

		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName { get; }

		/// <summary>
		/// 参与构建的资源列表
		/// 注意：不包含零依赖资源
		/// </summary>
		public readonly List<BuildAssetInfo> AllMainAssets = new();

		/// <summary>
		/// Bundle文件的加载方法
		/// </summary>
		public EBundleLoadMethod LoadMethod { set; get; }

		/// <summary>
		/// 加密生成文件的路径
		/// 注意：如果未加密该路径为空
		/// </summary>
		public string EncryptedFilePath { set; get; }

		/// <summary>
		/// 是否为原生文件
		/// </summary>
		public bool IsRawFile
		{
			get
			{
				foreach (BuildAssetInfo assetInfo in AllMainAssets)
				{
					if (assetInfo.IsRawAsset)
						return true;
				}
				return false;
			}
		}

		/// <summary>
		/// 是否为加密文件
		/// </summary>
		public bool IsEncryptedFile
		{
			get
			{
				if (string.IsNullOrEmpty(EncryptedFilePath))
					return false;
				else
					return true;
			}
		}


		public BuildBundleInfo(string bundleName)
		{
			BundleName = bundleName;
		}

		/// <summary>
		/// 添加一个打包资源
		/// </summary>
		public void PackAsset(BuildAssetInfo assetInfo)
		{
			if (IsContainsAsset(assetInfo.AssetPath))
				throw new($"Asset is existed : {assetInfo.AssetPath}");

			AllMainAssets.Add(assetInfo);
		}

		/// <summary>
		/// 是否包含指定资源
		/// </summary>
		public bool IsContainsAsset(string assetPath)
		{
			return AllMainAssets.Any(assetInfo => assetInfo.AssetPath == assetPath);
		}

		/// <summary>
		/// 获取资源包的分类标签列表
		/// </summary>
		public void GetBundleTags(List<string> result)
		{
			if (result == null)
			{
				return;
			}

			foreach (BuildAssetInfo assetInfo in AllMainAssets)
			{
				foreach (string assetTag in assetInfo.BundleTags)
				{
					if (!result.Contains(assetTag))
					{
						result.Add(assetTag);
					}
				}
			}
		}

		/// <summary>
		/// 获取该资源包内的所有资源（包括零依赖资源）
		/// </summary>
		public void GetAllBuiltinAssetPaths(List<string> result)
		{
			if (result == null)
			{
				return;
			}

			result.Clear();
			GetAllMainAssetPaths(result);
			foreach (BuildAssetInfo assetInfo in AllMainAssets)
			{
				if (Collections.IsNullOrEmpty(assetInfo.AllDependAssetInfos))
				{
					continue;
				}

				foreach (BuildAssetInfo dependAssetInfo in assetInfo.AllDependAssetInfos)
				{
					if (dependAssetInfo.HasBundleName())
					{
						continue;
					}

					if (!result.Contains(dependAssetInfo.AssetPath))
					{
						result.Add(dependAssetInfo.AssetPath);
					}
				}
			}
		}

		/// <summary>
		/// 获取构建的资源路径列表
		/// </summary>
		public void GetAllMainAssetPaths(List<string> result)
		{
			if (result == null)
			{
				return;
			}

			result.Clear();
			result.AddRange(AllMainAssets.Select(info => info.AssetPath));
		}

		internal IEnumerable<Asset> GetBuildAssets(PackageManifest manifest, bool enableAddressable)
		{
			List<Asset> assets = new();
			IEnumerable<BuildAssetInfo> assetInfos = AllMainAssets.Where(t => t.CollectorType == ECollectorType.MainAssetCollector);
			foreach (BuildAssetInfo assetInfo in assetInfos)
			{
				Asset asset = new()
				{
					Address = enableAddressable ? assetInfo.Address : string.Empty,
					AssetPath = assetInfo.AssetPath,
					AssetTags = assetInfo.AssetTags,
					BundleID = manifest.GetAssetBundleId(assetInfo.BundleName)
				};
				asset.DependIDs = GetAssetBundleDependIDs(asset.BundleID, assetInfo, manifest);
				assets.Add(asset);
			}

			return assets;
		}

		/// <summary>
		/// 获取所有写入补丁清单的资源
		/// </summary>
		public void GetAllMainAssetInfos(List<BuildAssetInfo> result)
		{
			if (result == null)
			{
				return;
			}

			result.Clear();
			result.AddRange(AllMainAssets.Where(t => t.CollectorType == ECollectorType.MainAssetCollector));
		}

		/// <summary>
		/// 创建AssetBundleBuild类
		/// </summary>
		public AssetBundleBuild CreatePipelineBuild()
		{
			// 注意：我们不在支持AssetBundle的变种机制
			AssetBundleBuild build = new()
			{
				assetBundleName = BundleName,
				assetBundleVariant = string.Empty,
			};

			List<string> assetPaths = new();
			GetAllMainAssetPaths(assetPaths);
			build.assetNames = assetPaths.ToArray();
			return build;
		}

		/// <summary>
		/// 创建PackageBundle类
		/// </summary>
		internal PackageBundle CreatePackageBundle()
		{
			PackageBundle packageBundle = new()
			{
				BundleName = BundleName,
				FileHash = FileHash,
				FileCRC = FileCRC,
				FileSize = FileSize,
				IsRawFile = IsRawFile,
				LoadMethod = (byte)LoadMethod,
			};

			GetBundleTags(packageBundle.Tags);
			return packageBundle;
		}

		static List<int> GetAssetBundleDependIDs(int mainBundleID, BuildAssetInfo assetInfo, PackageManifest manifest)
		{
			List<int> result = new();
			foreach (BuildAssetInfo dependAssetInfo in assetInfo.AllDependAssetInfos)
			{
				if (dependAssetInfo.HasBundleName())
				{
					int bundleID = manifest.GetAssetBundleId(dependAssetInfo.BundleName);
					if (mainBundleID != bundleID)
					{
						if (!result.Contains(bundleID))
						{
							result.Add(bundleID);
						}
					}
				}
			}

			return result;
		}
	}
}
