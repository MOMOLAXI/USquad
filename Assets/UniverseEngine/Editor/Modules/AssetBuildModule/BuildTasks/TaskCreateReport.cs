using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;

namespace UniverseEngine.Editor
{
	public class TaskCreateReport : IBuildTask
	{
		public string GetDisplayName() => "创建构建报告文件";

		public EBuildMode[] IgnoreBuildModes => new[] { EBuildMode.SimulateBuild };
		void IBuildTask.Run(BuildContext context)
		{
			BuildParametersContext buildParameters = context.GetContextObject<BuildParametersContext>();
			BuildMapContext buildMapContext = context.GetContextObject<BuildMapContext>();
			ManifestContext manifestContext = context.GetContextObject<ManifestContext>();
			CreateReportFile(buildParameters, buildMapContext, manifestContext);
		}

		private void CreateReportFile(BuildParametersContext buildParametersContext, BuildMapContext buildMapContext, ManifestContext manifestContext)
		{
			BuildArguments buildArguments = buildParametersContext.Arguments;

			string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
			PackageManifest manifest = manifestContext.Manifest;
			BuildReport buildReport = new();

			// 概述信息
			{
				UnityEditor.PackageManager.PackageInfo packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(BuildReport).Assembly);
				if (packageInfo != null)
				{
					buildReport.Summary.AssetSystemVersion = packageInfo.version;
				}

				buildReport.Summary.UnityVersion = UnityEngine.Application.unityVersion;
				buildReport.Summary.BuildDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
				buildReport.Summary.BuildSeconds = UniverseBuildPipeline.LastBuildSeconds;
				buildReport.Summary.BuildTarget = buildArguments.BuildTarget;
				buildReport.Summary.BuildPipeline = buildArguments.BuildPipeline;
				buildReport.Summary.BuildMode = buildArguments.BuildMode;
				buildReport.Summary.BuildPackageName = buildArguments.PackageName;
				buildReport.Summary.BuildPackageVersion = buildArguments.PackageVersion;
				buildReport.Summary.EnableAddressable = buildMapContext.EnableAddressable;
				buildReport.Summary.UniqueBundleName = buildMapContext.UniqueBundleName;
				buildReport.Summary.EncryptionServicesClassName = buildArguments.EncryptionType.ToString();

				// 构建参数
				buildReport.Summary.OutputNameStyle = buildArguments.OutputNameStyle;
				buildReport.Summary.CompressOption = buildArguments.CompressOption;
				buildReport.Summary.DisableWriteTypeTree = buildArguments.DisableWriteTypeTree;
				buildReport.Summary.IgnoreTypeTreeChanges = buildArguments.IgnoreTypeTreeChanges;

				// 构建结果
				buildReport.Summary.AssetFileTotalCount = buildMapContext.AssetFileCount;
				buildReport.Summary.MainAssetTotalCount = manifest.MainAssetCount;
				buildReport.Summary.AllBundleTotalCount = manifest.BundleCount;
				buildReport.Summary.AllBundleTotalSize = manifest.BundleTotalSize;
				buildReport.Summary.EncryptedBundleTotalCount = manifest.EncryptedBundleCount;
				buildReport.Summary.EncryptedBundleTotalSize = manifest.EncryptedBundleSize;
				buildReport.Summary.RawBundleTotalCount = manifest.RawBundleCount;
				buildReport.Summary.RawBundleTotalSize = manifest.RawBundleSize;
			}

			// 资源对象列表
			buildReport.AssetInfos = new();
			manifest.ForeachAssets(asset =>
			{
				PackageBundle mainBundle = manifest.GetBundleById(asset.BundleID);
				ReportAssetInfo reportAssetInfo = new()
				{
					Address = asset.Address,
					AssetPath = asset.AssetPath,
					AssetTags = asset.AssetTags,
					AssetGuid = AssetDatabase.AssetPathToGUID(asset.AssetPath),
					MainBundleName = mainBundle.BundleName,
					MainBundleSize = mainBundle.FileSize,
					DependBundles = GetDependBundles(manifest, asset),
					DependAssets = GetDependAssets(buildMapContext, mainBundle.BundleName, asset.AssetPath)
				};
				buildReport.AssetInfos.Add(reportAssetInfo);
			});

			// 资源包列表
			buildReport.BundleInfos = new();
			manifest.ForeachBundles(bundle =>
			{
				ReportBundleInfo reportBundleInfo = new()
				{
					BundleName = bundle.BundleName,
					FileName = bundle.FileName,
					FileHash = bundle.FileHash,
					FileCRC = bundle.FileCRC,
					FileSize = bundle.FileSize,
					IsRawFile = bundle.IsRawFile,
					LoadMethod = (EBundleLoadMethod)bundle.LoadMethod,
					Tags = bundle.Tags,
					ReferenceIDs = bundle.ReferenceIDs,
				};

				GetAllBuiltinAssets(buildMapContext, bundle.BundleName, reportBundleInfo.AllBuiltinAssets);
				buildReport.BundleInfos.Add(reportBundleInfo);
			});

			// 序列化文件
			string fileName = FileSystem.GetReportFileName(buildArguments.PackageName, buildArguments.PackageVersion);
			string filePath = $"{packageOutputDirectory}/{fileName}";
			FileSystem.SaveToJson(buildReport, filePath);
			Log<AssetBuildModule>.Info($"资源构建报告文件创建完成：{filePath}");
		}

		/// <summary>
		/// 获取资源对象依赖的所有资源包
		/// </summary>
		private List<string> GetDependBundles(PackageManifest manifest, Asset asset)
		{
			List<string> dependBundles = new(asset.DependIDs.Count);
			for (int i = 0; i < asset.DependIDs.Count; i++)
			{
				int index = asset.DependIDs[i];
				PackageBundle bundle = manifest.GetBundleById(index);
				string dependBundleName = bundle.BundleName;
				dependBundles.Add(dependBundleName);
			}

			return dependBundles;
		}

		/// <summary>
		/// 获取资源对象依赖的其它所有资源
		/// </summary>
		static List<string> GetDependAssets(BuildMapContext buildMapContext, string bundleName, string assetPath)
		{
			List<string> result = new();
			BuildBundleInfo bundleInfo = buildMapContext.GetBundleInfo(bundleName);
			{
				BuildAssetInfo findAssetInfo = null;
				for (int i = 0; i < bundleInfo.AllMainAssets.Count; i++)
				{
					BuildAssetInfo assetInfo = bundleInfo.AllMainAssets[i];
					if (assetInfo.AssetPath == assetPath)
					{
						findAssetInfo = assetInfo;
						break;
					}
				}

				if (findAssetInfo == null)
				{
					throw new($"Not found asset {assetPath} in bundle {bundleName}");
				}

				for (int i = 0; i < findAssetInfo.AllDependAssetInfos.Count; i++)
				{
					BuildAssetInfo dependAssetInfo = findAssetInfo.AllDependAssetInfos[i];
					result.Add(dependAssetInfo.AssetPath);
				}
			}
			return result;
		}

		/// <summary>
		/// 获取该资源包内的所有资源（包括零依赖资源）
		/// </summary>
		static void GetAllBuiltinAssets(BuildMapContext buildMapContext, string bundleName, List<string> result)
		{
			BuildBundleInfo bundleInfo = buildMapContext.GetBundleInfo(bundleName);
			bundleInfo.GetAllBuiltinAssetPaths(result);
		}
	}
}
