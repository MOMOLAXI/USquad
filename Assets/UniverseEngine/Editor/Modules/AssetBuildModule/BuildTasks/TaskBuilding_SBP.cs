using System.Collections.Generic;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;

namespace UniverseEngine.Editor
{
	public class TaskBuilding_Sbp : IBuildTask
	{
		public string GetDisplayName() => "资源构建内容打包";

		public class BuildResultContext : IContextObject
		{
			public IBundleBuildResults Results;
		}

		public EBuildMode[] IgnoreBuildModes => new[] { EBuildMode.SimulateBuild };

		void IBuildTask.Run(BuildContext context)
		{
			BuildParametersContext buildParametersContext = context.GetContextObject<BuildParametersContext>();
			BuildMapContext buildMapContext = context.GetContextObject<BuildMapContext>();

			// 构建内容
			BundleBuildContent buildContent = new(buildMapContext.GetPipelineBuilds());

			// 开始构建
			BundleBuildParameters buildParameters = buildParametersContext.GetSbpBuildParameters();
			IList<UnityEditor.Build.Pipeline.Interfaces.IBuildTask> taskList = SbpBuildTasks.Create(buildMapContext.ShadersBundleName);
			ReturnCode exitCode = ContentPipeline.BuildAssetBundles(buildParameters, buildContent, out IBundleBuildResults buildResults, taskList);
			if (exitCode < 0)
			{
				throw new($"构建过程中发生错误 : {exitCode}");
			}

			// 创建着色器信息
			// 说明：解决因为着色器资源包导致验证失败。
			// 例如：当项目里没有着色器，如果有依赖内置着色器就会验证失败。
			string shadersBundleName = buildMapContext.ShadersBundleName;
			if (buildResults.BundleInfos.ContainsKey(shadersBundleName))
			{
				buildMapContext.CreateShadersBundleInfo(shadersBundleName);
			}

			Log<AssetBuildModule>.Info("Unity引擎打包成功！");
			BuildResultContext buildResultContext = new()
			{
				Results = buildResults
			};
			context.SetContextObject(buildResultContext);
		}
	}
}
