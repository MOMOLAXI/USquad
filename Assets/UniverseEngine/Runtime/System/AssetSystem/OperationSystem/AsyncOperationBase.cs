using System;
using System.Collections;

namespace UniverseEngine
{
	public abstract class AsyncOperationBase : IEnumerator
	{
		Action<AsyncOperationBase> m_Callback;

		public EOperationStatus Status { get; protected set; } = EOperationStatus.None;

		public string Error { get; protected set; }

		public float Progress { get; protected set; }

		public bool IsDone => Status is EOperationStatus.Failed or EOperationStatus.Succeed;

		public event Action<AsyncOperationBase> Completed
		{
			add
			{
				if (IsDone)
					value.Invoke(this);
				else
					m_Callback += value;
			}
			remove => m_Callback -= value;
		}

		internal abstract void Start();
		internal abstract void Update();
		internal void Finish()
		{
			Progress = 1f;
			m_Callback?.Invoke(this);
		}

		/// <summary>
		/// 清空完成回调
		/// </summary>
		protected void ClearCompletedCallback()
		{
			m_Callback = null;
		}

		bool IEnumerator.MoveNext()
		{
			return !IsDone;
		}
		void IEnumerator.Reset()
		{
		}
		
		object IEnumerator.Current => null;
	}
}
