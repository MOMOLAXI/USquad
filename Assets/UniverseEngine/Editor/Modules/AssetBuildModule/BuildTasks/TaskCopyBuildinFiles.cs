using UnityEditor;

namespace UniverseEngine.Editor
{
	public class TaskCopyBuildinFiles : IBuildTask
	{
		public string GetDisplayName() => "拷贝内置文件到流目录";

		public EBuildMode[] IgnoreBuildModes => new[]
		{
			EBuildMode.SimulateBuild, EBuildMode.DryRunBuild
		};

		void IBuildTask.Run(BuildContext context)
		{
			BuildParametersContext buildParametersContext = context.GetContextObject<BuildParametersContext>();
			ManifestContext manifestContext = context.GetContextObject<ManifestContext>();
			if (buildParametersContext.Arguments.CopyBuildinFileOption != ECopyBuildinFileOption.None)
			{
				CopyBuildinFilesToStreaming(buildParametersContext, manifestContext);
			}
		}

		/// <summary>
		/// 拷贝首包资源文件
		/// </summary>
		private void CopyBuildinFilesToStreaming(BuildParametersContext buildParametersContext, ManifestContext manifestContext)
		{
			ECopyBuildinFileOption option = buildParametersContext.Arguments.CopyBuildinFileOption;
			string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
			string streamingAssetsDirectory = FileSystem.GetStreamingAssetsBuiltInFolderPath();
			string buildPackageName = buildParametersContext.Arguments.PackageName;
			string buildPackageVersion = buildParametersContext.Arguments.PackageVersion;

			// 加载补丁清单
			PackageManifest manifest = manifestContext.Manifest;

			// 清空流目录
			if (option == ECopyBuildinFileOption.ClearAndCopyAll || option == ECopyBuildinFileOption.ClearAndCopyByTags)
			{
				FileSystem.ClearStreamingAssetsBuiltInFolder();
			}

			// 拷贝补丁清单文件
			{
				string fileName = FileSystem.GetManifestBinaryFileName(buildPackageName, buildPackageVersion);
				string sourcePath = $"{packageOutputDirectory}/{fileName}";
				string destPath = $"{streamingAssetsDirectory}/{fileName}";
				FileSystem.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝补丁清单哈希文件
			{
				string fileName = FileSystem.GetPackageHashFileName(buildPackageName, buildPackageVersion);
				string sourcePath = $"{packageOutputDirectory}/{fileName}";
				string destPath = $"{streamingAssetsDirectory}/{fileName}";
				FileSystem.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝补丁清单版本文件
			{
				string fileName = FileSystem.GetPackageVersionFileName(buildPackageName);
				string sourcePath = $"{packageOutputDirectory}/{fileName}";
				string destPath = $"{streamingAssetsDirectory}/{fileName}";
				FileSystem.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝文件列表（所有文件）
			if (option is ECopyBuildinFileOption.ClearAndCopyAll or ECopyBuildinFileOption.OnlyCopyAll)
			{
				manifest.CopyBundles(packageOutputDirectory, streamingAssetsDirectory);
			}

			// 拷贝文件列表（带标签的文件）
			if (option == ECopyBuildinFileOption.ClearAndCopyByTags || option == ECopyBuildinFileOption.OnlyCopyByTags)
			{
				string[] tags = buildParametersContext.Arguments.CopyBuildinFileTags.Split(';');
				manifest.CopyBundles(packageOutputDirectory, streamingAssetsDirectory, bundle => bundle.HasTag(tags));
			}

			// 刷新目录
			AssetDatabase.Refresh();
			Log<AssetBuildModule>.Info($"内置文件拷贝完成：{streamingAssetsDirectory}");
		}
	}
}
