using System.Threading;
using UniGame.Core.Runtime;

namespace UniGame.Runtime.Extension
{
    using System;
    using Cysharp.Threading.Tasks;
    using global::UniGame.Core.Runtime;
    using UnityEngine;

    public static class AsyncExtension
    {
        public static float DefaultTimeOutMs = 60000;
        
        public static async UniTask WaitUntil(this object source, Func<bool> waitFunc,PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            while (waitFunc() == false)
            {
                await UniTask.Yield(timing);
            }
        }

        public static async UniTask WaitUntil(this AsyncOperation source)
        {
            while (source.isDone == false)
            {
                await UniTask.DelayFrame(1);
            }

        }

        public static async UniTask WaitUntil(this ICompletionStatus status)
        {
            while (status.IsComplete == false) {
                await UniTask.DelayFrame(1);
            }
        }

        public static async UniTask<(bool IsCanceled, TValue Result)> AwaitFirstAsyncNoException<TValue>(this IObservable<TValue> value, ILifeTime lifeTime)
        {
            return await AwaitFirstAsync(value, lifeTime).SuppressCancellationThrow();
        }

        public static async UniTask<TValue> AwaitFirstAsync<TValue>(this IObservable<TValue> value, ILifeTime lifeTime)
        {
            CancellationTokenSource tokenSource = null;
            
#if UNITY_EDITOR
            tokenSource = new CancellationTokenSource();
            lifeTime.AwaitTimeoutLog(TimeSpan.FromMilliseconds(DefaultTimeOutMs),() => $"AwaitFirstAsync FOR {nameof(TValue)} FAILED")
                .AttachExternalCancellation(tokenSource.Token)
                .Forget();
#endif
            try
            {
                var result = await value.ToUniTask(true, lifeTime.Token);
                return result;
            }
            finally
            {
#if UNITY_EDITOR
                tokenSource?.Cancel();
                tokenSource?.Dispose();
#endif
            }

            return default;
        }
    }
}
