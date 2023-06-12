
namespace UniverseEngine.Editor
{
	/// <summary>
	/// 寻址规则接口
	/// </summary>
	public interface IAddressRule 
	{
		string GetAssetAddress(AddressRuleData data);
	}
}