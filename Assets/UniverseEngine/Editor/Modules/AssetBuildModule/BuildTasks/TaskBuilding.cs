using System.IO;
using UnityEditor;
using UnityEngine;

namespace UniverseEngine.Editor
{
	public struct UnityManifest
	{
		public AssetBundleManifest Manifest;
	}
	
	public class TaskBuilding : IBuildTask
	{
		public string GetDisplayName() => "资源构建内容打包";

		public EBuildMode[] IgnoreBuildModes => new[] { EBuildMode.SimulateBuild };

		public class BuildResultContext : IContextObject
		{
			public AssetBundleManifest UnityManifest;
		}
		
		void IBuildTask.Run(BuildContext context)
		{
			BuildParametersContext buildParametersContext = context.GetContextObject<BuildParametersContext>();

			// 开始构建
			string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
			BuildPipelineArgs args = context.GetPipelineArguments();
			AssetBundleManifest buildResults = BuildPipeline.BuildAssetBundles(args.OutputPath, args.Builds, args.AssetBundleOptions, args.TargetPlatform);
			if (buildResults == null)
			{
				throw new("构建过程中发生错误！");
			}

			EBuildMode buildMode = buildParametersContext.Arguments.BuildMode;
			if (buildMode is EBuildMode.ForceRebuild or EBuildMode.IncrementalBuild)
			{
				string unityOutputManifestFilePath = $"{pipelineOutputDirectory}/{UniverseConstant.OUTPUT_FOLDER_NAME}";
				if (!File.Exists(unityOutputManifestFilePath))
				{
					throw new("构建过程中发生严重错误！请查阅上下文日志！");
				}
			}

			Log<AssetBuildModule>.Info("Unity引擎打包成功！");
			BuildResultContext buildResultContext = new()
			{
				UnityManifest = buildResults
			};

			context.SetContextObject(buildResultContext);
		}
	}
}
