using System;
using System.Collections.Generic;
using System.Linq;

namespace UniverseEngine
{
	[Serializable]
	internal class Asset
	{
		/// <summary>
		/// 可寻址地址
		/// </summary>
		public string Address;

		/// <summary>
		/// 资源路径
		/// </summary>
		public string AssetPath;

		/// <summary>
		/// 资源的分类标签
		/// </summary>
		public List<string> AssetTags = new();

		/// <summary>
		/// 所属资源包ID
		/// </summary>
		public int BundleID;

		/// <summary>
		/// 依赖的资源包ID列表
		/// </summary>
		public List<int> DependIDs = new();

		public void ReadFromBuffer(BufferReader buffer)
		{
			Address = buffer.ReadUTF8();
			AssetPath = buffer.ReadUTF8();
			BundleID = buffer.ReadInt32();
			buffer.ReadUTF8Array(AssetTags);
			buffer.ReadInt32Array(DependIDs);
		}

		public void WriteToBuffer(BufferWriter buffer)
		{
			buffer.WriteUTF8(Address);
			buffer.WriteUTF8(AssetPath);
			buffer.WriteInt32(BundleID);
			buffer.WriteUTF8Array(AssetTags);
			buffer.WriteInt32Array(DependIDs);
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

			if (Collections.IsNullOrEmpty(AssetTags))
			{
				return false;
			}

			foreach (string tag in tags)
			{
				if (AssetTags.Contains(tag))
				{
					return true;
				}
			}

			return false;
		}
	}
}
