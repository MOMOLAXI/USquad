using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Build.Pipeline;

namespace UniverseEngine.Editor
{
	public class ManifestContext : IContextObject
	{
		internal PackageManifest Manifest;
	}

	public class TaskCreateManifest : IBuildTask
	{
		public string GetDisplayName() => "创建清单文件";

		public EBuildMode[] IgnoreBuildModes => Array.Empty<EBuildMode>();

		void IBuildTask.Run(BuildContext context)
		{
			CreateManifestFile(context);
		}

		/// <summary>
		/// 创建补丁清单文件到输出目录
		/// </summary>
		void CreateManifestFile(BuildContext context)
		{
			BuildMapContext buildMapContext = context.GetContextObject<BuildMapContext>();
			BuildParametersContext buildParametersContext = context.GetContextObject<BuildParametersContext>();
			BuildArguments buildArguments = buildParametersContext.Arguments;
			string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();

			// 创建新补丁清单
			PackageManifest manifest = new()
			{
				FileVersion = UniverseConstant.PATCH_MANIFEST_FILE_VERSION,
				EnableAddressable = buildMapContext.EnableAddressable,
				OutputNameStyle = buildArguments.OutputNameStyle,
				PackageName = buildArguments.PackageName,
				PackageVersion = buildArguments.PackageVersion,
			};

			manifest.SetBundles(GetAllPackageBundle(context));
			manifest.SetAssets(GetAllPackageAsset(context, manifest));

			switch (buildArguments.BuildPipeline)
			{
				//更新Unity内置资源包的引用关系, 更新资源包之间的引用关系
				case EBuildPipeline.ScriptableBuildPipeline:
				{
					if (buildArguments.BuildMode == EBuildMode.IncrementalBuild)
					{
						TaskBuilding_Sbp.BuildResultContext buildResultContext = context.GetContextObject<TaskBuilding_Sbp.BuildResultContext>();
						UpdateScriptPipelineReference(manifest, buildResultContext);
						UpdateBuiltInShaderReference(manifest, buildResultContext, buildMapContext.ShadersBundleName);
					}
					break;
				}
				// 更新资源包之间的引用关系
				case EBuildPipeline.BuiltinBuildPipeline:
				{
					if (buildArguments.BuildMode != EBuildMode.SimulateBuild)
					{
						TaskBuilding.BuildResultContext buildResultContext = context.GetContextObject<TaskBuilding.BuildResultContext>();
						UpdateBuiltinPipelineReference(manifest, buildResultContext);
					}
					break;
				}
				default:
				{
					throw new ArgumentOutOfRangeException();
				}
			}

			// 创建补丁清单文本文件
			{
				string fileName = FileSystem.GetManifestJsonFileName(buildArguments.PackageName, buildArguments.PackageVersion);
				string filePath = $"{packageOutputDirectory}/{fileName}";
				PackageManifest.SerializeToJson(manifest, filePath);
				Log<AssetBuildModule>.Info($"创建补丁清单文件：{filePath}");
			}

			// 创建补丁清单二进制文件
			string packageHash;
			{
				string fileName = FileSystem.GetManifestBinaryFileName(buildArguments.PackageName, buildArguments.PackageVersion);
				string filePath = $"{packageOutputDirectory}/{fileName}";
				PackageManifest.SerializeToBinary(manifest, filePath);
				packageHash = HashUtilities.FileMD5(filePath);
				Log<AssetBuildModule>.Info($"创建补丁清单文件：{filePath}");

				ManifestContext manifestContext = new();
				byte[] bytesData = FileSystem.ReadAllBytes(filePath);
				manifestContext.Manifest = PackageManifest.DeserializeFromBinary(bytesData);
				context.SetContextObject(manifestContext);
			}

			// 创建补丁清单哈希文件
			{
				string fileName = FileSystem.GetPackageHashFileName(buildArguments.PackageName, buildArguments.PackageVersion);
				string filePath = $"{packageOutputDirectory}/{fileName}";
				FileSystem.CreateFile(filePath, packageHash);
				Log<AssetBuildModule>.Info($"创建补丁清单哈希文件：{filePath}");
			}

			// 创建补丁清单版本文件
			{
				string fileName = FileSystem.GetPackageVersionFileName(buildArguments.PackageName);
				string filePath = $"{packageOutputDirectory}/{fileName}";
				FileSystem.CreateFile(filePath, buildArguments.PackageVersion);
				Log<AssetBuildModule>.Info($"创建补丁清单版本文件：{filePath}");
			}
		}

		/// <summary>
		/// 获取资源包列表
		/// </summary>
		static List<PackageBundle> GetAllPackageBundle(BuildContext context)
		{
			BuildMapContext buildMapContext = context.GetContextObject<BuildMapContext>();
			List<PackageBundle> result = new();
			result.AddRange(buildMapContext.BundleCollection.Select(bundleInfo => bundleInfo.CreatePackageBundle()));
			return result;
		}

		/// <summary>
		/// 获取资源列表
		/// </summary>
		static List<Asset> GetAllPackageAsset(BuildContext context, PackageManifest manifest)
		{
			BuildMapContext buildMapContext = context.GetContextObject<BuildMapContext>();
			return buildMapContext.GetBuildAssets(manifest);
		}

		/// <summary>
		/// 更新Unity内置资源包的引用关系
		/// </summary>
		static void UpdateBuiltInShaderReference(PackageManifest manifest, TaskBuilding_Sbp.BuildResultContext buildResultContext, string shadersBunldeName)
		{
			// 获取所有依赖着色器资源包的资源包列表
			List<string> shaderBundleReferenceList = new();
			foreach (KeyValuePair<string, BundleDetails> valuePair in buildResultContext.Results.BundleInfos)
			{
				if (valuePair.Value.Dependencies.Any(t => t == shadersBunldeName))
				{
					shaderBundleReferenceList.Add(valuePair.Key);
				}
			}

			// 注意：没有任何资源依赖着色器
			if (shaderBundleReferenceList.Count == 0)
			{
				return;
			}

			manifest.UpdateBuiltInShaderDependency(shaderBundleReferenceList, shadersBunldeName);
		}

	#region 资源包引用关系相关

		static readonly Dictionary<string, int> s_CachedBundleID = new();
		static readonly Dictionary<string, string[]> s_CachedBundleDepends = new();

		void UpdateScriptPipelineReference(PackageManifest manifest, TaskBuilding_Sbp.BuildResultContext buildResultContext)
		{
			//缓存资源包ID
			s_CachedBundleID.Clear();
			manifest.ForeachBundles(packageBundle =>
			{
				int bundleID = manifest.GetAssetBundleId(packageBundle.BundleName);
				s_CachedBundleID[packageBundle.BundleName] = bundleID;
			});

			// 缓存资源包依赖
			s_CachedBundleDepends.Clear();
			manifest.ForeachBundles(packageBundle =>
			{
				if (packageBundle.IsRawFile)
				{
					s_CachedBundleDepends.Add(packageBundle.BundleName, new string[] { });
					return;
				}

				if (buildResultContext.Results.BundleInfos.ContainsKey(packageBundle.BundleName) == false)
					throw new($"Not found bundle in SBP build results : {packageBundle.BundleName}");

				string[] depends = buildResultContext.Results.BundleInfos[packageBundle.BundleName].Dependencies;
				s_CachedBundleDepends[packageBundle.BundleName] = depends;
			});

			// 计算资源包引用列表
			manifest.ForeachBundles(bundle =>
			{
				bundle.ReferenceIDs = GetBundleRefrenceIDs(manifest, bundle);
			});
		}

		void UpdateBuiltinPipelineReference(PackageManifest manifest, TaskBuilding.BuildResultContext buildResultContext)
		{

			// 缓存资源包ID
			s_CachedBundleID.Clear();
			manifest.ForeachBundles(packageBundle =>
			{
				int bundleID = manifest.GetAssetBundleId(packageBundle.BundleName);
				s_CachedBundleID[packageBundle.BundleName] = bundleID;
			});

			// 缓存资源包依赖
			s_CachedBundleDepends.Clear();
			manifest.ForeachBundles(packageBundle =>
			{
				if (packageBundle.IsRawFile)
				{
					s_CachedBundleDepends.Add(packageBundle.BundleName, new string[] { });
					return;
				}

				string[] depends = buildResultContext.UnityManifest.GetDirectDependencies(packageBundle.BundleName);
				s_CachedBundleDepends[packageBundle.BundleName] = depends;
			});
			// 计算资源包引用列表
			manifest.ForeachBundles(packageBundle =>
			{
				packageBundle.ReferenceIDs = GetBundleRefrenceIDs(manifest, packageBundle);
			});
		}

		static List<int> GetBundleRefrenceIDs(PackageManifest manifest, PackageBundle targetBundle)
		{
			List<string> referenceList = new();
			manifest.ForeachBundles(bundle =>
			{
				string bundleName = bundle.BundleName;
				if (bundleName == targetBundle.BundleName)
				{
					return;
				}

				string[] dependencies = GetCachedBundleDepends(bundleName);
				if (dependencies.Contains(targetBundle.BundleName))
				{
					referenceList.Add(bundleName);
				}
			});

			List<int> result = new();
			foreach (string bundleName in referenceList)
			{
				int bundleID = GetCachedBundleID(bundleName);
				if (!result.Contains(bundleID))
				{
					result.Add(bundleID);
				}
			}

			return result;
		}

		static int GetCachedBundleID(string bundleName)
		{
			if (s_CachedBundleID.TryGetValue(bundleName, out int value))
			{
				return value;
			}

			throw new($"Not found cached bundle ID : {bundleName}");
		}

		static string[] GetCachedBundleDepends(string bundleName)
		{
			if (s_CachedBundleDepends.TryGetValue(bundleName, out string[] value))
			{
				return value;
			}

			throw new($"Not found cached bundle depends : {bundleName}");
		}

	#endregion
	}
}
