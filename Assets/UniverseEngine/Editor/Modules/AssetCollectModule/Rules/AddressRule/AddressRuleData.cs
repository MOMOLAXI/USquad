namespace UniverseEngine.Editor
{
	public struct AddressRuleData
	{
		public string AssetPath;
		public string CollectPath;
		public string GroupName;
		public string UserData;

		public AddressRuleData(string assetPath, string collectPath, string groupName, string userData)
		{
			AssetPath = assetPath;
			CollectPath = collectPath;
			GroupName = groupName;
			UserData = userData;
		}
	}
}
