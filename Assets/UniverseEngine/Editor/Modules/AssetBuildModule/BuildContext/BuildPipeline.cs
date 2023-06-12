using System;
using System.Collections.Generic;
using System.Linq;

namespace UniverseEngine.Editor
{
	public class UniverseBuildPipeline
	{
		readonly List<IBuildTask> m_Tasks = new();
		readonly BuildContext m_BuildContext = new();

		readonly BuildArguments m_Arguments;
		public static double LastBuildSeconds;

		UniverseBuildPipeline(BuildArguments arguments)
		{
			m_Arguments = arguments ??= new();
			m_BuildContext.Initialize(arguments);
		}

		public static UniverseBuildPipeline Start(BuildArguments arguments)
		{
			arguments ??= new();
			return new(arguments);
		}

		public UniverseBuildPipeline Task<TBuildTask>() where TBuildTask : IBuildTask, new()
		{
			TBuildTask task = new();
			AddTask(task);
			return this;
		}

		public UniverseBuildPipeline TaskVariant<TBuiltIn, TSbp>() where TBuiltIn : IBuildTask, new()
		                                                           where TSbp : IBuildTask, new()
		{
			IBuildTask task = m_BuildContext.BuildPipeline switch
			{
				EBuildPipeline.BuiltinBuildPipeline => new TBuiltIn(),
				EBuildPipeline.ScriptableBuildPipeline => new TSbp(),
				_ => throw new ArgumentOutOfRangeException()
			};

			AddTask(task);
			return this;
		}

		public BuildResult Build()
		{
			BuildParametersContext buildParametersContext = new(m_Arguments);
			m_BuildContext.SetContextObject(buildParametersContext);
			BuildResult buildResult = new()
			{
				IsSuccess = true,
				OutputPackageDirectory = buildParametersContext.GetPackageOutputDirectory()
			};

			(bool checkResult, string error) = BuildCheck();
			if (!checkResult)
			{
				buildResult.IsSuccess = false;
				buildResult.ErrorInfo = error;
				return buildResult;
			}

			if (Collections.IsNullOrEmpty(m_Tasks))
			{
				return BuildResult.Unknown;
			}

			double totalSeconds = 0;
			foreach (IBuildTask task in m_Tasks)
			{
				try
				{
					ValueStopwatch buildWatch = ValueStopwatch.StartNew();
					Log<AssetBuildModule>.Info($"---------------------------------------->{task.GetDisplayName()}<---------------------------------------");
					task.Run(m_BuildContext);

					// 统计耗时
					double seconds = buildWatch.TotalSeconds;
					totalSeconds += seconds;
					Log<AssetBuildModule>.Info($"{task.GetDisplayName()}耗时：{seconds}秒");
				}
				catch (Exception e)
				{
					buildResult.FailedTask = task.GetType().Name;
					buildResult.ErrorInfo = e.ToString();
					buildResult.IsSuccess = false;
					break;
				}
			}

			// 返回运行结果
			Log<AssetBuildModule>.Info($"构建过程总计耗时：{totalSeconds}秒");
			LastBuildSeconds = totalSeconds;
			return buildResult;
		}

		void AddTask(IBuildTask task)
		{
			if (task.IgnoreBuildModes.Contains(m_Arguments.BuildMode))
			{
				return;
			}

			m_Tasks.Add(task);
		}

		(bool result, string error) BuildCheck()
		{
			if (m_Arguments.BuildPipeline != EBuildPipeline.ScriptableBuildPipeline)
			{
				return (true, string.Empty);
			}

			string error = m_Arguments.BuildMode switch
			{
				EBuildMode.DryRunBuild => $"{nameof(EBuildPipeline.ScriptableBuildPipeline)} not support {nameof(EBuildMode.DryRunBuild)} build mode !",
				EBuildMode.ForceRebuild => $"{nameof(EBuildPipeline.ScriptableBuildPipeline)} not support {nameof(EBuildMode.ForceRebuild)} build mode !",
				_ => string.Empty
			};

			if (!string.IsNullOrEmpty(error))
			{
				return (false, error);
			}

			return (true, string.Empty);
		}
	}
}
