namespace UniverseEngine.Editor
{
	public struct PackRuleData
	{
		public string AssetPath;
		public string CollectPath;
		public string GroupName;
		public string UserData;

		public PackRuleData(string assetPath)
		{
			AssetPath = assetPath;
			CollectPath = string.Empty;
			GroupName = string.Empty;
			UserData = string.Empty;
		}
		public PackRuleData(string assetPath, string collectPath, string groupName, string userData)
		{
			AssetPath = assetPath;
			CollectPath = collectPath;
			GroupName = groupName;
			UserData = userData;
		}
	}
}
