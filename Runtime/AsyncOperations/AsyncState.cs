using UniGame.Core.Runtime;

namespace UniGame.Rx.Runtime
{
    using System;
    using Cysharp.Threading.Tasks;
    using UniGame.Runtime.DataFlow;
    using UniGame.Runtime.Rx;

    public class AsyncState<TData> :
        AsyncState<TData, AsyncStatus>,
        IAsyncState<TData>
    {
    }

    public class AsyncState<TData, TResult> : IAsyncState<TData, TResult>
    {
        private LifeTime               _lifeTime;
        private bool                             _isActive;
        private bool                             _isInitialized;
        private TData                            _data;
        private ReactiveValue<TResult> _value;
        private UniTask<TResult>                 _taskHandle;

        #region public properties

        public ILifeTime LifeTime => _lifeTime = (_lifeTime ?? new LifeTime());

        public bool IsActive => _isActive;

        #endregion

        public async UniTask<TResult> ExecuteAsync(TData data)
        {
            //state already active
            if (_isActive)
            {
                await UniTask.WaitWhile(this, x => x._isActive,cancellationToken:_lifeTime.Token);
                return _value.Value;
            }

            _isActive = true;

            if (!_isInitialized) Initialize();

            _data = data;

            //setup default value
            _value.Value = GetInitialExecutionValue();

            //if target value contains lifetime, then bind
            var contextLifetime = LifeTime;

            _taskHandle = OnExecute(data, contextLifetime)
                .AttachExternalCancellation(contextLifetime.Token)
                .Preserve();

            var result = await _taskHandle;

            if (!_isActive) return result;

            switch (_taskHandle.Status)
            {
                case UniTaskStatus.Succeeded:
                    await OnComplete(result, data, contextLifetime);
                    break;
                default:
                    if (this is IAsyncRollback<TData, TResult> valueRollback)
                        result = await valueRollback.Rollback(data).AttachExternalCancellation(contextLifetime.Token);
                    if (this is IAsyncRollback<TData> rollback)
                        await rollback.Rollback(data).AttachExternalCancellation(contextLifetime.Token);
                    break;
            }

            _value.Value = result;

            await Finish(data).AttachExternalCancellation(contextLifetime.Token);

            return result;
        }

        public async UniTask ExitAsync()
        {
            if (!_isActive)
                return;

            await Finish(_data);
        }

        private async UniTask Finish(TData data)
        {
            await OnExit(data);

            _isActive = false;
            _lifeTime?.Terminate();
        }

        protected virtual TResult          GetInitialExecutionValue()                         => default;
        protected virtual UniTask<TResult> OnExecute(TData data, ILifeTime executionLifeTime) => UniTask.FromResult<TResult>(default);

        protected virtual UniTask OnComplete(TResult value, TData data, ILifeTime lifeTime) => UniTask.CompletedTask;

        protected virtual UniTask OnExit(TData data) => UniTask.CompletedTask;

        private void Initialize()
        {
            _isInitialized =   true;
            _lifeTime      ??= new LifeTime();
            _value         ??= new ReactiveValue<TResult>();
        }
    }
    
        
    [Serializable]
    public struct StateResult
    {
        public bool success;
        public string error;
    }
}