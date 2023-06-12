#pragma warning disable CS1591
#pragma warning disable CS0436

using System.Runtime.CompilerServices;


namespace UniverseEngine
{
    [AsyncMethodBuilder(typeof(AsyncUniTaskVoidMethodBuilder))]
    public readonly struct UniTaskVoid
    {
        public void Forget()
        {
        }
    }
}

