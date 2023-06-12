namespace UniverseEngine.Editor
{
	/// <summary>
	/// 打包着色器变种集合
	/// </summary>
	[DisplayName("打包着色器变种集合文件")]
	public class PackShaderVariants : IPackRule
	{
		public PackRuleResult GetPackRuleResult(PackRuleData data)
		{
			return AssetCollectModule.CreateShadersPackRuleResult();
		}

		bool IPackRule.IsRawFilePackRule()
		{
			return false;
		}
	}
}