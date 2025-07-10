using System;
using Cysharp.Threading.Tasks;
using UniGame.Runtime.DataFlow;
using UniGame.Runtime.Extension;
using UniGame.Runtime.ObjectPool.Extensions;
using UniGame.Core.Runtime.ObjectPool;
using UniGame.Core.Runtime;

namespace UniModules.UniGame.CoreModules.UniGame.Core.Runtime.Async
{
    using R3;

    public class AwaitFirstAsyncOperation<TData> : IPoolable, IDisposable
    {
        private LifeTime _lifeTime = new ();
        private bool _valueInitialized = false;
        private TData _value;
    
        public async UniTask<TData> AwaitFirstAsync(
            IObservable<TData> observable, 
            ILifeTime observableLIfeTime,
            Func<TData,bool> predicate = null)
        {
            _valueInitialized = false;
            
            if (observable == null) 
                return default;

            observable.ToObservable()
                .Subscribe(x => OnNext(x,predicate))
                .AddTo(observableLIfeTime);
            
            await this.WaitUntil(() => _lifeTime.IsTerminated || _valueInitialized)
                .AttachExternalCancellation(_lifeTime.Token);
            
            return _value;
        }

        public void Dispose() => this.DespawnWithRelease();
        
        public void Release()
        {
            _lifeTime.Terminate();
            _valueInitialized = true;
            _value = default;
        }
    
        private void OnNext(TData value,Func<TData,bool> predicate = null)
        {
            if (_valueInitialized || (predicate != null && !predicate.Invoke(value)))
                return;
        
            _valueInitialized = true;
            _value = value;
        }

    }
}
