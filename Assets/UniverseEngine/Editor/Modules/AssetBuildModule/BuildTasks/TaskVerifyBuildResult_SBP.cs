using System.Linq;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline.Interfaces;

namespace UniverseEngine.Editor
{
	public class TaskVerifyBuildResult_Sbp : IBuildTask
	{
		public string GetDisplayName() => "验证构建结果";

		public EBuildMode[] IgnoreBuildModes => new[] { EBuildMode.SimulateBuild };

		void IBuildTask.Run(BuildContext context)
		{
			BuildParametersContext buildParametersContext = context.GetContextObject<BuildParametersContext>();

			// 验证构建结果
			if (buildParametersContext.Arguments.VerifyBuildingResult)
			{
				TaskBuilding_Sbp.BuildResultContext buildResultContext = context.GetContextObject<TaskBuilding_Sbp.BuildResultContext>();
				VerifyingBuildingResult(context, buildResultContext.Results);
			}
		}

		/// <summary>
		/// 验证构建结果
		/// </summary>
		static void VerifyingBuildingResult(BuildContext context, IBundleBuildResults buildResults)
		{
			BuildMapContext buildMapContext = context.GetContextObject<BuildMapContext>();
			List<string> unityCreateBundles = buildResults.BundleInfos.Keys.ToList();

			// 1. 过滤掉原生Bundle
			List<string> expectBundles = buildMapContext.BundleCollection.Where(t => t.IsRawFile == false).Select(t => t.BundleName).ToList();

			// 2. 验证Bundle
			List<string> exceptBundleList1 = unityCreateBundles.Except(expectBundles).ToList();
			if (exceptBundleList1.Count > 0)
			{
				foreach (string exceptBundle in exceptBundleList1)
				{
					Log<AssetBuildModule>.Warning($"差异资源包: {exceptBundle}");
				}

				throw new("存在差异资源包！请查看警告信息！");
			}

			// 3. 验证Bundle
			List<string> exceptBundleList2 = expectBundles.Except(unityCreateBundles).ToList();
			if (exceptBundleList2.Count > 0)
			{
				foreach (string exceptBundle in exceptBundleList2)
				{
					Log<AssetBuildModule>.Warning($"差异资源包: {exceptBundle}");
				}

				throw new("存在差异资源包！请查看警告信息！");
			}

			Log<AssetBuildModule>.Info("构建结果验证成功！");
		}
	}
}
