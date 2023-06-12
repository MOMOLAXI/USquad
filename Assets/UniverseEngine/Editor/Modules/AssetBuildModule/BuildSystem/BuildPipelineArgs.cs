using UnityEditor;

namespace UniverseEngine.Editor
{
	public struct BuildPipelineArgs
	{
		public string OutputPath;
		public AssetBundleBuild[] Builds;
		public BuildAssetBundleOptions AssetBundleOptions;
		public BuildTarget TargetPlatform;
	}
}
