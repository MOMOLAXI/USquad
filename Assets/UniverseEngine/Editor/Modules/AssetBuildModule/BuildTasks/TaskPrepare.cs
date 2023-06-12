using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace UniverseEngine.Editor
{
	public class TaskPrepare : IBuildTask
	{
		public string GetDisplayName() => "资源构建准备工作";

		public EBuildMode[] IgnoreBuildModes => Array.Empty<EBuildMode>();

		void IBuildTask.Run(BuildContext context)
		{
			BuildParametersContext buildParametersContext = context.GetContextObject<BuildParametersContext>();
			BuildArguments buildArguments = buildParametersContext.Arguments;

			// 检测构建参数合法性
			if (buildArguments.BuildTarget == BuildTarget.NoTarget)
			{
				throw new("请选择目标平台");
			}

			if (string.IsNullOrEmpty(buildArguments.PackageName))
			{
				throw new("包裹名称不能为空");
			}

			if (string.IsNullOrEmpty(buildArguments.PackageVersion))
			{
				throw new("包裹版本不能为空");
			}

			if (buildArguments.BuildMode != EBuildMode.SimulateBuild)
			{
				if (buildArguments.BuildPipeline == EBuildPipeline.BuiltinBuildPipeline)
				{
					Log<AssetBuildModule>.Warning("推荐使用可编程构建管线（SBP）！");
				}

				// 检测当前是否正在构建资源包
				if (BuildPipeline.isBuildingPlayer)
				{
					throw new("当前正在构建资源包，请结束后再试");
				}

				// 检测是否有未保存场景
				if (UniverseEditor.HasDirtyScenes())
				{
					for (int i = 0; i < SceneManager.sceneCount; ++i)
					{
						Scene scene = SceneManager.GetSceneAt(i);
						if (scene.isDirty)
						{
							Log<AssetBuildModule>.Info($"Save dirty scene : {scene.name}");
							EditorSceneManager.SaveScene(scene);
						}
					}
				}

				// 检测首包资源标签
				if (buildArguments.CopyBuildinFileOption is ECopyBuildinFileOption.ClearAndCopyByTags or ECopyBuildinFileOption.OnlyCopyByTags)
				{
					if (string.IsNullOrEmpty(buildArguments.CopyBuildinFileTags))
					{
						throw new("首包资源标签不能为空！");
					}
				}

				// 检测包裹输出目录是否存在
				string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
				if (Directory.Exists(packageOutputDirectory))
				{
					FileSystem.DeleteDirectory(packageOutputDirectory);
				}

				// 保存改动的资源
				AssetDatabase.SaveAssets();
			}

			if (buildArguments.BuildMode == EBuildMode.ForceRebuild)
			{
				// 删除总目录
				string platformDirectory = $"{buildArguments.OutputRoot}/{buildArguments.BuildTarget}/{buildArguments.PackageName}";
				if (FileSystem.DeleteDirectory(platformDirectory))
				{
					Log<AssetBuildModule>.Info($"删除平台总目录：{platformDirectory}");
				}
			}

			// 如果输出目录不存在
			string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
			if (FileSystem.CreateDirectory(pipelineOutputDirectory))
			{
				Log<AssetBuildModule>.Info($"创建输出目录：{pipelineOutputDirectory}");
			}
		}
	}
}
