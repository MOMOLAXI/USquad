namespace UniverseEngine.Editor
{
	[DisplayName("收集所有资源")]
	public class CollectAll : IFilterRule
	{
		public bool IsCollectAsset(FilterRuleData data)
		{
			return true;
		}
	}
}
