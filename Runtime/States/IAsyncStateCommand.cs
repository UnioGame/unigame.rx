namespace UniGame.Context.Runtime.Runtime.States
{
    using Cysharp.Threading.Tasks;

    public interface IAsyncStateCommand<TData, TResult>
    {
        UniTask<TResult> ExecuteStateAsync(TData value);
    }
}