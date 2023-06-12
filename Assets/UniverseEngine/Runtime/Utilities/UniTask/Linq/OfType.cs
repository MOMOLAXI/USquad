using System.Threading;

namespace UniverseEngine
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static IUniTaskAsyncEnumerable<TResult> OfType<TResult>(this IUniTaskAsyncEnumerable<object> source)
        {
            Error.ThrowArgumentNullException(source, nameof(source));

            return new OfType<TResult>(source);
        }
    }

    sealed internal class OfType<TResult> : IUniTaskAsyncEnumerable<TResult>
    {
        readonly IUniTaskAsyncEnumerable<object> source;

        public OfType(IUniTaskAsyncEnumerable<object> source)
        {
            this.source = source;
        }

        public IUniTaskAsyncEnumerator<TResult> GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken))
        {
            return new _OfType(source, cancellationToken);
        }

        class _OfType : AsyncEnumeratorBase<object, TResult>
        {
            public _OfType(IUniTaskAsyncEnumerable<object> source, CancellationToken cancellationToken)

                : base(source, cancellationToken)
            {
            }

            protected override bool TryMoveNextCore(bool sourceHasCurrent, out bool result)
            {
                if (sourceHasCurrent)
                {
                    if (SourceCurrent is TResult castCurent)
                    {
                        Current = castCurent;
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