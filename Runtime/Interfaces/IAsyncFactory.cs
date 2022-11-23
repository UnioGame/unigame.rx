using Cysharp.Threading.Tasks;

namespace UniGame.Core.Runtime
{
    public interface IAsyncFactory<TResult>
    {

        UniTask<TResult> Create();

    }
    
    public interface IAsyncFactory<TValue,TResult>
    {
        UniTask<TResult> Create(TValue value);
    }
    
        
    public interface IAsyncPrototype<TValue>
    {
        UniTask<TValue> Create();
    }
}
