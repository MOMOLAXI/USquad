using System;
using System.IO;
using UnityEngine;
using UnityEngine.Build.Pipeline;

namespace UniverseEngine.Editor
{
	public class TaskUpdateBundleInfo : IBuildTask
	{
		public string GetDisplayName() => "更新资源包信息";

		public EBuildMode[] IgnoreBuildModes => Array.Empty<EBuildMode>();

		void IBuildTask.Run(BuildContext context)
		{
			BuildParametersContext buildParametersContext = context.GetContextObject<BuildParametersContext>();
			BuildMapContext buildMapContext = context.GetContextObject<BuildMapContext>();
			string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
			string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
			EAssetOutputNameStyle outputNameStyle = buildParametersContext.Arguments.OutputNameStyle;

			// 1.检测文件名长度
			foreach (BuildBundleInfo bundleInfo in buildMapContext.BundleCollection)
			{
				// NOTE：检测文件名长度不要超过260字符。
				string fileName = bundleInfo.BundleName;
				if (fileName.Length >= 260)
				{
					throw new($"The output bundle name is too long {fileName.Length} chars : {fileName}");
				}
			}

			// 2.更新构建输出的文件路径
			foreach (BuildBundleInfo bundleInfo in buildMapContext.BundleCollection)
			{
				bundleInfo.BuildOutputFilePath = bundleInfo.IsEncryptedFile
					                                 ? bundleInfo.EncryptedFilePath
					                                 : $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";
			}

			// 3.更新文件其它信息
			foreach (BuildBundleInfo bundleInfo in buildMapContext.BundleCollection)
			{
				string buildOutputFilePath = bundleInfo.BuildOutputFilePath;
				bundleInfo.ContentHash = GetBundleContentHash(bundleInfo, context);
				bundleInfo.FileHash = GetBundleFileHash(buildOutputFilePath, buildParametersContext);
				bundleInfo.FileCRC = GetBundleFileCRC(buildOutputFilePath, buildParametersContext);
				bundleInfo.FileSize = GetBundleFileSize(buildOutputFilePath, buildParametersContext);
			}

			// 4.更新补丁包输出的文件路径
			foreach (BuildBundleInfo bundleInfo in buildMapContext.BundleCollection)
			{
				string fileExtension = Path.GetExtension(bundleInfo.BundleName);
				string fileName = FileSystem.GetRemoteBundleFileName(outputNameStyle, bundleInfo.BundleName, fileExtension, bundleInfo.FileHash);
				bundleInfo.PackageOutputFilePath = $"{packageOutputDirectory}/{fileName}";
			}
		}

		static string GetBundleContentHash(BuildBundleInfo bundleInfo, BuildContext context)
		{
			BuildParametersContext buildParametersContext = context.GetContextObject<BuildParametersContext>();
			BuildArguments arguments = buildParametersContext.Arguments;
			EBuildMode buildMode = arguments.BuildMode;
			if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
			{
				return "00000000000000000000000000000000"; //32位
			}

			if (bundleInfo.IsRawFile)
			{
				string filePath = bundleInfo.BuildOutputFilePath;
				return HashUtilities.FileMD5(filePath);
			}

			switch (arguments.BuildPipeline)
			{
				case EBuildPipeline.BuiltinBuildPipeline:
				{
					TaskBuilding.BuildResultContext buildResult = context.GetContextObject<TaskBuilding.BuildResultContext>();
					Hash128 hash = buildResult.UnityManifest.GetAssetBundleHash(bundleInfo.BundleName);
					if (hash.isValid)
					{
						return hash.ToString();
					}

					throw new($"Not found bundle in build result : {bundleInfo.BundleName}");
				}
				case EBuildPipeline.ScriptableBuildPipeline:
				{
					// 注意：当资源包的依赖列表发生变化的时候，ContentHash也会发生变化！
					TaskBuilding_Sbp.BuildResultContext buildResult = context.GetContextObject<TaskBuilding_Sbp.BuildResultContext>();
					if (buildResult.Results.BundleInfos.TryGetValue(bundleInfo.BundleName, out BundleDetails value))
					{
						return value.Hash.ToString();
					}
					{
						throw new($"Not found bundle in build result : {bundleInfo.BundleName}");
					}
				}
				default:
					throw new NotImplementedException();
			}

		}

		static string GetBundleFileHash(string filePath, BuildParametersContext buildParametersContext)
		{
			EBuildMode buildMode = buildParametersContext.Arguments.BuildMode;
			if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
			{
				return "00000000000000000000000000000000"; //32位
			}

			return HashUtilities.FileMD5(filePath);
		}

		static string GetBundleFileCRC(string filePath, BuildParametersContext buildParametersContext)
		{
			EBuildMode buildMode = buildParametersContext.Arguments.BuildMode;
			if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
			{
				return "00000000"; //8位
			}

			return HashUtilities.FileCRC32(filePath);
		}

		static long GetBundleFileSize(string filePath, BuildParametersContext buildParametersContext)
		{
			EBuildMode buildMode = buildParametersContext.Arguments.BuildMode;
			if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
			{
				return 0;
			}

			return FileSystem.GetFileSize(filePath);
		}
	}
}
