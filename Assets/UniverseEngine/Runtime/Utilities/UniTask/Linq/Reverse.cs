﻿using System.Threading;

namespace UniverseEngine
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static IUniTaskAsyncEnumerable<TSource> Reverse<TSource>(this IUniTaskAsyncEnumerable<TSource> source)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            return new Reverse<TSource>(source);
        }
    }

    sealed internal class Reverse<TSource> : IUniTaskAsyncEnumerable<TSource>
    {
        readonly IUniTaskAsyncEnumerable<TSource> source;

        public Reverse(IUniTaskAsyncEnumerable<TSource> source)
        {
            this.source = source;
        }

        public IUniTaskAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken))
        {
            return new _Reverse(source, cancellationToken);
        }

        sealed class _Reverse : MoveNextSource, IUniTaskAsyncEnumerator<TSource>
        {
            readonly IUniTaskAsyncEnumerable<TSource> source;
            CancellationToken cancellationToken;

            TSource[] array;
            int index;

            public _Reverse(IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
            {
                this.source = source;
                this.cancellationToken = cancellationToken;
                TaskTracker.TrackActiveTask(this, 3);
            }

            public TSource Current { get; private set; }

            // after consumed array, don't use await so allow async(not require UniTaskCompletionSourceCore).
            public async UniTask<bool> MoveNextAsync()
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (array == null)
                {
                    array = await source.ToArrayAsync(cancellationToken);
                    index = array.Length - 1;
                }

                if (index != -1)
                {
                    Current = array[index];
                    --index;
                    return true;
                }
                return false;
            }

            public UniTask DisposeAsync()
            {
                TaskTracker.RemoveTracking(this);
                return default;
            }
        }
    }
}