using System.IO;

namespace UniverseEngine
{
	internal class AssetStreamEncryption : IAssetEncryption
	{
		public AssetBundleEncryptResult Encrypt(AssetBundleEncryptFileInfo fileInfo)
		{
			byte[] fileData = File.ReadAllBytes(fileInfo.FilePath);
			for (int i = 0; i < fileData.Length; i++)
			{
				fileData[i] ^= BundleStream.KEY;
			}

			AssetBundleEncryptResult result = new()
			{
				LoadMethod = EBundleLoadMethod.LoadFromStream,
				EncryptedData = fileData
			};
			return result;
		}
	}
}
