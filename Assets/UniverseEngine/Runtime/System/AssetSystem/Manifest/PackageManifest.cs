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
	[Serializable]
	internal partial class PackageManifest
	{
		/// <summary>
		/// 文件版本
		/// </summary>
		public string FileVersion;

		/// <summary>
		/// 启用可寻址资源定位
		/// </summary>
		public bool EnableAddressable;

		/// <summary>
		/// 文件名称样式
		/// </summary>
		public EAssetOutputNameStyle OutputNameStyle;

		/// <summary>
		/// 资源包裹名称
		/// </summary>
		public string PackageName;

		/// <summary>
		/// 资源包裹的版本信息
		/// </summary>
		public string PackageVersion;

		public PackageManifestInfo Info;

		/// <summary>
		/// 资源路径映射集合
		/// </summary>
		[NonSerialized]
		public Dictionary<string, string> AssetPathMapping = new();

		/// <summary>
		/// 初始化资源路径映射
		/// </summary>
		public void InitAssetPathMapping()
		{
			AssetPathMapping.Clear();
			if (EnableAddressable)
			{
				foreach (Asset packageAsset in m_AssetList)
				{
					string location = packageAsset.Address;
					if (AssetPathMapping.ContainsKey(location))
					{
						throw new($"Address have existed : {location}");
					}

					AssetPathMapping[location] = packageAsset.AssetPath;
				}
			}
			else
			{
				foreach (Asset packageAsset in m_AssetList)
				{
					string location = packageAsset.AssetPath;

					// 添加原生路径的映射
					if (AssetPathMapping.ContainsKey(location))
					{
						throw new($"AssetPath have existed : {location}");
					}

					AssetPathMapping[location] = packageAsset.AssetPath;

					// 添加无后缀名路径的映射
					if (Path.HasExtension(location))
					{
						string locationWithoutExtension = StringUtilities.RemoveExtension(location);
						if (AssetPathMapping.ContainsKey(locationWithoutExtension))
						{
							Log<AssetSystem>.Warning($"AssetPath have existed : {locationWithoutExtension}");
						}
						else
						{
							AssetPathMapping[locationWithoutExtension] = packageAsset.AssetPath;
						}
					}
				}
			}
		}

		/// <summary>
		/// 映射为资源路径
		/// </summary>
		public string MappingToAssetPath(string location)
		{
			if (string.IsNullOrEmpty(location))
			{
				Log<AssetSystem>.Error("Failed to mapping location to asset path, The location is null or empty.");
				return string.Empty;
			}

			if (AssetPathMapping.TryGetValue(location, out string assetPath))
			{
				return assetPath;
			}

			Log<AssetSystem>.Warning($"Failed to mapping location to asset path : {location}");
			return string.Empty;
		}

		/// <summary>
		/// 尝试映射为资源路径
		/// </summary>
		public string TryMappingToAssetPath(string location)
		{
			if (string.IsNullOrEmpty(location))
			{
				return string.Empty;
			}

			if (AssetPathMapping.TryGetValue(location, out string assetPath))
			{
				return assetPath;
			}

			return string.Empty;
		}

		/// <summary>
		/// 获取主资源包
		/// 注意：传入的资源路径一定合法有效！
		/// </summary>
		public PackageBundle GetMainPackageBundle(string assetPath)
		{
			if (m_AssetQuery.TryGetValue(assetPath, out Asset packageAsset))
			{
				int bundleID = packageAsset.BundleID;
				if (Collections.IsValidIndex(m_BundleList, bundleID))
				{
					PackageBundle packageBundle = m_BundleList[bundleID];
					return packageBundle;
				}

				throw new($"Invalid bundle id : {bundleID} Asset path : {assetPath}");
			}

			throw new("Should never get here !");
		}

		/// <summary>
		/// 获取资源依赖列表
		/// 注意：传入的资源路径一定合法有效！
		/// </summary>
		public void GetAllDependencies(string assetPath, List<PackageBundle> result)
		{
			if (string.IsNullOrEmpty(assetPath) || result == null)
			{
				return;
			}

			result.Clear();
			if (m_AssetQuery.TryGetValue(assetPath, out Asset packageAsset))
			{
				foreach (int dependID in packageAsset.DependIDs)
				{
					if (dependID >= 0 && dependID < m_BundleList.Count)
					{
						PackageBundle dependBundle = m_BundleList[dependID];
						result.Add(dependBundle);
					}
					else
					{
						throw new($"Invalid bundle id : {dependID} Asset path : {assetPath}");
					}
				}
			}
			else
			{
				throw new("Should never get here !");
			}
		}

		/// <summary>
		/// 获取资源包名称
		/// </summary>
		public string GetBundleName(int bundleID)
		{
			if (Collections.IsValidIndex(m_BundleList, bundleID))
			{
				PackageBundle packageBundle = m_BundleList[bundleID];
				return packageBundle.BundleName;
			}

			throw new($"Invalid bundle id : {bundleID}");
		}

		/// <summary>
		/// 尝试获取包裹的资源
		/// </summary>
		public bool TryGetPackageAsset(string assetPath, out Asset result)
		{
			return m_AssetQuery.TryGetValue(assetPath, out result);
		}

		/// <summary>
		/// 获取资源信息列表
		/// </summary>
		public void GetAssetsInfoByTags(string[] tags, List<AssetInfo> result)
		{
			if (Collections.IsNullOrEmpty(tags) || Collections.IsNullOrEmpty(tags))
			{
				return;
			}

			result.Clear();
			for (int i = 0; i < m_AssetList.Count; i++)
			{
				Asset packageAsset = m_AssetList[i];
				if (packageAsset.HasTag(tags))
				{
					AssetInfo assetInfo = new(packageAsset);
					result.Add(assetInfo);
				}
			}
		}

		/// <summary>
		/// 资源定位地址转换为资源信息类，失败时内部会发出错误日志。
		/// </summary>
		/// <returns>如果转换失败会返回一个无效的资源信息类</returns>
		public AssetInfo ConvertLocationToAssetInfo(string location, Type assetType)
		{
			DebugCheckLocation(location);

			string assetPath = MappingToAssetPath(location);
			if (TryGetPackageAsset(assetPath, out Asset packageAsset))
			{
				AssetInfo assetInfo = new(packageAsset, assetType);
				return assetInfo;
			}
			else
			{
				string error = string.IsNullOrEmpty(location) ? "The location is null or empty !" : $"The location is invalid : {location}";
				AssetInfo assetInfo = new(error);
				return assetInfo;
			}
		}

		public static void SerializeToJson(PackageManifest manifest, string savePath, bool prettyPrint = true)
		{
			string json = JsonUtility.ToJson(manifest, prettyPrint);
			FileSystem.CreateFile(savePath, json);
		}

		/// <summary>
		/// 反序列化（JSON文件）
		/// </summary>
		public static PackageManifest DeserializeFromJson(string jsonContent)
		{
			return JsonUtility.FromJson<PackageManifest>(jsonContent);
		}

		public static void SerializeToBinary(PackageManifest manifest, string savePath)
		{
			manifest.SerializeToBinary(savePath);
		}

		public (bool, string) ReadFromBuffer(BufferReader buffer)
		{
			string error = string.Empty;
			// 读取文件标记
			uint fileSign = buffer.ReadUInt32();
			if (fileSign != UniverseConstant.PATCH_MANIFEST_FILE_SIGN)
			{
				error = "The manifest file format is invalid !";
				return (false, error);
			}

			// 读取文件版本
			string fileVersion = buffer.ReadUTF8();
			if (fileVersion != UniverseConstant.PATCH_MANIFEST_FILE_VERSION)
			{
				error = $"The manifest file version are not compatible : {fileVersion} != {UniverseConstant.PATCH_MANIFEST_FILE_VERSION}";
				return (false, error);
			}

			EnableAddressable = buffer.ReadBool();
			OutputNameStyle = (EAssetOutputNameStyle)buffer.ReadInt32();
			PackageName = buffer.ReadUTF8();
			PackageVersion = buffer.ReadUTF8();

			ReadAssetsFromBuffer(buffer);
			ReadBundlesFromBuffer(buffer);

			return (true, string.Empty);
		}

		public async UniTask<(bool, string)> ReadFromBufferAsync(BufferReader buffer)
		{
			(bool, string) result = default;

			await UniTask.SwitchToThreadPool();
			result = ReadFromBuffer(buffer);
			await UniTask.SwitchToMainThread();

			return result;
		}

		public void WriteToBuffer(BufferWriter buffer)
		{
			//写入文件标记
			buffer.WriteUInt32(UniverseConstant.PATCH_MANIFEST_FILE_SIGN);

			//写入文件版本
			buffer.WriteUTF8(FileVersion);

			//写入文件头信息
			buffer.WriteBool(EnableAddressable);
			buffer.WriteInt32((int)OutputNameStyle);
			buffer.WriteUTF8(PackageName);
			buffer.WriteUTF8(PackageVersion);

			//写入资源列表
			WriteAssetsToBuffer(buffer);

			// 写入资源包列表
			WriteBundlesToBuffer(buffer);
		}

		/// <summary>
		/// 序列化（二进制文件）
		/// </summary>
		public void SerializeToBinary(string savePath)
		{
			using (FileStream fs = new(savePath, FileMode.Create))
			{
				// 创建缓存器
				BufferWriter buffer = new(UniverseConstant.MANIFEST_FILE_MAX_SIZE);
				WriteToBuffer(buffer);

				// 写入文件流
				buffer.WriteToStream(fs);
				fs.Flush();
			}
		}

		/// <summary>
		/// 反序列化（二进制文件）
		/// </summary>
		public static PackageManifest DeserializeFromBinary(byte[] binaryData)
		{
			// 创建缓存器
			BufferReader buffer = new(binaryData);
			PackageManifest manifest = new();
			(bool result, string error) = manifest.ReadFromBuffer(buffer);
			if (!result)
			{
				throw new AssetException(error);
			}

			return manifest;
		}

	#region 调试方法

		[Conditional("DEBUG")]
		private void DebugCheckLocation(string location)
		{
			if (string.IsNullOrEmpty(location) == false)
			{
				// 检查路径末尾是否有空格
				int index = location.LastIndexOf(" ", StringComparison.Ordinal);
				if (index != -1)
				{
					if (location.Length == index + 1)
						Log<AssetSystem>.Warning($"Found blank character in location : \"{location}\"");
				}

				if (location.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
					Log<AssetSystem>.Warning($"Found illegal character in location : \"{location}\"");
			}
		}

	#endregion
	}
}
