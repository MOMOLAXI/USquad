using System;
using System.Collections.Generic;
using UnityEditor;

namespace UniverseEngine.Editor
{
	public class BuildAssetInfo
	{
		bool m_IsAddAssetTags;
		readonly HashSet<string> m_ReferenceBundleNames = new();

		/// <summary>
		/// 收集器类型
		/// </summary>
		public ECollectorType CollectorType { get; }

		/// <summary>
		/// 资源包完整名称
		/// </summary>
		public string BundleName { private set; get; }

		/// <summary>
		/// 可寻址地址
		/// </summary>
		public string Address { private set; get; }

		/// <summary>
		/// 资源路径
		/// </summary>
		public string AssetPath { get; }

		/// <summary>
		/// 是否为原生资源
		/// </summary>
		public bool IsRawAsset { get; }

		/// <summary>
		/// 是否为着色器资源
		/// </summary>
		public bool IsShaderAsset { get; }

		/// <summary>
		/// 资源的分类标签
		/// </summary>
		public readonly List<string> AssetTags = new();

		/// <summary>
		/// 资源包的分类标签
		/// </summary>
		public readonly List<string> BundleTags = new();

		/// <summary>
		/// 依赖的所有资源
		/// 注意：包括零依赖资源和冗余资源（资源包名无效）
		/// </summary>
		public List<BuildAssetInfo> AllDependAssetInfos = new();

		public BuildAssetInfo(CollectAssetInfo collectAssetInfo)
		{
			CollectorType = collectAssetInfo.CollectorType;
			BundleName = collectAssetInfo.BundleName;
			Address = collectAssetInfo.Address;
			AssetPath = collectAssetInfo.AssetPath;
			IsRawAsset = collectAssetInfo.IsRawAsset;
			Type assetType = AssetDatabase.GetMainAssetTypeAtPath(collectAssetInfo.AssetPath);
			IsShaderAsset = assetType == typeof(UnityEngine.Shader) || assetType == typeof(UnityEngine.ShaderVariantCollection);
		}

		public BuildAssetInfo(ECollectorType collectorType, string bundleName, string address, string assetPath, bool isRawAsset)
		{
			CollectorType = collectorType;
			BundleName = bundleName;
			Address = address;
			AssetPath = assetPath;
			IsRawAsset = isRawAsset;
			Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
			IsShaderAsset = assetType == typeof(UnityEngine.Shader) || assetType == typeof(UnityEngine.ShaderVariantCollection);
		}

		public BuildAssetInfo(string assetPath)
		{
			CollectorType = ECollectorType.None;
			Address = string.Empty;
			AssetPath = assetPath;
			IsRawAsset = false;

			Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
			IsShaderAsset = assetType == typeof(UnityEngine.Shader) || assetType == typeof(UnityEngine.ShaderVariantCollection);
		}

		/// <summary>
		/// 设置所有依赖的资源
		/// </summary>
		public void SetAllDependAssetInfos(List<BuildAssetInfo> dependAssetInfos)
		{
			AllDependAssetInfos.Clear();
			AllDependAssetInfos.AddRange(dependAssetInfos);
		}

		/// <summary>
		/// 添加资源的分类标签
		/// 说明：原始定义的资源分类标签
		/// </summary>
		public void AddAssetTags(List<string> tags)
		{
			if (m_IsAddAssetTags)
			{
				throw new("Should never get here !");
			}

			m_IsAddAssetTags = true;
			foreach (string tag in tags)
			{
				if (!AssetTags.Contains(tag))
				{
					AssetTags.Add(tag);
				}
			}
		}

		/// <summary>
		/// 添加资源包的分类标签
		/// 说明：传染算法统计到的分类标签
		/// </summary>
		public void AddBundleTags(List<string> tags)
		{
			foreach (string tag in tags)
			{
				if (!BundleTags.Contains(tag))
				{
					BundleTags.Add(tag);
				}
			}
		}

		/// <summary>
		/// 资源包名是否存在
		/// </summary>
		public bool HasBundleName() => !string.IsNullOrEmpty(BundleName);

		/// <summary>
		/// 添加关联的资源包名称
		/// </summary>
		public void AddReferenceBundleName(string bundleName)
		{
			if (string.IsNullOrEmpty(bundleName))
				throw new("Should never get here !");

			m_ReferenceBundleNames.Add(bundleName);
		}

		/// <summary>
		/// 计算共享资源包的完整包名
		/// </summary>
		public void CalculateShareBundleName(IShareAssetPackRule packRule, bool uniqueBundleName, string packageName, string shadersBundleName)
		{
			if (CollectorType != ECollectorType.None)
				return;

			if (IsRawAsset)
				throw new("Should never get here !");

			if (IsShaderAsset)
			{
				BundleName = shadersBundleName;
			}
			else
			{
				if (m_ReferenceBundleNames.Count > 1)
				{
					PackRuleResult packRuleResult = packRule.GetPackRuleResult(AssetPath);
					BundleName = packRuleResult.GetShareBundleName(packageName, uniqueBundleName);
				}
				else
				{
					// 注意：被引用次数小于1的资源不需要设置资源包名称
					BundleName = string.Empty;
				}
			}
		}
	}
}
