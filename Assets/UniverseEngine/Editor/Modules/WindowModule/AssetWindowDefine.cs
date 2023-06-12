using System;

namespace UniverseEngine.Editor
{
	public static class AssetWindowDefine
	{
		/// <summary>
		/// 停靠窗口类型集合
		/// </summary>
		public static readonly Type[] DockedWindowTypes =
		{
			typeof(AssetBundleBuilderWindow),
			typeof(AssetBundleCollectorWindow),
			typeof(AssetBundleDebuggerWindow),
			typeof(AssetBundleReporterWindow),
			typeof(ShaderVariantCollectorWindow)
		};
	}
}
