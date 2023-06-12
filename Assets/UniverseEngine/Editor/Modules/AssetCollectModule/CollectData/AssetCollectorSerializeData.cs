using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace UniverseEngine.Editor
{
	[XmlRoot]
	[Serializable]
	public class AssetCollectorSerializeData
	{
		/// <summary>
		/// 是否显示包裹列表视图
		/// </summary>
		[XmlAttribute]
		public bool ShowPackageView;

		/// <summary>
		/// 是否启用可寻址资源定位
		/// </summary>
		[XmlAttribute]
		public bool EnableAddressable;

		/// <summary>
		/// 资源包名唯一化
		/// </summary>
		[XmlAttribute]
		public bool UniqueBundleName;

		/// <summary>
		/// 是否显示编辑器别名
		/// </summary>
		[XmlAttribute]
		public bool ShowEditorAlias;

		/// <summary>
		/// 包裹列表
		/// </summary>
		[XmlElement]
		public List<AssetBundleCollectorPackage> Packages = new();

		/// <summary>
		/// 清空所有数据
		/// </summary>
		public void ClearAll()
		{
			EnableAddressable = false;
			Packages.Clear();
		}

		/// <summary>
		/// 检测配置错误
		/// </summary>
		public void CheckConfigError()
		{
			foreach (AssetBundleCollectorPackage package in Packages)
			{
				package.CheckConfigError();
			}
		}

		/// <summary>
		/// 修复配置错误
		/// </summary>
		public bool FixConfigError()
		{
			bool isFixed = false;
			foreach (AssetBundleCollectorPackage package in Packages)
			{
				if (package.FixConfigError())
				{
					isFixed = true;
				}
			}

			return isFixed;
		}

		/// <summary>
		/// 获取所有的资源标签
		/// </summary>
		public List<string> GetPackageAllTags(string packageName)
		{
			foreach (AssetBundleCollectorPackage package in Packages)
			{
				if (package.PackageName == packageName)
				{
					return package.GetAllTags();
				}
			}

			EditorLog.Warning($"Not found package : {packageName}");
			return new();
		}

		/// <summary>
		/// 获取包裹收集的资源文件
		/// </summary>
		public CollectResult GetPackageAssets(EBuildMode buildMode, string packageName)
		{
			if (string.IsNullOrEmpty(packageName))
			{
				throw new("Build package name is null or empty !");
			}

			foreach (AssetBundleCollectorPackage package in Packages)
			{
				if (package.PackageName == packageName)
				{
					CollectCommand command = new(buildMode, packageName, EnableAddressable, UniqueBundleName);
					CollectResult collectResult = new(command);
					collectResult.SetCollectAssets(package.GetAllCollectAssets(command));
					return collectResult;
				}
			}

			throw new($"Not found collector package : {packageName}");
		}
	}
}
