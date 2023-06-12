using System.IO;

namespace UniverseEngine.Editor
{
	[DisplayName("收集场景")]
	public class CollectScene : IFilterRule
	{
		public bool IsCollectAsset(FilterRuleData data)
		{
			return Path.GetExtension(data.AssetPath) == ".unity";
		}
	}
}
