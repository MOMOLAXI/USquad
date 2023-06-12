namespace UniverseEngine.Editor
{
	public class AssetBundleBuilderSetting : UniverseManagedSetting
	{
		/// <summary>
		/// 版本
		/// </summary>
		public string Version => $"{MajorVersion}.{SubVersion}.{RevisionVersion}";

		public int MajorVersion;
		public int SubVersion;
		public int RevisionVersion;

		/// <summary>
		/// 构建管线
		/// </summary>
		public EBuildPipeline BuildPipeline = EBuildPipeline.BuiltinBuildPipeline;

		/// <summary>
		/// 构建模式
		/// </summary>
		public EBuildMode BuildMode = EBuildMode.ForceRebuild;

		/// <summary>
		/// 构建的包裹名称
		/// </summary>
		public string BuildPackage = string.Empty;

		/// <summary>
		/// 压缩方式
		/// </summary>
		public ECompressOption CompressOption = ECompressOption.LZ4;

		/// <summary>
		/// 输出文件名称样式
		/// </summary>
		public EAssetOutputNameStyle OutputNameStyle = EAssetOutputNameStyle.HashName;

		/// <summary>
		/// 首包资源文件的拷贝方式
		/// </summary>
		public ECopyBuildinFileOption CopyBuildinFileOption = ECopyBuildinFileOption.None;

		/// <summary>
		/// 加密类名称
		/// </summary>
		public EAssetEncryption EncryptionType = EAssetEncryption.None;

		/// <summary>
		/// 共享资源的打包规则
		/// </summary>
		public EShareAssetPackRule ShareAssetPackRule = EShareAssetPackRule.Default;

		/// <summary>
		/// 首包资源文件的标签集合
		/// </summary>
		public string CopyBuildinFileTags = string.Empty;

		/// <summary>
		/// 验证构建结果
		/// </summary>
		public bool VerifyBuildResult = true;
	}
}
