// using System;
// using System.Collections;
// using System.Threading.Tasks;
//
// namespace UniverseEngine
// {
//     public abstract class AsyncOperationBase : IEnumerator
//     {
//         Action<AsyncOperationBase> m_Callback;
//         
//         protected UProgress<float> Progress;
//
//         /// <summary>
//         /// 状态
//         /// </summary>
//         public EOperationStatus Status { get; protected set; } = EOperationStatus.None;
//
//         /// <summary>
//         /// 错误信息
//         /// </summary>
//         public string Error { get; protected set; }
//
//         /// <summary>
//         /// 处理进度
//         /// </summary>
//         public float ProgressValue
//         {
//             get => Progress.Value;
//             protected set => Progress.Value = value;
//         }
//
//         /// <summary>
//         /// 是否已经完成
//         /// </summary>
//         public bool IsDone => Status is EOperationStatus.Failed or EOperationStatus.Succeed;
//
//         protected AsyncOperationBase(UProgress<float> progress)
//         {
//             Progress = progress;
//         }
//
//         /// <summary>
//         /// 完成事件
//         /// </summary>
//         public event Action<AsyncOperationBase> Completed
//         {
//             add
//             {
//                 if (IsDone)
//                 {
//                     value.Invoke(this);
//                 }
//                 else
//                 {
//                     m_Callback += value;
//                 }
//             }
//             remove => m_Callback -= value;
//         }
//
//         /// <summary>
//         /// 异步操作任务
//         /// </summary>
//         public Task Task
//         {
//             get
//             {
//                 if (m_TaskCompletionSource == null)
//                 {
//                     m_TaskCompletionSource = new();
//                     if (IsDone)
//                     {
//                         m_TaskCompletionSource.SetResult(null);
//                     }
//                 }
//                 return m_TaskCompletionSource.Task;
//             }
//         }
//
//         internal abstract void Start();
//         internal abstract void Update();
//         internal void Finish()
//         {
//             ProgressValue = 1f;
//             m_Callback?.Invoke(this);
//             m_TaskCompletionSource?.TrySetResult(null);
//         }
//
//         /// <summary>
//         /// 清空完成回调
//         /// </summary>
//         protected void ClearCompletedCallback()
//         {
//             m_Callback = null;
//         }
//
//     #region 异步编程相关
//
//         bool IEnumerator.MoveNext()
//         {
//             return !IsDone;
//         }
//         void IEnumerator.Reset()
//         {
//         }
//         object IEnumerator.Current => null;
//
//         private TaskCompletionSource<object> m_TaskCompletionSource;
//
//     #endregion
//
//     }
// }
