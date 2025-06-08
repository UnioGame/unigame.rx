namespace UniGame.Runtime.AsyncOperations
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Cysharp.Threading.Tasks;
    using global::UniGame.Runtime.ObjectPool;
    using global::UniGame.Runtime.ObjectPool.Extensions;
    using R3;
    using UnityEngine;


    public static class AsyncOperationExtensions
    {

        public static async Task<T> WaitAsync<T>(this Task<T> task, CancellationToken token)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();
            using (token.Register(() => taskCompletionSource.TrySetCanceled(token), false)) {
                return await await Task.WhenAny(task, taskCompletionSource.Task).ConfigureAwait(false);
            }
        }
        
        public static IEnumerator AwaitAsUniTask<T>(this Task<T> task)
        {
            yield return task.AsUniTask().ToCoroutine();
        }
        
        public static IEnumerator AwaitTask<T>(this Task<T> task)
        {
            while (!task.IsCompleted) {
                yield return null;                
            }

            if (task.IsFaulted) {
                Debug.LogError($"{nameof(task)} Filed");
            }
        }
        
        public static IEnumerator AwaitTask<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            while (!task.IsCompleted) {
                yield return null;
            }

            if (task.IsFaulted) {
                Debug.LogError($"{nameof(task)} Filed");
            }
        }
    }
}
