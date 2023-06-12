
namespace UniverseEngine
{
	/// <summary>
	/// 解密类服务接口
	/// </summary>
	public interface IAssetDecryption
	{
		/// <summary>
		/// 文件偏移解密方法(AssetOffsetEncryption)
		/// </summary>
		ulong LoadFromFileOffset(AssetBundleDecryptFileInfo fileInfo);

		/// <summary>
		/// 文件内存解密方法(TODO)
		/// </summary>
		byte[] LoadFromMemory(AssetBundleDecryptFileInfo fileInfo);

		/// <summary>
		/// 文件流解密方法(AssetStreamEncryption)
		/// </summary>
		System.IO.Stream LoadFromStream(AssetBundleDecryptFileInfo fileInfo);

		/// <summary>
		/// 文件流解密的托管缓存大小
		/// </summary>
		uint GetManagedReadBufferSize();
	}
}