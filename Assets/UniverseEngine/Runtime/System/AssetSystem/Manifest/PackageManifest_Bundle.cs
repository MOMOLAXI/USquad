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
		/// <summary>
		/// 资源包列表
		/// </summary>
		[SerializeField]
		List<PackageBundle> m_BundleList = new();

		/// <summary>
		/// 资源包集合（提供BundleName获取PackageBundle）
		/// </summary>
		Dictionary<string, PackageBundle> m_BundleQuery = new();

		Dictionary<string, PackageBundle> m_BundleGuids = new();

		public int BundleCount => m_BundleList.Count;

		public long BundleTotalSize => m_BundleList.Sum(packageBundle => packageBundle.FileSize);

		public int EncryptedBundleCount => m_BundleList.Count(packageBundle => packageBundle.LoadMethod != (byte)EBundleLoadMethod.Normal);

		public long EncryptedBundleSize => m_BundleList.Where(packageBundle => packageBundle.LoadMethod != (byte)EBundleLoadMethod.Normal)
		                                               .Sum(packageBundle => packageBundle.FileSize);

		public int RawBundleCount => m_BundleList.Count(packageBundle => packageBundle.IsRawFile);

		public long RawBundleSize => m_BundleList.Where(packageBundle => packageBundle.IsRawFile).Sum(packageBundle => packageBundle.FileSize);

		public void ReadBundlesFromBuffer(BufferReader buffer)
		{
			m_BundleList.Clear();
			m_BundleQuery.Clear();
			m_BundleGuids.Clear();
			int count = buffer.ReadInt32();
			while (count > 0)
			{
				PackageBundle bundle = new();
				bundle.ReadFromBuffer(buffer);
				bundle.ParseBundle(PackageName, OutputNameStyle);
				m_BundleList.Add(bundle);
				m_BundleQuery[bundle.BundleName] = bundle;
				m_BundleGuids[bundle.CacheGuid] = bundle;
				count--;
			}
		}

		public void WriteBundlesToBuffer(BufferWriter buffer)
		{
			buffer.WriteInt32(m_BundleList.Count);
			for (int i = 0; i < m_BundleList.Count; i++)
			{
				PackageBundle bundle = m_BundleList[i];
				bundle.WriteToBuffer(buffer);
			}
		}

		/// <summary>
		/// 尝试获取包裹的资源包
		/// </summary>
		public bool TryGetPackageBundle(string bundleName, out PackageBundle result)
		{
			return m_BundleQuery.TryGetValue(bundleName, out result);
		}

		/// <summary>
		/// 是否包含资源文件
		/// </summary>
		public bool IsIncludeBundleFile(string cacheGuid)
		{
			return m_BundleGuids.ContainsKey(cacheGuid);
		}

		public void SetBundles(List<PackageBundle> bundles)
		{
			m_BundleList.Clear();
			m_BundleList.AddRange(bundles);
		}

		public void ForeachBundles(Action<PackageBundle> function)
		{
			foreach (PackageBundle packageBundle in m_BundleList)
			{
				function?.Invoke(packageBundle);
			}
		}

		public PackageBundle GetBundleById(int bundleId)
		{
			if (!Collections.IsValidIndex(m_BundleList, bundleId))
			{
				return null;
			}

			return m_BundleList[bundleId];
		}

		public int GetAssetBundleId(string bundleName)
		{
			if (string.IsNullOrEmpty(bundleName))
			{
				return -1;
			}

			for (int i = 0; i < m_BundleList.Count; i++)
			{
				PackageBundle bundle = m_BundleList[i];
				if (bundle.BundleName == bundleName)
				{
					return i;
				}
			}

			throw new AssetException($"Not found bundle name : {bundleName}");
		}

		public int FindAssetBundleId(Predicate<PackageBundle> predicate)
		{
			if (predicate == null)
			{
				return -1;
			}

			return m_BundleList.FindIndex(predicate);
		}

		public List<string> GetAssetDependBundles(Asset asset)
		{
			List<string> result = new();

			PackageBundle mainBundle = GetBundleById(asset.BundleID);
			string mainBundleName = mainBundle.BundleName;
			result.Add(mainBundleName);

			for (int i = 0; i < asset.DependIDs.Count; i++)
			{
				int dependID = asset.DependIDs[i];
				PackageBundle bundle = GetBundleById(dependID);
				string dependBundle = bundle.BundleName;
				result.Add(dependBundle);
			}

			return result;
		}

		public void CopyBundles(string fromDir, string toDir, Predicate<PackageBundle> predicate = null)
		{
			if (predicate == null)
			{
				foreach (PackageBundle packageBundle in m_BundleList)
				{
					CopyBundle(packageBundle, fromDir, toDir);
				}
			}
			else
			{
				foreach (PackageBundle packageBundle in m_BundleList)
				{
					if (predicate.Invoke(packageBundle))
					{
						CopyBundle(packageBundle, fromDir, toDir);
					}
				}
			}
		}

		public void CopyBundle(PackageBundle bundle, string fromDir, string toDir)
		{
			string sourcePath = FileSystem.ToPath(fromDir, bundle.FileName);
			string destPath = FileSystem.ToPath(toDir, bundle.FileName);
			FileSystem.CopyFile(sourcePath, destPath, true);
		}
	}
}
