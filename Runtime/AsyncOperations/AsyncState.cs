namespace UniModules.UniGame.Core.Runtime.AsyncOperations
{
    using Cysharp.Threading.Tasks;
    using global::UniGame.Core.Runtime;
    using Rx;
    using UniCore.Runtime.AsyncOperations;
    using UniCore.Runtime.DataFlow;
    using UniRx;

    public class AsyncState<TData> :
        AsyncState<TData, AsyncStatus>,
        IAsyncState<TData>
    {
    }

    public class AsyncState<TData, TResult> :
        IAsyncState<TData, TResult>
    {
        private LifeTimeDefinition               _lifeTime;
        private bool                             _isActive;
        private bool                             _isInitialized;
        private TData                            _data;
        private RecycleReactiveProperty<TResult> _value;
        private UniTask<TResult>                 _taskHandle;

        #region public properties

        public IReadOnlyReactiveProperty<TResult> Value => _value = (_value ?? new RecycleReactiveProperty<TResult>());

        public ILifeTime LifeTime => _lifeTime = (_lifeTime ?? new LifeTimeDefinition());

        public bool IsActive => _isActive;

        #endregion

        public async UniTask<TResult> ExecuteAsync(TData data)
        {
            //state already active
            if (_isActive)
                return await UniTaskOperations.AwaitAsync(() => _isActive, () => _value.Value, LifeTime.CancellationToken);

            _isActive = true;

            if (!_isInitialized)
                Initialize();

            _data = data;

            //cleanup value on reset
            LifeTime.AddCleanUpAction(() => _value.Release());
            //setup default value
            _value.Value = GetInitialExecutionValue();

            //if target value contains lifetime, then bind
            var contextLifetime = data is ILifeTimeContext lifeTimeContext ? lifeTimeContext.LifeTime.Compose(LifeTime) : LifeTime;

            _taskHandle = OnExecute(data, contextLifetime)
                .AttachExternalCancellation(contextLifetime.CancellationToken)
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
                        result = await valueRollback.Rollback(data).AttachExternalCancellation(contextLifetime.CancellationToken);
                    if (this is IAsyncRollback<TData> rollback)
                        await rollback.Rollback(data).AttachExternalCancellation(contextLifetime.CancellationToken);
                    break;
            }

            _value.Value = result;

            await Finish(data).AttachExternalCancellation(contextLifetime.CancellationToken);

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
            _lifeTime?.Release();
        }

        protected virtual TResult          GetInitialExecutionValue()                         => default;
        protected virtual UniTask<TResult> OnExecute(TData data, ILifeTime executionLifeTime) => UniTask.FromResult<TResult>(default);

        protected virtual UniTask OnComplete(TResult value, TData data, ILifeTime lifeTime) => UniTask.CompletedTask;

        protected virtual UniTask OnExit(TData data) => UniTask.CompletedTask;

        private void Initialize()
        {
            _isInitialized =   true;
            _lifeTime      ??= new LifeTimeDefinition();
            _value         ??= new RecycleReactiveProperty<TResult>();
        }
    }
}