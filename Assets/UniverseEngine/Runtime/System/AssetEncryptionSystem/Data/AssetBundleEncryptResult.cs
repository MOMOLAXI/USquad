namespace UniverseEngine
{
	public struct AssetBundleEncryptResult
	{
		/// <summary>
		/// 加密后的Bunlde文件加载方法
		/// </summary>
		public EBundleLoadMethod LoadMethod;
		
		/// <summary>
		/// 加密后的文件数据
		/// </summary>
		public byte[] EncryptedData;
	}
}
