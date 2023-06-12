using UnityEditor;

namespace UniverseEngine.Editor
{
	/// <summary>
	/// 以收集器路径作为资源包名
	/// 注意：收集的所有文件打进一个资源包
	/// </summary>
	[DisplayName("资源包名: 收集器路径")]
	public class PackCollector : IPackRule
	{
		PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
		{
			string bundleName;
			string collectPath = data.CollectPath;
			if (AssetDatabase.IsValidFolder(collectPath))
			{
				bundleName = collectPath;
			}
			else
			{
				bundleName = StringUtilities.RemoveExtension(collectPath);
			}

			PackRuleResult result = new(bundleName, AssetCollectModule.ASSET_BUNDLE_FILE_EXTENSION);
			return result;
		}

		bool IPackRule.IsRawFilePackRule()
		{
			return false;
		}
	}
}
