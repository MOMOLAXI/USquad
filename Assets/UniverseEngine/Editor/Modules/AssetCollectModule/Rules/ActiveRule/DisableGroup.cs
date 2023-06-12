
namespace UniverseEngine.Editor
{
	[DisplayName("禁用分组")]
	public class DisableGroup : IActiveRule
	{
		public bool IsActive()
		{
			return false;
		}
	}
}