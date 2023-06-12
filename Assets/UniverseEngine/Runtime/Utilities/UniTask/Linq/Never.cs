using System.Threading;

namespace UniverseEngine
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static IUniTaskAsyncEnumerable<T> Never<T>()
        {
            return global::UniverseEngine.Never<T>.Instance;
        }
    }

    internal class Never<T> : IUniTaskAsyncEnumerable<T>
    {
        public static readonly IUniTaskAsyncEnumerable<T> Instance = new Never<T>();

        Never()
        {
        }

        public IUniTaskAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken))
        {
            return new _Never(cancellationToken);
        }

        class _Never : IUniTaskAsyncEnumerator<T>
        {
            CancellationToken cancellationToken;

            public _Never(CancellationToken cancellationToken)
            {
                this.cancellationToken = cancellationToken;
            }

            public T Current => default(T);

            public UniTask<bool> MoveNextAsync()
            {
                var tcs = new UniTaskCompletionSource<bool>();

                cancellationToken.Register(state =>
                {
                    var task = (UniTaskCompletionSource<bool>)state;
                    task.TrySetCanceled(cancellationToken);
                }, tcs);

                return tcs.Task;
            }

            public UniTask DisposeAsync()
            {
                return default;
            }
        }
    }
}