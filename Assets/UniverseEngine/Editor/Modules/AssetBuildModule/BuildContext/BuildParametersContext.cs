using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEngine;

namespace UniverseEngine.Editor
{
	public class BuildParametersContext : IContextObject
	{
		string m_PipelineOutputDirectory = string.Empty;
		string m_PackageOutputDirectory = string.Empty;

		/// <summary>
		/// 构建参数
		/// </summary>
		public BuildArguments Arguments { get; }

		public BuildParametersContext(BuildArguments arguments)
		{
			Arguments = arguments;
		}

		/// <summary>
		/// 获取构建管线的输出目录
		/// </summary>
		/// <returns></returns>
		public string GetPipelineOutputDirectory()
		{
			if (string.IsNullOrEmpty(m_PipelineOutputDirectory))
			{
				m_PipelineOutputDirectory = $"{Arguments.OutputRoot}/{Arguments.BuildTarget}/{Arguments.PackageName}/{UniverseConstant.OUTPUT_FOLDER_NAME}";
			}
			return m_PipelineOutputDirectory;
		}

		/// <summary>
		/// 获取本次构建的补丁目录
		/// </summary>
		public string GetPackageOutputDirectory()
		{
			if (string.IsNullOrEmpty(m_PackageOutputDirectory))
			{
				m_PackageOutputDirectory = $"{Arguments.OutputRoot}/{Arguments.BuildTarget}/{Arguments.PackageName}/{Arguments.PackageVersion}";
			}
			return m_PackageOutputDirectory;
		}

		/// <summary>
		/// 获取内置构建管线的构建选项
		/// </summary>
		public BuildAssetBundleOptions GetPipelineBuildOptions()
		{
			// For the new build system, unity always need BuildAssetBundleOptions.CollectDependencies and BuildAssetBundleOptions.DeterministicAssetBundle
			// 除非设置ForceRebuildAssetBundle标记，否则会进行增量打包

			if (Arguments.BuildMode == EBuildMode.SimulateBuild)
				throw new("Should never get here !");

			BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
			opt |= BuildAssetBundleOptions.StrictMode; //Do not allow the build to succeed if any errors are reporting during it.

			if (Arguments.BuildMode == EBuildMode.DryRunBuild)
			{
				opt |= BuildAssetBundleOptions.DryRunBuild;
				return opt;
			}

			if (Arguments.CompressOption == ECompressOption.Uncompressed)
				opt |= BuildAssetBundleOptions.UncompressedAssetBundle;
			else if (Arguments.CompressOption == ECompressOption.LZ4)
				opt |= BuildAssetBundleOptions.ChunkBasedCompression;

			if (Arguments.BuildMode == EBuildMode.ForceRebuild)
				opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle; //Force rebuild the asset bundles
			if (Arguments.DisableWriteTypeTree)
				opt |= BuildAssetBundleOptions.DisableWriteTypeTree; //Do not include type information within the asset bundle (don't write type tree).
			if (Arguments.IgnoreTypeTreeChanges)
				opt |= BuildAssetBundleOptions.IgnoreTypeTreeChanges; //Ignore the type tree changes when doing the incremental build check.

			opt |= BuildAssetBundleOptions.DisableLoadAssetByFileName;              //Disables Asset Bundle LoadAsset by file name.
			opt |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension; //Disables Asset Bundle LoadAsset by file name with extension.			

			return opt;
		}

		/// <summary>
		/// 获取可编程构建管线的构建参数
		/// </summary>
		public BundleBuildParameters GetSbpBuildParameters()
		{
			if (Arguments.BuildMode == EBuildMode.SimulateBuild)
			{
				throw new("Should never get here !");
			}

			BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(Arguments.BuildTarget);
			string pipelineOutputDirectory = GetPipelineOutputDirectory();
			BundleBuildParameters buildParams = new(Arguments.BuildTarget, targetGroup, pipelineOutputDirectory);

			buildParams.BundleCompression = Arguments.CompressOption switch
			{
				ECompressOption.Uncompressed => BuildCompression.Uncompressed,
				ECompressOption.LZMA => BuildCompression.LZMA,
				ECompressOption.LZ4 => BuildCompression.LZ4,
				_ => throw new System.NotImplementedException(Arguments.CompressOption.ToString())
			};

			if (Arguments.DisableWriteTypeTree)
			{
				buildParams.ContentBuildFlags |= UnityEditor.Build.Content.ContentBuildFlags.DisableWriteTypeTree;
			}

			buildParams.UseCache = true;
			buildParams.CacheServerHost = Arguments.CacheServerHost;
			buildParams.CacheServerPort = Arguments.CacheServerPort;
			buildParams.WriteLinkXML = Arguments.WriteLinkXML;

			return buildParams;
		}
	}
}
