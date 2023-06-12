namespace UniverseEngine.Editor
{
	[DisplayName("定位地址: 文件路径")]
	public class AddressByFilePath : IAddressRule
	{
		string IAddressRule.GetAssetAddress(AddressRuleData data)
		{
			return data.AssetPath;
		}
	}
}
