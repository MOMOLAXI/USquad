using System.Collections.Generic;

namespace UniverseEngine
{
	public static class OperationSystem
	{
		private static readonly List<AsyncOperationBase> s_Operations = new(100);
		private static readonly List<AsyncOperationBase> s_AddList = new(100);
		private static readonly List<AsyncOperationBase> s_RemoveList = new(100);

		// 计时器相关
		private static ValueStopwatch s_Watch;
		private static long s_FrameTime;

		/// <summary>
		/// 异步操作的最小时间片段
		/// </summary>
		public static long MaxTimeSlice { set; get; } = long.MaxValue;

		/// <summary>
		/// 处理器是否繁忙
		/// </summary>
		public static bool IsBusy => s_Watch.Elapsed.TotalMilliseconds - s_FrameTime >= MaxTimeSlice;

		/// <summary>
		/// 初始化异步操作系统
		/// </summary>
		public static void Initialize()
		{
			s_Watch = ValueStopwatch.StartNew();
		}

		/// <summary>
		/// 更新异步操作系统
		/// </summary>
		public static void Update()
		{
			s_FrameTime = s_Watch.ElapsedTicks;

			// 添加新的异步操作
			if (s_AddList.Count > 0)
			{
				for (int i = 0; i < s_AddList.Count; i++)
				{
					AsyncOperationBase operation = s_AddList[i];
					s_Operations.Add(operation);
				}
				s_AddList.Clear();
			}

			// 更新所有的异步操作
			foreach (AsyncOperationBase operation in s_Operations)
			{
				if (IsBusy)
					break;

				operation.Update();
				if (operation.IsDone)
				{
					s_RemoveList.Add(operation);
					operation.Finish();
				}
			}

			// 移除已经完成的异步操作
			if (s_RemoveList.Count > 0)
			{
				foreach (AsyncOperationBase operation in s_RemoveList)
				{
					s_Operations.Remove(operation);
				}
				s_RemoveList.Clear();
			}
		}

		/// <summary>
		/// 销毁异步操作系统
		/// </summary>
		public static void DestroyAll()
		{
			s_Operations.Clear();
			s_AddList.Clear();
			s_RemoveList.Clear();
			s_Watch = ValueStopwatch.StartNew();
			s_FrameTime = 0;
			MaxTimeSlice = long.MaxValue;
		}

		/// <summary>
		/// 开始处理异步操作类
		/// </summary>
		public static void StartOperation(AsyncOperationBase operation)
		{
			s_AddList.Add(operation);
			operation.Start();
		}
	}
}