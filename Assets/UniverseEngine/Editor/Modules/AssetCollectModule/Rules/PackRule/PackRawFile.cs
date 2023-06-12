namespace UniverseEngine.Editor
{
	/// <summary>
	/// 打包原生文件
	/// </summary>
	[DisplayName("打包原生文件")]
	public class PackRawFile : IPackRule
	{
		PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
		{
			string bundleName = data.AssetPath;
			PackRuleResult result = new(bundleName, AssetCollectModule.RAW_FILE_EXTENSION);
			return result;
		}

		bool IPackRule.IsRawFilePackRule()
		{
			return true;
		}
	}
}
