
namespace UniverseEngine
{
	/// <summary>
	/// 加密服务类接口
	/// </summary>
	public interface IAssetEncryption
	{
		AssetBundleEncryptResult Encrypt(AssetBundleEncryptFileInfo fileInfo);
	}
}