using Cysharp.Threading.Tasks;

namespace UniGame.Core.Runtime.Extension
{
    using System;
    using System.Threading;
    using UniCore.Runtime.ProfilerTools;
    using Object = UnityEngine.Object;

    public static class UniTaskExtensions
    {
        public static bool IsCompleted(this UniTask task) => task.Status.IsCompleted();
    
        public static bool IsCompleted<T>(this UniTask<T> task) => task.Status.IsCompleted();
        
        public static async UniTask AwaitTiming(this object source,PlayerLoopTiming loopTiming,int awaitAmount)
        {
            var count = 0;
            
            while (count < awaitAmount)
            {
                await UniTask.Yield(loopTiming);
                count++;
            }
        }
        
        public static UniTask AttachTimeoutLogAsync(this UniTask task,string message,float timeOut, CancellationToken cancellationToken)
        {
            LogTimeoutErrorAsync(task, message, timeOut, cancellationToken)
                .Forget();
            return task;
        }
        
        private static async UniTask LogTimeoutErrorAsync(this UniTask task,string message,float timeOut, CancellationToken cancellationToken)
        {
            if (timeOut <= 0) return;

            await UniTask.Delay(TimeSpan.FromMilliseconds(timeOut), cancellationToken: cancellationToken)
                .AttachExternalCancellation(cancellationToken);

            GameLog.LogError(message);
        }

        public static async UniTask<TAsset> ToSharedInstanceAsync<TAsset>(
            this UniTask<TAsset> task, ILifeTime lifeTime)
            where TAsset : class
        {
            var instance = await ToSharedInstanceAsync(task);
            if(instance is Object asset)
                asset.DestroyWith(lifeTime);
            return instance;
        }
        
        public static async UniTask<TAsset> ToSharedInstanceAsync<TAsset>(
            this UniTask<TAsset> task)  
            where TAsset : class
        {
            var asset = await task;
            var instance = asset.ToSharedInstance();
            return instance;
        }
    }
}
