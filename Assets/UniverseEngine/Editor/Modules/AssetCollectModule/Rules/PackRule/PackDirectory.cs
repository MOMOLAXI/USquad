using System.IO;

namespace UniverseEngine.Editor
{
	/// <summary>
	/// 以父类文件夹路径作为资源包名
	/// 注意：文件夹下所有文件打进一个资源包
	/// 例如："Assets/UIPanel/Shop/Image/backgroud.png" --> "assets_uipanel_shop_image.bundle"
	/// 例如："Assets/UIPanel/Shop/View/main.prefab" --> "assets_uipanel_shop_view.bundle"
	/// </summary>
	[DisplayName("资源包名: 父类文件夹路径")]
	public class PackDirectory : IPackRule
	{
		PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
		{
			string bundleName = Path.GetDirectoryName(data.AssetPath);
			PackRuleResult result = new(bundleName, AssetCollectModule.ASSET_BUNDLE_FILE_EXTENSION);
			return result;
		}

		bool IPackRule.IsRawFilePackRule()
		{
			return false;
		}
	}
}
