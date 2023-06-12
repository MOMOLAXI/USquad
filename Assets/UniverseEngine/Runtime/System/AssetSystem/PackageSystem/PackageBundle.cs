using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UniverseEngine
{
	[Serializable]
	internal class PackageBundle
	{
		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName;

		/// <summary>
		/// 文件哈希值
		/// </summary>
		public string FileHash;

		/// <summary>
		/// 文件校验码
		/// </summary>
		public string FileCRC;

		/// <summary>
		/// 文件大小（字节数）
		/// </summary>
		public long FileSize;

		/// <summary>
		/// 是否为原生文件
		/// </summary>
		public bool IsRawFile;

		/// <summary>
		/// 加载方法
		/// </summary>
		public byte LoadMethod;

		/// <summary>
		/// 资源包的分类标签
		/// </summary>
		public List<string> Tags = new();

		/// <summary>
		/// 引用该资源包的ID列表
		/// </summary>
		public List<int> ReferenceIDs = new();

		/// <summary>
		/// 所属的包裹名称
		/// </summary>
		public string PackageName { private set; get; }

		/// <summary>
		/// 缓存GUID
		/// </summary>
		public string CacheGuid => FileHash;

		/// <summary>
		/// 缓存的数据文件路径
		/// </summary>
		private string m_CachedDataFilePath;
		public string CachedDataFilePath
		{
			get
			{
				if (string.IsNullOrEmpty(m_CachedDataFilePath) == false)
					return m_CachedDataFilePath;

				string folderName = FileHash.Substring(0, 2);
				if (IsRawFile)
				{
					string cacheRoot = FileSystem.GetCachedRawFileFolderPath(PackageName);
					m_CachedDataFilePath = $"{cacheRoot}/{folderName}/{CacheGuid}/{UniverseConstant.CACHE_BUNDLE_DATA_FILE_NAME}{m_FileExtension}";
				}
				else
				{
					string cacheRoot = FileSystem.GetCachedBundleFileFolderPath(PackageName);
					m_CachedDataFilePath = $"{cacheRoot}/{folderName}/{CacheGuid}/{UniverseConstant.CACHE_BUNDLE_DATA_FILE_NAME}";
				}
				return m_CachedDataFilePath;
			}
		}

		/// <summary>
		/// 缓存的信息文件路径
		/// </summary>
		private string m_CachedInfoFilePath;
		public string CachedInfoFilePath
		{
			get
			{
				if (string.IsNullOrEmpty(m_CachedInfoFilePath) == false)
					return m_CachedInfoFilePath;

				string folderName = FileHash.Substring(0, 2);
				if (IsRawFile)
				{
					string cacheRoot = FileSystem.GetCachedRawFileFolderPath(PackageName);
					m_CachedInfoFilePath = $"{cacheRoot}/{folderName}/{CacheGuid}/{UniverseConstant.CACHE_BUNDLE_INFO_FILE_NAME}";
				}
				else
				{
					string cacheRoot = FileSystem.GetCachedBundleFileFolderPath(PackageName);
					m_CachedInfoFilePath = $"{cacheRoot}/{folderName}/{CacheGuid}/{UniverseConstant.CACHE_BUNDLE_INFO_FILE_NAME}";
				}
				return m_CachedInfoFilePath;
			}
		}

		/// <summary>
		/// 临时的数据文件路径
		/// </summary>
		private string m_TempDataFilePath;
		public string TempDataFilePath
		{
			get
			{
				if (string.IsNullOrEmpty(m_TempDataFilePath) == false)
					return m_TempDataFilePath;

				m_TempDataFilePath = $"{CachedDataFilePath}.temp";
				return m_TempDataFilePath;
			}
		}

		/// <summary>
		/// 内置文件路径
		/// </summary>
		private string m_StreamingFilePath;
		public string StreamingFilePath
		{
			get
			{
				if (string.IsNullOrEmpty(m_StreamingFilePath) == false)
					return m_StreamingFilePath;

				m_StreamingFilePath = FileSystem.ToStreamingLoadPath(FileName);
				return m_StreamingFilePath;
			}
		}

		/// <summary>
		/// 文件名称
		/// </summary>
		private string m_FileName;
		public string FileName
		{
			get
			{
				if (string.IsNullOrEmpty(m_FileName))
					throw new("Should never get here !");
				return m_FileName;
			}
		}

		/// <summary>
		/// 文件后缀名
		/// </summary>
		private string m_FileExtension;
		public string FileExtension
		{
			get
			{
				if (string.IsNullOrEmpty(m_FileExtension))
					throw new("Should never get here !");
				return m_FileExtension;
			}
		}


		public PackageBundle()
		{
		}

		/// <summary>
		/// 解析资源包
		/// </summary>
		public void ParseBundle(string packageName, EAssetOutputNameStyle nameStype)
		{
			PackageName = packageName;
			m_FileExtension = Path.GetExtension(BundleName);
			m_FileName = FileSystem.GetRemoteBundleFileName(nameStype, BundleName, m_FileExtension, FileHash);
		}

		/// <summary>
		/// 获取解压BundleInfo
		/// </summary>
		public BundleInfo GetUnpackInfo()
		{
			// 注意：我们把流加载路径指定为远端下载地址
			string streamingPath = FileSystem.ToWWWPath(StreamingFilePath);
			BundleInfo bundleInfo = new(this, BundleInfo.ELoadMode.LoadFromStreaming, streamingPath, streamingPath);
			return bundleInfo;
		}

		public void ReadFromBuffer(BufferReader buffer)
		{
			BundleName = buffer.ReadUTF8();
			FileHash = buffer.ReadUTF8();
			FileCRC = buffer.ReadUTF8();
			FileSize = buffer.ReadInt64();
			IsRawFile = buffer.ReadBool();
			LoadMethod = buffer.ReadByte();
			buffer.ReadUTF8Array(Tags);
			buffer.ReadInt32Array(ReferenceIDs);
		}

		public void WriteToBuffer(BufferWriter buffer)
		{
			buffer.WriteUTF8(BundleName);
			buffer.WriteUTF8(FileHash);
			buffer.WriteUTF8(FileCRC);
			buffer.WriteInt64(FileSize);
			buffer.WriteBool(IsRawFile);
			buffer.WriteByte(LoadMethod);
			buffer.WriteUTF8Array(Tags);
			buffer.WriteInt32Array(ReferenceIDs);
		}

		/// <summary>
		/// 是否包含Tag
		/// </summary>
		public bool HasTag(string[] tags)
		{
			if (Collections.IsNullOrEmpty(tags))
			{
				return false;
			}

			if (Collections.IsNullOrEmpty(Tags))
			{
				return false;
			}

			foreach (string tag in tags)
			{
				if (Tags.Contains(tag))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// 是否包含任意Tags
		/// </summary>
		public bool HasAnyTags()
		{
			return Tags is { Count: > 0 };
		}

		public void UpdateTags(HashSet<string> tags)
		{
			foreach (string tag in tags)
			{
				if (!Tags.Contains(tag))
				{
					Tags.Add(tag);
				}
			}
		}

		/// <summary>
		/// 检测资源包文件内容是否相同
		/// </summary>
		public bool Equals(PackageBundle otherBundle)
		{
			return FileHash == otherBundle.FileHash;
		}
	}
}
