using System;
using System.Collections.Generic;

namespace UniverseEngine
{
    /// <summary>
    /// ProgressFactory
    /// </summary>
    internal static class Progress<T>
    {
        static readonly PolyObjectPool<UniverseProgress<T>> s_ProgressPool = new();

        internal static UniverseProgress<T> Create(string name, Action<string, T> handler, IEqualityComparer<T> comparer = null)
        {
            UniverseProgress<T> progress = s_ProgressPool.Get<UniverseProgress<T>>();
            progress.ProgressName = name;
            progress.OnProgress2 = handler;
            progress.Comparer = comparer;
            return progress;
        }

        internal static UniverseProgress<T> Create(Action<T> handler, IEqualityComparer<T> comparer = null)
        {
            UniverseProgress<T> progress = s_ProgressPool.Get<UniverseProgress<T>>();
            progress.ProgressName = string.Empty;
            progress.OnProgress1 = handler;
            progress.Comparer = comparer;
            return progress;
        }

        internal static void Release(UniverseProgress<T> progress)
        {
            if (progress == null)
            {
                Log.Error("Progress to release is invalid");
                return;
            }

            s_ProgressPool.Release(progress);
        }
    }
}
