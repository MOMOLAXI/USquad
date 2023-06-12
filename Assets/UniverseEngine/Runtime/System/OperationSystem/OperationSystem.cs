// using System.Collections.Generic;
// using System.Diagnostics;
//
// namespace UniverseEngine
// {
//     internal class OperationSystem : EngineSystem
//     {
//         // readonly List<AsyncOperationBase> m_Operations = new();
//         // readonly Queue<AsyncOperationBase> m_Enter = new();
//         // readonly Queue<AsyncOperationBase> m_Exit = new();
//         //
//         // // 计时器相关
//         // long m_FrameTime;
//         // Stopwatch m_Watcher;
//         //
//         // /// <summary>
//         // /// 异步操作的最小时间片段
//         // /// </summary>
//         // public long MaxSliceTimeMs { set; get; } = 30;
//         //
//         // /// <summary>
//         // /// 处理器是否繁忙
//         // /// </summary>
//         // public bool IsBusy => m_Watcher.ElapsedMilliseconds - m_FrameTime >= MaxSliceTimeMs;
//         //
//         // /// <summary>
//         // /// 启动一个异步操作
//         // /// </summary>
//         // public void StartOperation(AsyncOperationBase operation)
//         // {
//         //     m_Enter.Enqueue(operation);
//         //     operation.Start();
//         // }
//         //
//         // public override void OnInit()
//         // {
//         //     m_Watcher = Stopwatch.StartNew();
//         // }
//         //
//         // /// <summary>
//         // /// 更新异步操作系统
//         // /// </summary>
//         // public override void OnUpdate(float dt)
//         // {
//         //     m_FrameTime = m_Watcher.ElapsedMilliseconds;
//         //
//         //     // 添加新的异步操作
//         //     while (m_Enter.Count > 0)
//         //     {
//         //         AsyncOperationBase operation = m_Enter.Dequeue();
//         //         m_Operations.Add(operation);
//         //     }
//         //
//         //     // 更新所有的异步操作
//         //     if (IsBusy)
//         //     {
//         //         return;
//         //     }
//         //
//         //     for (int i = 0; i < m_Operations.Count; i++)
//         //     {
//         //         AsyncOperationBase operation = m_Operations[i];
//         //         operation.Update();
//         //         if (operation.IsDone)
//         //         {
//         //             m_Exit.Enqueue(operation);
//         //             operation.Finish();
//         //         }
//         //     }
//         //
//         //     // 移除已经完成的异步操作
//         //     while (m_Exit.Count > 0)
//         //     {
//         //         AsyncOperationBase operation = m_Exit.Dequeue();
//         //         m_Operations.Remove(operation);
//         //     }
//         // }
//         //
//         // /// <summary>
//         // /// 销毁异步操作系统
//         // /// </summary>
//         // public override void OnDestroy()
//         // {
//         //     m_Operations.Clear();
//         //     m_Enter.Clear();
//         //     m_Exit.Clear();
//         //     m_Watcher = null;
//         //     m_FrameTime = 0;
//         //     MaxSliceTimeMs = long.MaxValue;
//         // }
//     }
// }
