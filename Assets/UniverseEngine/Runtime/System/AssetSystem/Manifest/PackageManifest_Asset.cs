using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;

namespace UniverseEngine
{
	/// <summary>
	/// 清单文件
	/// </summary>
	internal partial class PackageManifest
	{
		/// <summary>
		/// 资源列表（主动收集的资源列表）
		/// </summary>
		[SerializeField]
		List<Asset> m_AssetList = new();

		/// <summary>
		/// 资源映射集合（提供AssetPath获取PackageAsset）
		/// </summary>
		[NonSerialized]
		Dictionary<string, Asset> m_AssetQuery = new();

		public int MainAssetCount => m_AssetList.Count;

		public void SetAssets(List<Asset> assets)
		{
			m_AssetList.Clear();
			m_AssetList.AddRange(assets);
		}

		public void WriteAssetsToBuffer(BufferWriter buffer)
		{
			buffer.WriteInt32(m_AssetList.Count);
			for (int i = 0; i < m_AssetList.Count; i++)
			{
				Asset asset = m_AssetList[i];
				asset.WriteToBuffer(buffer);
			}
		}

		public void ReadAssetsFromBuffer(BufferReader buffer)
		{
			m_AssetList.Clear();
			m_AssetQuery.Clear();

			int count = buffer.ReadInt32();
			while (count > 0)
			{
				Asset asset = new();
				asset.ReadFromBuffer(buffer);
				m_AssetList.Add(asset);

				string assetPath = asset.AssetPath;
				if (m_AssetQuery.ContainsKey(assetPath))
				{
					Log<AssetSystem>.Error($"Found duplicated asset path at package {Info.PackageName} : {assetPath}");
				}
				else
				{
					m_AssetQuery[assetPath] = asset;
				}

				count--;
			}
		}

		public void ForeachAssets(Action<Asset> function)
		{
			foreach (Asset asset in m_AssetList)
			{
				function?.Invoke(asset);
			}
		}
	}
}
