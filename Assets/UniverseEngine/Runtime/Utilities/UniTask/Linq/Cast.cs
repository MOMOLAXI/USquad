using System.Threading;

namespace UniverseEngine
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static IUniTaskAsyncEnumerable<TResult> Cast<TResult>(this IUniTaskAsyncEnumerable<object> source)
        {
            Error.ThrowArgumentNullException(source, nameof(source));

            return new Cast<TResult>(source);
        }
    }

    sealed internal class Cast<TResult> : IUniTaskAsyncEnumerable<TResult>
    {
        readonly IUniTaskAsyncEnumerable<object> source;

        public Cast(IUniTaskAsyncEnumerable<object> source)
        {
            this.source = source;
        }

        public IUniTaskAsyncEnumerator<TResult> GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken))
        {
            return new _Cast(source, cancellationToken);
        }

        class _Cast : AsyncEnumeratorBase<object, TResult>
        {
            public _Cast(IUniTaskAsyncEnumerable<object> source, CancellationToken cancellationToken)

                : base(source, cancellationToken)
            {
            }

            protected override bool TryMoveNextCore(bool sourceHasCurrent, out bool result)
            {
                if (sourceHasCurrent)
                {
                    Current = (TResult)SourceCurrent;
                    result = true;
                    return true;
                }

                result = false;
                return true;
            }
        }
    }
}