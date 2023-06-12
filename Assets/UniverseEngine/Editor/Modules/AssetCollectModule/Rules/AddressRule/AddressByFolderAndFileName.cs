using System.IO;

namespace UniverseEngine.Editor
{
	[DisplayName("定位地址: 文件夹名+文件名")]
	public class AddressByFolderAndFileName : IAddressRule
	{
		string IAddressRule.GetAssetAddress(AddressRuleData data)
		{
			string fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
			FileInfo fileInfo = new(data.AssetPath);
			return $"{fileInfo.Directory.Name}_{fileName}";
		}
	}
}