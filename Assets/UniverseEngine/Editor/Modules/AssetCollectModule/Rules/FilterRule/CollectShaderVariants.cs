using System.IO;

namespace UniverseEngine.Editor
{
	[DisplayName("收集着色器变种集合")]
	public class CollectShaderVariants : IFilterRule
	{
		public bool IsCollectAsset(FilterRuleData data)
		{
			return Path.GetExtension(data.AssetPath) == ".shadervariants";
		}
	}
}
