#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Threading;
using UnityEngine;
using System;

namespace UniverseEngine
{

    public static partial class CancellationTokenSourceExtensions
    {
        static readonly Action<object> CancelCancellationTokenSourceStateDelegate = new(CancelCancellationTokenSourceState);

        static void CancelCancellationTokenSourceState(object state)
        {
            var cts = (CancellationTokenSource)state;
            cts.Cancel();
        }

        public static IDisposable CancelAfterSlim(this CancellationTokenSource cts, int millisecondsDelay, DelayType delayType = DelayType.DeltaTime, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update)
        {
            return CancelAfterSlim(cts, TimeSpan.FromMilliseconds(millisecondsDelay), delayType, delayTiming);
        }

        public static IDisposable CancelAfterSlim(this CancellationTokenSource cts, TimeSpan delayTimeSpan, DelayType delayType = DelayType.DeltaTime, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update)
        {
            return PlayerLoopTimer.StartNew(delayTimeSpan, false, delayType, delayTiming, cts.Token, CancelCancellationTokenSourceStateDelegate, cts);
        }

        public static void RegisterRaiseCancelOnDestroy(this CancellationTokenSource cts, Component component)
        {
            RegisterRaiseCancelOnDestroy(cts, component.gameObject);
        }

        public static void RegisterRaiseCancelOnDestroy(this CancellationTokenSource cts, GameObject gameObject)
        {
            var trigger = gameObject.GetAsyncDestroyTrigger();
            trigger.CancellationToken.RegisterWithoutCaptureExecutionContext(CancelCancellationTokenSourceStateDelegate, cts);
        }
    }
}

