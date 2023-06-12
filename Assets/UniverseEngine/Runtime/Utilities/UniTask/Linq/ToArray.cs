using System;
using System.Threading;

namespace UniverseEngine
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static UniTask<TSource[]> ToArrayAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            Error.ThrowArgumentNullException(source, nameof(source));

            return ToArray.ToArrayAsync(source, cancellationToken);
        }
    }

    internal static class ToArray
    {
        internal static async UniTask<TSource[]> ToArrayAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            var pool = ArrayPool<TSource>.Shared;
            var array = pool.Rent(16);

            TSource[] result = default(TSource[]);
            IUniTaskAsyncEnumerator<TSource> e = default(IUniTaskAsyncEnumerator<TSource>);
            try
            {
                e = source.GetAsyncEnumerator(cancellationToken);
                var i = 0;
                while (await e.MoveNextAsync())
                {
                    ArrayPoolUtil.EnsureCapacity(ref array, i, pool);
                    array[i++] = e.Current;
                }

                if (i == 0)
                {
                    result = Array.Empty<TSource>();
                }
                else
                {
                    result = new TSource[i];
                    Array.Copy(array, result, i);
                }
            }
            finally
            {
                pool.Return(array, clearArray: !RuntimeHelpersAbstraction.IsWellKnownNoReferenceContainsType<TSource>());

                if (e != null)
                {
                    await e.DisposeAsync();
                }
            }

            return result;
        }
    }
}