using System.IO;

namespace UniverseEngine.Editor
{
	[DisplayName("定位地址: 文件名")]
	public class AddressByFileName : IAddressRule
	{
		string IAddressRule.GetAssetAddress(AddressRuleData data)
		{
			return Path.GetFileNameWithoutExtension(data.AssetPath);
		}
	}
}
