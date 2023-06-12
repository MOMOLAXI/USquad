using UnityEditor;

namespace UniverseEngine.Editor
{
	public class AssetBuildModule : UniverseEditorModule<AssetBundleBuilderSetting>
	{
		/// <summary>
		/// 模拟构建(运行时调用)
		/// </summary>
		public static string SimulateBuild(string packageName)
		{
			Log<AssetBuildModule>.Info($"Begin to create simulate package : {packageName}");
			BuildArguments buildArguments = new()
			{
				OutputRoot = FileSystem.GetBundleOutputDirectory(),
				BuildTarget = EditorUserBuildSettings.activeBuildTarget,
				BuildMode = EBuildMode.SimulateBuild,
				PackageName = packageName,
				PackageVersion = "Simulate",
			};

			BuildResult buildResult = StartBuild(buildArguments);
			if (buildResult.IsSuccess)
			{
				string manifestFileName = FileSystem.GetManifestBinaryFileName(buildArguments.PackageName, buildArguments.PackageVersion);
				string manifestFilePath = FileSystem.ToPath(buildResult.OutputPackageDirectory, manifestFileName);
				return manifestFilePath;
			}

			return null;
		}

		public static BuildResult StartBuild()
		{
			BuildArguments arguments = ConvertSettingToArguments();
			return StartBuild(arguments);
		}

		public static void ImportSettingFromXml(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				return;
			}

			BuildArguments arguments = FileSystem.DeserializeFromXml(filePath, () => new BuildArguments());
			ConvertArgumentsToSetting(arguments);
		}

		public static void ExportBuildArgumentsToXml()
		{
			string directory = WindowModule.OpenFolderPanel("Export XML", "Assets/");
			if (!string.IsNullOrEmpty(directory))
			{
				string path = FileSystem.ToXmlPath(directory, $"{nameof(BuildArguments)}_{Setting.BuildPackage}");
				FileSystem.SerializeToXml(ConvertSettingToArguments(), path);
				EditorLog.Info($"导出构建配置完成 {path}");
			}
		}

		public static BuildArguments ConvertSettingToArguments()
		{
			return new()
			{
				BuildPipeline = Setting.BuildPipeline,
				BuildMode = Setting.BuildMode,
				PackageName = Setting.BuildPackage,
				PackageVersion = Setting.Version,
				VerifyBuildingResult = Setting.VerifyBuildResult,
				ShareAssetPackRule = Setting.ShareAssetPackRule,
				EncryptionType = Setting.EncryptionType,
				CompressOption = Setting.CompressOption,
				OutputNameStyle = Setting.OutputNameStyle,
				CopyBuildinFileOption = Setting.CopyBuildinFileOption,
				CopyBuildinFileTags = Setting.CopyBuildinFileTags,
				WriteLinkXML = Setting.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline
			};
		}

		public static void ConvertArgumentsToSetting(BuildArguments arguments)
		{
			if (arguments == null)
			{
				return;
			}

			Setting.BuildPipeline = arguments.BuildPipeline;
			Setting.BuildMode = arguments.BuildMode;
			Setting.BuildPackage = arguments.PackageName;
			string[] versions = arguments.PackageVersion.Split(".");
			if (versions.Length > 0)
			{
				Setting.MajorVersion = versions[0].ToInt();
			}

			if (versions.Length > 1)
			{
				Setting.SubVersion = versions[1].ToInt();
			}

			if (versions.Length > 2)
			{
				Setting.RevisionVersion = versions[2].ToInt();
			}

			Setting.VerifyBuildResult = arguments.VerifyBuildingResult;
			Setting.ShareAssetPackRule = arguments.ShareAssetPackRule;
			Setting.EncryptionType = arguments.EncryptionType;
			Setting.CompressOption = arguments.CompressOption;
			Setting.OutputNameStyle = arguments.OutputNameStyle;
			Setting.CopyBuildinFileOption = arguments.CopyBuildinFileOption;
			Setting.CopyBuildinFileTags = arguments.CopyBuildinFileTags;
			Setting.Save();
			EditorLog.Info("导入构建设置完成");
		}

		/// <summary>
		/// 开始构建
		/// </summary>
		public static BuildResult StartBuild(BuildArguments arguments)
		{
			// 执行构建流程
			BuildResult buildResult = UniverseBuildPipeline.Start(arguments)
			                                               .Task<TaskPrepare>()
			                                               .Task<TaskCreateBuildMap>()
			                                               .TaskVariant<TaskBuilding, TaskBuilding_Sbp>()
			                                               .Task<TaskCopyRawFile>()
			                                               .TaskVariant<TaskVerifyBuildResult ,TaskVerifyBuildResult_Sbp>()
			                                               .Task<TaskEncryption>()
			                                               .Task<TaskUpdateBundleInfo>()
			                                               .Task<TaskCreateManifest>()
			                                               .Task<TaskCreateReport>()
			                                               .Task<TaskCreatePackage>()
			                                               .Task<TaskCopyBuildinFiles>()
			                                               .Build();
			if (buildResult.IsSuccess)
			{
				Log<AssetBuildModule>.Info($"{arguments.BuildMode} pipeline build succeed !");
			}
			else
			{
				Log<AssetBuildModule>.Warning($"{arguments.BuildMode} pipeline build failed !");
				Log<AssetBuildModule>.Error($"Build task failed : {buildResult.FailedTask}");
				Log<AssetBuildModule>.Error(buildResult.ErrorInfo);
			}

			return buildResult;
		}
	}
}
