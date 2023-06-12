using System;
using System.Collections.Generic;

namespace UniverseEngine
{
    internal class UniverseProgress<T> : IProgress<T>, ICacheAble
    {
        bool m_IsFirstCall;
        T m_LatestValue;
        T m_CurrentValue;

        /// <summary>
        /// 可手动设置Value触发回调，或者走IProgress的Report触发回调
        /// </summary>
        public T Value
        {
            get => m_CurrentValue;
            set
            {
                m_CurrentValue = value;
                OnProgressChange(value);
            }
        }

        public IEqualityComparer<T> Comparer;

        public string ProgressName { get; set; }

        public Action<T> OnProgress1;
        public Action<string, T> OnProgress2;

        public UniverseProgress()
        {
            m_IsFirstCall = true;
        }

        public void OnProgressChange(T value)
        {
            if (Comparer != null)
            {
                if (m_IsFirstCall)
                {
                    m_IsFirstCall = false;
                }
                else if (Comparer.Equals(value, m_LatestValue))
                {
                    return;
                }

                m_LatestValue = value;
                if (string.IsNullOrEmpty(ProgressName))
                {
                    OnProgress1?.Invoke(value);
                }
                else
                {
                    OnProgress2?.Invoke(ProgressName, value);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(ProgressName))
                {
                    OnProgress1?.Invoke(value);
                }
                else
                {
                    OnProgress2?.Invoke(ProgressName, value);
                }
            }
        }

        public void Report(T value)
        {
            OnProgressChange(value);
        }

        public bool IsInCache { get; set; }
        public void Reset()
        {
        }
    }
}
