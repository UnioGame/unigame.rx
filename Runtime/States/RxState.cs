namespace UniGame.Rx.Runtime
{
    using System;
    using Cysharp.Threading.Tasks;
    using global::UniGame.Core.Runtime;
    using R3;
    using UniGame.Runtime.DataFlow;


    public abstract class RxState<TData> : RxState<TData, Unit>,
        IRxState<TData>
    {
    }

    public abstract class RxState<TData, TResult> : IRxState<TData, TResult>
    {
        private LifeTime _lifeTime;
        private bool _isActive;
        private bool _isInitialized;
        private Exception _exception;

        #region public properties
        
        public ReactiveProperty<TData> Context { get; } = new();

        public ReactiveProperty<TResult> Result { get; } = new();
        
        public ILifeTime LifeTime => _lifeTime = (_lifeTime ?? new ());

        public bool IsActive => _isActive;

        #endregion

        public async UniTask<TResult> ExecuteAsync(TData data)
        {
            //state already active
            if (_isActive) return await Result.FirstAsync();
            _isActive = true;
            
            Context.Value = data;
            
            if (!_isInitialized) await OnInitializeAsync(data);
            
            //setup default value
            await OnInitializeAsync(data);

            var executionResult = await OnExecuteAsync(data, _lifeTime);

            if (executionResult.success)
            {
                await OnCompleteAsync(data, executionResult.result, _lifeTime);
            }
            else
            {
                await OnFallbackAsync(data,executionResult,_lifeTime);
            }
            
            await OnExit(data,executionResult,_lifeTime);
            
            Result.Value = executionResult.result;
            
            return executionResult.result;
        }

        public void Dispose() => _lifeTime.Terminate();

        protected virtual UniTask OnInitializeAsync(TData value)
        {
            return UniTask.CompletedTask;
        }

        protected abstract UniTask<StateResult<TResult>> OnExecuteAsync(TData data, ILifeTime executionLifeTime);

        protected virtual UniTask OnCompleteAsync(TData data, TResult value, ILifeTime lifeTime)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask OnExit(TData data,StateResult<TResult> result, ILifeTime lifeTime)
        {
            return UniTask.CompletedTask;
        }

        protected virtual async UniTask<bool> OnFallbackAsync(TData data,StateResult<TResult> result, ILifeTime lifeTime)
        {
            return true;
        }
    }
    
    [Serializable]
    public struct StateResult<TResult>
    {
        public bool success;
        public string error;
        public Exception exception;
        public TResult result;
    }
}