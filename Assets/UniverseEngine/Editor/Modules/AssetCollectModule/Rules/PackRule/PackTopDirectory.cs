using System.IO;

namespace UniverseEngine.Editor
{
	/// <summary>
	/// 以收集器路径下顶级文件夹为资源包名
	/// 注意：文件夹下所有文件打进一个资源包
	/// 例如：收集器路径为 "Assets/UIPanel"
	/// 例如："Assets/UIPanel/Shop/Image/backgroud.png" --> "assets_uipanel_shop.bundle"
	/// 例如："Assets/UIPanel/Shop/View/main.prefab" --> "assets_uipanel_shop.bundle"
	/// </summary>
	[DisplayName("资源包名: 收集器下顶级文件夹路径")]
	public class PackTopDirectory : IPackRule
	{
		PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
		{
			string assetPath = data.AssetPath.Replace(data.CollectPath, string.Empty);
			assetPath = assetPath.TrimStart('/');
			string[] splits = assetPath.Split('/');
			if (splits.Length > 0)
			{
				if (Path.HasExtension(splits[0]))
					throw new($"Not found root directory : {assetPath}");
				string bundleName = $"{data.CollectPath}/{splits[0]}";
				PackRuleResult result = new(bundleName, AssetCollectModule.ASSET_BUNDLE_FILE_EXTENSION);
				return result;
			}
			else
			{
				throw new($"Not found root directory : {assetPath}");
			}
		}

		bool IPackRule.IsRawFilePackRule()
		{
			return false;
		}
	}
}
