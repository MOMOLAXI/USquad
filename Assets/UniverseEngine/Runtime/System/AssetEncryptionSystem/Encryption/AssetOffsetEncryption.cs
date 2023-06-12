using System;
using System.IO;

namespace UniverseEngine
{
	internal class AssetOffsetEncryption : IAssetEncryption
	{
		const int OFFSET = 32;

		public AssetBundleEncryptResult Encrypt(AssetBundleEncryptFileInfo fileInfo)
		{
			byte[] fileData = File.ReadAllBytes(fileInfo.FilePath);
			byte[] encryptedData = new byte[fileData.Length + OFFSET];
			Buffer.BlockCopy(fileData, 0, encryptedData, OFFSET, fileData.Length);
			return new()
			{
				LoadMethod = EBundleLoadMethod.LoadFromFileOffset,
				EncryptedData = encryptedData
			};
		}
	}
}
