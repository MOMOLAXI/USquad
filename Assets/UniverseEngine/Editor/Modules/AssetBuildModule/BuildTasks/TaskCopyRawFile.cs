namespace UniverseEngine.Editor
{
	public class TaskCopyRawFile : IBuildTask
	{
		public string GetDisplayName() => "拷贝原生文件";

		public EBuildMode[] IgnoreBuildModes => new[]
		{
			EBuildMode.SimulateBuild, EBuildMode.DryRunBuild
		};

		void IBuildTask.Run(BuildContext context)
		{
			BuildParametersContext buildParametersContext = context.GetContextObject<BuildParametersContext>();
			BuildMapContext buildMapContext = context.GetContextObject<BuildMapContext>();
			CopyRawBundle(buildMapContext, buildParametersContext);
		}

		/// <summary>
		/// 拷贝原生文件
		/// </summary>
		private void CopyRawBundle(BuildMapContext buildMapContext, BuildParametersContext buildParametersContext)
		{
			string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
			foreach (BuildBundleInfo bundleInfo in buildMapContext.BundleCollection)
			{
				if (bundleInfo.IsRawFile)
				{
					string dest = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";
					foreach (BuildAssetInfo assetInfo in bundleInfo.AllMainAssets)
					{
						if (assetInfo.IsRawAsset)
						{
							FileSystem.CopyFile(assetInfo.AssetPath, dest, true);
						}
					}
				}
			}
		}
	}
}
