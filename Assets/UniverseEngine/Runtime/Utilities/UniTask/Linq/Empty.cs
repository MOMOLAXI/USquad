using System.Threading;

namespace UniverseEngine
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static IUniTaskAsyncEnumerable<T> Empty<T>()
        {
            return global::UniverseEngine.Empty<T>.Instance;
        }
    }

    internal class Empty<T> : IUniTaskAsyncEnumerable<T>
    {
        public static readonly IUniTaskAsyncEnumerable<T> Instance = new Empty<T>();

        Empty()
        {
        }

        public IUniTaskAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _Empty.Instance;
        }

        class _Empty : IUniTaskAsyncEnumerator<T>
        {
            public static readonly IUniTaskAsyncEnumerator<T> Instance = new _Empty();

            _Empty()
            {
            }

            public T Current => default(T);

            public UniTask<bool> MoveNextAsync()
            {
                return CompletedTasks.False;
            }

            public UniTask DisposeAsync()
            {
                return default;
            }
        }
    }
}