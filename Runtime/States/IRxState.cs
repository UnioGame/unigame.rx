namespace UniGame.Core.Runtime
{
    using System;
    using Cysharp.Threading.Tasks;
    using R3;

    public interface IRxState<TValue> : IRxState<TValue,Unit>
    {
        
    }
    
    public interface IRxState<TValue,TResult>:
        ILifeTimeContext,
        IActiveStatus,
        IDisposable
    {
        ReactiveProperty<TValue> Context { get; }
        
        ReactiveProperty<TResult> Result { get; }
        
        UniTask<TResult> ExecuteAsync(TValue value);
    }
    
    public interface IRxCommand<TValue, TResult>
    {
        Observable<TResult> Execute(TValue value);
    }
    
    public interface IRxEndPoint
    {
        void ExitState();
    }
    
    public interface IRxRolldback<TData>
    {
        Observable<bool> Rollback(TData data);
    }
    
    public interface IRxRolldback
    {
        Observable<bool> Rolldback();
    }
    
    public interface IRxCompletion<TData,TResult>
    {
        Observable<Unit> CompleteAsync(TData data, TResult value,  ILifeTime lifeTime);
    }

    public interface IRxStateExecution<TData,TResult>
    {
        Observable<TResult> ExecuteState(TData data, ILifeTime lifeTime);
    }
}