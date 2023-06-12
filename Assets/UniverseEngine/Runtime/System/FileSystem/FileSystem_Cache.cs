using System;
using System.Collections.Generic;

namespace UniverseEngine
{
	public partial class FileSystem
	{
		/// <summary>
		/// 获取缓存的BundleFile文件夹路径
		/// </summary>
		static readonly Dictionary<string, string> s_CachedBundleFileFolder = new();

		/// <summary>
		/// 获取缓存的RawFile文件夹路径
		/// </summary>
		static readonly Dictionary<string, string> s_CachedRawFileFolder = new();

		/// <summary>
		/// 删除沙盒
		/// </summary>
		public static void DeleteCacheSystem()
		{
			DeleteDirectory(s_CacheSystemRoot.Value);
		}

		/// <summary>
		/// 删除沙盒内的缓存文件夹
		/// </summary>
		public static void DeleteCacheFilesRoot()
		{
			string root = ToPersistentLoadPath(CACHED_FOLDER_NAME);
			DeleteDirectory(root);
		}

		/// <summary>
		/// 删除沙盒内的清单文件夹
		/// </summary>
		public static void DeleteManifestRoot()
		{
			string root = ToPersistentLoadPath(CACHED_MANIFEST_FOLDER_NAME);
			DeleteDirectory(root);
		}

		public static string GetCachedRawFileFolderPath(string packageName)
		{
			if (s_CachedRawFileFolder.TryGetValue(packageName, out string value) == false)
			{
				string root = ToPersistentLoadPath(CACHED_FOLDER_NAME);
				value = StringUtilities.Format(FILE_PATH_FORMAT_3, root, packageName, CACHED_RAW_FILE_FOLDER);
				s_CachedRawFileFolder.Add(packageName, value);
			}
			return value;
		}

		public static string GetCachedBundleFileFolderPath(string packageName)
		{
			if (!s_CachedBundleFileFolder.TryGetValue(packageName, out string value))
			{
				string root = ToPersistentLoadPath(CACHED_FOLDER_NAME);
				value = StringUtilities.Format(FILE_PATH_FORMAT_3, root, packageName, CACHED_BUNDLE_FILE_FOLDER);
				s_CachedBundleFileFolder.Add(packageName, value);
			}

			return value;
		}

		public static string GetCachedBundleDataFilePath(string directory, string extension = EMPTY)
		{
			if (string.IsNullOrEmpty(directory))
			{
				return string.Empty;
			}

			return extension switch
			{
				EMPTY => StringUtilities.Format(FILE_PATH_FORMAT_2, directory, UniverseConstant.CACHE_BUNDLE_DATA_FILE_NAME),
				_ => StringUtilities.Format(FILE_PATH_FORMAT_3, directory, UniverseConstant.CACHE_BUNDLE_DATA_FILE_NAME, extension)
			};
		}

		public static string GetCachedBundleInfoFilePath(string directory)
		{
			if (string.IsNullOrEmpty(directory))
			{
				return string.Empty;
			}

			return StringUtilities.Format(FILE_PATH_FORMAT_2, directory, UniverseConstant.CACHE_BUNDLE_INFO_FILE_NAME);
		}

		/// <summary>
		/// 获取应用程序的水印文件路径
		/// </summary>
		public static string GetAppFootPrintFilePath()
		{
			return ToPersistentLoadPath(APP_FOOT_PRINT_FILE_NAME);
		}

		/// <summary>
		/// 获取沙盒内清单文件的路径
		/// </summary>
		public static string GetCachedManifestFilePath(string packageName, string packageVersion)
		{
			string fileName = GetManifestBinaryFileName(packageName, packageVersion);
			string path = StringUtilities.Format(FILE_PATH_FORMAT_2, CACHED_MANIFEST_FOLDER_NAME, fileName);
			return ToPersistentLoadPath(path);
		}

		/// <summary>
		/// 获取沙盒内包裹的哈希文件的路径
		/// </summary>
		public static string GetCachedPackageHashFilePath(string packageName, string packageVersion)
		{
			string fileName = GetPackageHashFileName(packageName, packageVersion);
			string path = StringUtilities.Format(FILE_PATH_FORMAT_2, CACHED_MANIFEST_FOLDER_NAME, fileName);
			return ToPersistentLoadPath(path);
		}

		/// <summary>
		/// 获取沙盒内包裹的版本文件的路径
		/// </summary>
		public static string GetCachedPackageVersionFilePath(string packageName)
		{
			string fileName = GetPackageVersionFileName(packageName);
			string path = StringUtilities.Format(FILE_PATH_FORMAT_2, CACHED_MANIFEST_FOLDER_NAME, fileName);
			return ToPersistentLoadPath(path);
		}

		/// <summary>
		/// 保存默认的包裹版本
		/// </summary>
		public static void SaveCachedPackageVersionFile(string packageName, string version)
		{
			string filePath = GetCachedPackageVersionFilePath(packageName);
			CreateFile(filePath, version);
		}

		public static string GetRemoteBundleFileName(EAssetOutputNameStyle nameStyle, string bundleName, string fileExtension, string fileHash)
		{
			switch (nameStyle)
			{
				//HashName
				case EAssetOutputNameStyle.HashName:
				{
					return StringUtilities.Format("{0}{1}", fileHash, fileExtension);
				}
				//BundleName_HashName
				case EAssetOutputNameStyle.BundleName_HashName:
				{
					string fileName = bundleName.Remove(bundleName.LastIndexOf('.'));
					return StringUtilities.Format("{0}_{1}{2}", fileName, fileHash, fileExtension);
				}
				default:
				{
					throw new NotImplementedException($"Invalid name style : {nameStyle}");
				}
			}
		}

		/// <summary>
		/// 检测AssetBundle文件是否合法
		/// </summary>
		public static bool CheckBundleFileValid(byte[] fileData)
		{
			string signature = StringUtilities.ReadString(fileData, 20);
			return signature is "UnityFS" or "UnityRaw" or "UnityWeb" or "\xFA\xFA\xFA\xFA\xFA\xFA\xFA\xFA";
		}
	}
}
