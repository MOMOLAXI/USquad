using System;
using System.IO;

namespace UniverseEngine
{
	/// <summary>
	/// 资源文件解密服务类
	/// </summary>
	internal class AssetDefaultDecryption : IAssetDecryption
	{
		public ulong LoadFromFileOffset(AssetBundleDecryptFileInfo fileInfo)
		{
			return 32;
		}

		public byte[] LoadFromMemory(AssetBundleDecryptFileInfo fileInfo)
		{
			throw new NotImplementedException();
		}

		public Stream LoadFromStream(AssetBundleDecryptFileInfo fileInfo)
		{
			BundleStream bundleStream = new(fileInfo.FilePath, FileMode.Open);
			return bundleStream;
		}

		public uint GetManagedReadBufferSize()
		{
			return 1024;
		}
	}
}
