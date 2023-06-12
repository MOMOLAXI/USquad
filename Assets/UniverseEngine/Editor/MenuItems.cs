using UnityEditor;

namespace UniverseEngine.Editor
{
	public static class UniverseMenuItems
	{
		[MenuItem("UniverseEngine/AssetBundleBuilder")]
		public static void ShowAssetBuilderWindow()
		{
			AssetBundleBuilderWindow window = WindowModule.Open<AssetBundleBuilderWindow>("资源包构建工具", true, AssetWindowDefine.DockedWindowTypes);
			window.minSize = new(800, 600);
		}

		[MenuItem("UniverseEngine/AssetBundleCollector")]
		public static void ShowAssetCollectorWindow()
		{
			AssetBundleCollectorWindow window = WindowModule.Open<AssetBundleCollectorWindow>("资源包收集工具", true, AssetWindowDefine.DockedWindowTypes);
			window.minSize = new(800, 600);
		}

		[MenuItem("UniverseEngine/AssetBundleReporter")]
		public static void ShowAssetReporterWindow()
		{
			AssetBundleReporterWindow window = WindowModule.Open<AssetBundleReporterWindow>("资源包报告工具", true, AssetWindowDefine.DockedWindowTypes);
			window.minSize = new(800, 600);
		}

		[MenuItem("UniverseEngine/AssetBundleDebugger")]
		public static void ShowAssetDebuggerWindow()
		{
			AssetBundleDebuggerWindow wnd = WindowModule.Open<AssetBundleDebuggerWindow>("资源包调试工具", true, AssetWindowDefine.DockedWindowTypes);
			wnd.minSize = new(800, 600);
		}

		[MenuItem("UniverseEngine/ShaderVariantCollector")]
		public static void ShowShaderVariantCollectorWindow()
		{
			ShaderVariantCollectorWindow window = WindowModule.Open<ShaderVariantCollectorWindow>("着色器变种收集工具", true, AssetWindowDefine.DockedWindowTypes);
			window.minSize = new(800, 600);
		}

		[MenuItem("UniverseEngine/UniTask Tracker")]
		public static void OpenWindow()
		{
			WindowModule.Open<UniTaskTrackerWindow>("UniTask Tracker", true);
		}

		[MenuItem("UniverseEngine/Test")]
		public static void Test() { }
	}
}
