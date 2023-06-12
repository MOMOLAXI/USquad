using System;
using System.Xml.Serialization;
using UnityEditor;

namespace UniverseEngine.Editor
{
	/// <summary>
	/// 构建参数
	/// </summary>
	[Serializable]
	[XmlRoot]
	public class BuildArguments
	{
	#region SBP

		/// <summary>
		/// 生成代码防裁剪配置
		/// </summary>
		[XmlAttribute]
		public bool WriteLinkXML;

		/// <summary>
		/// 缓存服务器地址
		/// </summary>
		[XmlElement]
		public string CacheServerHost;

		/// <summary>
		/// 缓存服务器端口
		/// </summary>
		[XmlElement]
		public int CacheServerPort;

	#endregion

		/// <summary>
		/// 输出的根目录
		/// </summary>
		[XmlElement]
		public string OutputRoot = FileSystem.GetBundleOutputDirectory();

		/// <summary>
		/// 构建的平台
		/// </summary>
		[XmlElement]
		public BuildTarget BuildTarget = EditorUserBuildSettings.activeBuildTarget;

		/// <summary>
		/// 构建管线
		/// </summary>
		[XmlElement]
		public EBuildPipeline BuildPipeline;

		/// <summary>
		/// 构建模式
		/// </summary>
		[XmlElement]
		public EBuildMode BuildMode;

		/// <summary>
		/// 构建的包裹名称
		/// </summary>
		[XmlElement]
		public string PackageName;

		/// <summary>
		/// 构建的包裹版本
		/// </summary>
		[XmlElement]
		public string PackageVersion;

		/// <summary>
		/// 验证构建结果
		/// </summary>
		[XmlElement]
		public bool VerifyBuildingResult;

		/// <summary>
		/// 共享资源的打包规则
		/// </summary>
		[XmlElement]
		public EShareAssetPackRule ShareAssetPackRule;

		/// <summary>
		/// 资源的加密接口
		/// </summary>
		[XmlElement]
		public EAssetEncryption EncryptionType;

		/// <summary>
		/// 补丁文件名称的样式
		/// </summary>
		[XmlElement]
		public EAssetOutputNameStyle OutputNameStyle;

		/// <summary>
		/// 拷贝内置资源选项
		/// </summary>
		[XmlElement]
		public ECopyBuildinFileOption CopyBuildinFileOption;

		/// <summary>
		/// 拷贝内置资源的标签
		/// </summary>
		[XmlElement]
		public string CopyBuildinFileTags;

		/// <summary>
		/// 压缩选项
		/// </summary>
		[XmlElement]
		public ECompressOption CompressOption;

		/// <summary>
		/// 禁止写入类型树结构（可以降低包体和内存并提高加载效率）
		/// </summary>
		[XmlElement]
		public bool DisableWriteTypeTree;

		/// <summary>
		/// 忽略类型树变化
		/// </summary>
		[XmlElement]
		public bool IgnoreTypeTreeChanges;
	}
}
