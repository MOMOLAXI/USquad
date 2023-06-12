namespace UniverseEngine.Editor
{
	/// <summary>
	/// 资源打包规则接口
	/// </summary>
	public interface IPackRule
	{
		/// <summary>
		/// 获取打包规则结果
		/// </summary>
		PackRuleResult GetPackRuleResult(PackRuleData data);

		/// <summary>
		/// 是否为原生文件打包规则
		/// </summary>
		bool IsRawFilePackRule();
	}
}
