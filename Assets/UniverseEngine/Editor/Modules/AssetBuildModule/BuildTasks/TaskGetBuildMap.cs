using System;
using System.Linq;
using System.Collections.Generic;

namespace UniverseEngine.Editor
{
	public class TaskCreateBuildMap : IBuildTask
	{
		public string GetDisplayName() => "创建资源构建内容";

		public EBuildMode[] IgnoreBuildModes => Array.Empty<EBuildMode>();

		void IBuildTask.Run(BuildContext context)
		{
			BuildParametersContext buildParametersContext = context.GetContextObject<BuildParametersContext>();
			BuildArguments buildArguments = buildParametersContext.Arguments;
			BuildMapContext buildMapContext = CreateBuildMap(buildArguments.BuildMode, buildArguments.ShareAssetPackRule, buildArguments.PackageName);
			context.SetContextObject(buildMapContext);
			Log<AssetBuildModule>.Info("构建内容准备完毕！");

			// 检测构建结果
			CheckBuildMapContent(buildMapContext);
		}

		/// <summary>
		/// 资源构建上下文
		/// </summary>
		static BuildMapContext CreateBuildMap(EBuildMode buildMode, EShareAssetPackRule packRule, string packageName)
		{
			Dictionary<string, BuildAssetInfo> allBuildAssetInfo = new(1000);

			// 1. 检测配置合法性
			AssetCollectModule.Setting.CheckConfigError();

			// 2. 获取所有收集器收集的资源
			CollectResult collectResult = AssetCollectModule.Setting.GetPackageAssets(buildMode, packageName);
			List<CollectAssetInfo> allCollectAssetInfos = collectResult.CollectAssets;

			// 3. 剔除未被引用的依赖项资源
			RemoveZeroReferenceAssets(allCollectAssetInfos);

			// 4. 录入所有收集器收集的资源
			foreach (CollectAssetInfo collectAssetInfo in allCollectAssetInfos)
			{
				if (!allBuildAssetInfo.ContainsKey(collectAssetInfo.AssetPath))
				{
					BuildAssetInfo buildAssetInfo = new(collectAssetInfo);
					buildAssetInfo.AddAssetTags(collectAssetInfo.AssetTags);
					buildAssetInfo.AddBundleTags(collectAssetInfo.AssetTags);
					allBuildAssetInfo.Add(collectAssetInfo.AssetPath, buildAssetInfo);
				}
				else
				{
					throw new("Should never get here !");
				}
			}

			// 5. 录入所有收集资源的依赖资源
			foreach (CollectAssetInfo collectAssetInfo in allCollectAssetInfos)
			{
				string collectAssetBundleName = collectAssetInfo.BundleName;
				foreach (string dependAssetPath in collectAssetInfo.DependAssets)
				{
					if (allBuildAssetInfo.ContainsKey(dependAssetPath))
					{
						allBuildAssetInfo[dependAssetPath].AddBundleTags(collectAssetInfo.AssetTags);
						allBuildAssetInfo[dependAssetPath].AddReferenceBundleName(collectAssetBundleName);
					}
					else
					{
						BuildAssetInfo buildAssetInfo = new(dependAssetPath);
						buildAssetInfo.AddBundleTags(collectAssetInfo.AssetTags);
						buildAssetInfo.AddReferenceBundleName(collectAssetBundleName);
						allBuildAssetInfo.Add(dependAssetPath, buildAssetInfo);
					}
				}
			}

			// 6. 填充所有收集资源的依赖列表
			foreach (CollectAssetInfo collectAssetInfo in allCollectAssetInfos)
			{
				List<BuildAssetInfo> dependAssetInfos = new(collectAssetInfo.DependAssets.Count);
				foreach (string dependAssetPath in collectAssetInfo.DependAssets)
				{
					if (allBuildAssetInfo.TryGetValue(dependAssetPath, out BuildAssetInfo value))
					{
						dependAssetInfos.Add(value);
					}
					else
					{
						throw new("Should never get here !");
					}
				}

				allBuildAssetInfo[collectAssetInfo.AssetPath].SetAllDependAssetInfos(dependAssetInfos);
			}

			// 7. 记录关键信息
			BuildMapContext context = new()
			{
				AssetFileCount = allBuildAssetInfo.Count,
				EnableAddressable = collectResult.Command.EnableAddressable,
				UniqueBundleName = collectResult.Command.UniqueBundleName,
				ShadersBundleName = collectResult.Command.ShadersBundleName
			};

			// 8. 计算共享的资源包名
			CollectCommand command = collectResult.Command;
			foreach (BuildAssetInfo buildAssetInfo in allBuildAssetInfo.Values)
			{
				IShareAssetPackRule rule = AssetCollectModule.GetShareAssetPackRule(packRule);
				buildAssetInfo.CalculateShareBundleName(rule, command.UniqueBundleName, command.PackageName, command.ShadersBundleName);
			}

			// 9. 移除不参与构建的资源
			List<BuildAssetInfo> removeBuildList = new();
			foreach (BuildAssetInfo buildAssetInfo in allBuildAssetInfo.Values)
			{
				if (!buildAssetInfo.HasBundleName())
				{
					removeBuildList.Add(buildAssetInfo);
				}
			}
			
			foreach (BuildAssetInfo removeValue in removeBuildList)
			{
				allBuildAssetInfo.Remove(removeValue.AssetPath);
			}

			// 10. 构建资源包
			List<BuildAssetInfo> allPackAssets = allBuildAssetInfo.Values.ToList();
			if (allPackAssets.Count == 0)
			{
				throw new("构建的资源列表为空");
			}

			foreach (BuildAssetInfo assetInfo in allPackAssets)
			{
				context.PackAsset(assetInfo);
			}
			
			return context;
		}

		static void RemoveZeroReferenceAssets(List<CollectAssetInfo> allCollectAssetInfos)
		{
			// 1. 检测是否任何存在依赖资源
			bool hasAnyDependAsset = false;
			foreach (CollectAssetInfo collectAssetInfo in allCollectAssetInfos)
			{
				ECollectorType collectorType = collectAssetInfo.CollectorType;
				if (collectorType == ECollectorType.DependAssetCollector)
				{
					hasAnyDependAsset = true;
					break;
				}
			}
			if (!hasAnyDependAsset)
			{
				return;
			}

			// 2. 获取所有主资源的依赖资源集合
			HashSet<string> allDependAsset = new();
			foreach (CollectAssetInfo collectAssetInfo in allCollectAssetInfos)
			{
				ECollectorType collectorType = collectAssetInfo.CollectorType;
				if (collectorType is ECollectorType.MainAssetCollector or ECollectorType.StaticAssetCollector)
				{
					foreach (string dependAsset in collectAssetInfo.DependAssets)
					{
						allDependAsset.Add(dependAsset);
					}
				}
			}

			// 3. 找出所有零引用的依赖资源集合
			List<CollectAssetInfo> removeList = new();
			foreach (CollectAssetInfo collectAssetInfo in allCollectAssetInfos)
			{
				ECollectorType collectorType = collectAssetInfo.CollectorType;
				if (collectorType == ECollectorType.DependAssetCollector)
				{
					if (!allDependAsset.Contains(collectAssetInfo.AssetPath))
					{
						removeList.Add(collectAssetInfo);
					}
				}
			}

			// 4. 移除所有零引用的依赖资源
			foreach (CollectAssetInfo removeValue in removeList)
			{
				Log<AssetBuildModule>.Info($"发现未被依赖的资源并自动移除 : {removeValue.AssetPath}");
				allCollectAssetInfos.Remove(removeValue);
			}
		}

		/// <summary>
		/// 检测构建结果
		/// </summary>
		static void CheckBuildMapContent(BuildMapContext buildMapContext)
		{
			foreach (BuildBundleInfo bundleInfo in buildMapContext.BundleCollection)
			{
				// 注意：原生文件资源包只能包含一个原生文件
				bool isRawFile = bundleInfo.IsRawFile;
				if (isRawFile)
				{
					if (bundleInfo.AllMainAssets.Count != 1)
					{
						throw new($"The bundle does not support multiple raw asset : {bundleInfo.BundleName}");
					}
					continue;
				}

				// 注意：原生文件不能被其它资源文件依赖
				foreach (BuildAssetInfo assetInfo in bundleInfo.AllMainAssets)
				{
					if (assetInfo.AllDependAssetInfos == null)
					{
						continue;
					}

					foreach (BuildAssetInfo dependAssetInfo in assetInfo.AllDependAssetInfos)
					{
						if (dependAssetInfo.IsRawAsset)
						{
							throw new($"{assetInfo.AssetPath} can not depend raw asset : {dependAssetInfo.AssetPath}");
						}
					}
				}
			}
		}
	}
}
