using System.Threading;

namespace UniverseEngine
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static IUniTaskAsyncEnumerable<TSource> Skip<TSource>(this IUniTaskAsyncEnumerable<TSource> source, int count)
        {
            Error.ThrowArgumentNullException(source, nameof(source));

            return new Skip<TSource>(source, count);
        }
    }

    sealed internal class Skip<TSource> : IUniTaskAsyncEnumerable<TSource>
    {
        readonly IUniTaskAsyncEnumerable<TSource> source;
        readonly int count;

        public Skip(IUniTaskAsyncEnumerable<TSource> source, int count)
        {
            this.source = source;
            this.count = count;
        }

        public IUniTaskAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken))
        {
            return new _Skip(source, count, cancellationToken);
        }

        sealed class _Skip : AsyncEnumeratorBase<TSource, TSource>
        {
            readonly int count;

            int index;

            public _Skip(IUniTaskAsyncEnumerable<TSource> source, int count, CancellationToken cancellationToken)
                : base(source, cancellationToken)
            {
                this.count = count;
            }

            protected override bool TryMoveNextCore(bool sourceHasCurrent, out bool result)
            {
                if (sourceHasCurrent)
                {
                    if (count <= checked(index++))
                    {
                        Current = SourceCurrent;
                        result = true;
                        return true;
                    }
                    result = default;
                    return false;
                }
                result = false;
                return true;
            }
        }
    }
}