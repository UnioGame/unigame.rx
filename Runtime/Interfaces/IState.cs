using UniGame.Core.Runtime.ObjectPool;

namespace UniGame.Core.Runtime
{
    public interface IState : 
        ICommand, 
        IEndPoint,
        ILifeTimeContext,
        IPoolable,
        IActiveStatus
    {
    }
    
    public interface IState<TResult,TValue> : 
        ICommand<TResult,TValue>, 
        IEndPoint,
        ILifeTimeContext,
        IPoolable,
        IActiveStatus
    {
    }
    

}