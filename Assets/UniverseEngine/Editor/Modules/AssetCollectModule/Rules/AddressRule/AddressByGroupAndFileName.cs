using System.IO;

namespace UniverseEngine.Editor
{
	[DisplayName("定位地址: 分组名+文件名")]
	public class AddressByGroupAndFileName : IAddressRule
	{
		string IAddressRule.GetAssetAddress(AddressRuleData data)
		{
			string fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
			return $"{data.GroupName}_{fileName}";
		}
	}
}
