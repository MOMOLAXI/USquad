using System.IO;

namespace UniverseEngine.Editor
{
	public class DefaultShareAssetPackRule : IShareAssetPackRule
	{
		public PackRuleResult GetPackRuleResult(string assetPath)
		{
			string bundleName = Path.GetDirectoryName(assetPath);
			PackRuleResult result = new(bundleName, AssetCollectModule.ASSET_BUNDLE_FILE_EXTENSION);
			return result;
		}
	}
}