namespace UniverseEngine.Editor
{
	/// <summary>
	/// 以文件路径作为资源包名
	/// 注意：每个文件独自打资源包
	/// 例如："Assets/UIPanel/Shop/Image/backgroud.png" --> "assets_uipanel_shop_image_backgroud.bundle"
	/// 例如："Assets/UIPanel/Shop/View/main.prefab" --> "assets_uipanel_shop_view_main.bundle"
	/// </summary>
	[DisplayName("资源包名: 文件路径")]
	public class PackSeparately : IPackRule
	{
		PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
		{
			string bundleName = StringUtilities.RemoveExtension(data.AssetPath);
			PackRuleResult result = new(bundleName, AssetCollectModule.ASSET_BUNDLE_FILE_EXTENSION);
			return result;
		}

		bool IPackRule.IsRawFilePackRule()
		{
			return false;
		}
	}
}
