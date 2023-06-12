namespace UniverseEngine.Editor
{
	public class TaskCreatePackage : IBuildTask
	{
		public string GetDisplayName() => "创建资源包";

		public EBuildMode[] IgnoreBuildModes => new[]
		{
			EBuildMode.SimulateBuild, EBuildMode.DryRunBuild
		};

		void IBuildTask.Run(BuildContext context)
		{
			BuildParametersContext buildParameters = context.GetContextObject<BuildParametersContext>();
			BuildMapContext buildMapContext = context.GetContextObject<BuildMapContext>();
			CopyPackageFiles(buildParameters, buildMapContext);
		}

		/// <summary>
		/// 拷贝补丁文件到补丁包目录
		/// </summary>
		private void CopyPackageFiles(BuildParametersContext buildParametersContext, BuildMapContext buildMapContext)
		{
			BuildArguments buildArguments = buildParametersContext.Arguments;
			string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
			string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
			Log<AssetBuildModule>.Info($"开始拷贝补丁文件到补丁包目录：{packageOutputDirectory}");

			if (buildArguments.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline)
			{
				// 拷贝构建日志
				{
					string sourcePath = $"{pipelineOutputDirectory}/buildlogtep.json";
					string destPath = $"{packageOutputDirectory}/buildlogtep.json";
					FileSystem.CopyFile(sourcePath, destPath, true);
				}

				// 拷贝代码防裁剪配置
				if (buildArguments.WriteLinkXML)
				{
					string sourcePath = $"{pipelineOutputDirectory}/link.xml";
					string destPath = $"{packageOutputDirectory}/link.xml";
					FileSystem.CopyFile(sourcePath, destPath, true);
				}
			}
			else if (buildArguments.BuildPipeline == EBuildPipeline.BuiltinBuildPipeline)
			{
				// 拷贝UnityManifest序列化文件
				{
					string sourcePath = $"{pipelineOutputDirectory}/{UniverseConstant.OUTPUT_FOLDER_NAME}";
					string destPath = $"{packageOutputDirectory}/{UniverseConstant.OUTPUT_FOLDER_NAME}";
					FileSystem.CopyFile(sourcePath, destPath, true);
				}

				// 拷贝UnityManifest文本文件
				{
					string sourcePath = $"{pipelineOutputDirectory}/{UniverseConstant.OUTPUT_FOLDER_NAME}.manifest";
					string destPath = $"{packageOutputDirectory}/{UniverseConstant.OUTPUT_FOLDER_NAME}.manifest";
					FileSystem.CopyFile(sourcePath, destPath, true);
				}
			}
			else
			{
				throw new System.NotImplementedException();
			}

			// 拷贝所有补丁文件
			foreach (BuildBundleInfo bundleInfo in buildMapContext.BundleCollection)
			{
				FileSystem.CopyFile(bundleInfo.BuildOutputFilePath, bundleInfo.PackageOutputFilePath, true);
			}
		}
	}
}
