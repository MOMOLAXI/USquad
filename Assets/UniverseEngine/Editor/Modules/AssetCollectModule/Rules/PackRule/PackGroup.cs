namespace UniverseEngine.Editor
{
	/// <summary>
	/// 以分组名称作为资源包名
	/// 注意：收集的所有文件打进一个资源包
	/// </summary>
	[DisplayName("资源包名: 分组名称")]
	public class PackGroup : IPackRule
	{
		PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
		{
			string bundleName = data.GroupName;
			PackRuleResult result = new(bundleName, AssetCollectModule.ASSET_BUNDLE_FILE_EXTENSION);
			return result;
		}

		bool IPackRule.IsRawFilePackRule()
		{
			return false;
		}
	}
}
