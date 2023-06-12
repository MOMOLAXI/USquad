using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniverseEngine
{
	/// <summary>
	/// 清单文件
	/// </summary>
	internal partial class PackageManifest
	{
		
		public void UpdateBuiltInShaderDependency(List<string> shaderBundleReferenceList, string shadersBunldeName)
		{
			if (Collections.IsNullOrEmpty(shaderBundleReferenceList))
			{
				return;
			}

			//获取着色器资源包索引
			bool Predicate(PackageBundle s) => s.BundleName == shadersBunldeName;
			int shaderBundleId = FindAssetBundleId(Predicate);
			if (shaderBundleId == -1)
			{
				throw new AssetException("没有发现着色器资源包！");
			}

			//检测依赖交集并更新依赖ID
			HashSet<string> tags = new();
			foreach (Asset packageAsset in m_AssetList)
			{
				List<string> dependBundles = GetAssetDependBundles(packageAsset);
				List<string> conflictAssetPathList = dependBundles.Intersect(shaderBundleReferenceList).ToList();
				if (conflictAssetPathList.Count > 0)
				{
					List<int> newDependIDs = new(packageAsset.DependIDs);
					if (!newDependIDs.Contains(shaderBundleId))
					{
						newDependIDs.Add(shaderBundleId);
					}

					packageAsset.DependIDs = newDependIDs;
					foreach (string tag in packageAsset.AssetTags)
					{
						tags.Add(tag);
					}
				}
			}

			//更新资源包标签
			PackageBundle packageBundle = GetBundleById(shaderBundleId);
			packageBundle.UpdateTags(tags);
		}
	}
}
