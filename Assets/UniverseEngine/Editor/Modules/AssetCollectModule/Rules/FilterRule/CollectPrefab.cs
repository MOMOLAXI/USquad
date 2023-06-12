using System.IO;

namespace UniverseEngine.Editor
{
	[DisplayName("收集预制体")]
	public class CollectPrefab : IFilterRule
	{
		public bool IsCollectAsset(FilterRuleData data)
		{
			return Path.GetExtension(data.AssetPath) == ".prefab";
		}
	}
}
