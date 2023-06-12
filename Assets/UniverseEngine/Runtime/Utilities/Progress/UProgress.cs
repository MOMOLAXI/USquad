using System;
using System.Diagnostics;

namespace UniverseEngine
{
    public readonly struct UProgress<T> : IDisposable
    {
        readonly UniverseProgress<T> m_Progress;

        public readonly string OriginalName;

        public string ProgressName
        {
            get => m_Progress.ProgressName;
            set => m_Progress.ProgressName = value;
        }

        public T Value
        {
            get => m_Progress.Value;
            set => m_Progress.Value = value;
        }

        public UProgress(string name, Action<string, T> handler) : this()
        {
            OriginalName = name;
            m_Progress = Progress<T>.Create(name, handler);
        }

        public static UProgress<T> Create(string name, Action<string, T> handler)
        {
            UProgress<T> progress = new(name, handler);
            return progress;
        }

        public static UProgress<T> Create(Action<string, T> handler)
        {
            UProgress<T> progress = new(string.Empty, handler);
            return progress;
        }

        public IProgress<T> Get()
        {
            return m_Progress;
        }

        public void Dispose()
        {
            DebugRelease();
            Progress<T>.Release(m_Progress);
        }

        [Conditional("UNIVERSE_DEBUG_PROGRESS")]
        void DebugRelease()
        {
            Log.Info($"Release progress {OriginalName}");
        }
    }
}
