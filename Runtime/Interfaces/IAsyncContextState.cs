namespace UniGame.Context.Runtime
{
    using global::UniGame.Core.Runtime;

    public interface IAsyncContextState<TValue>  : IAsyncState<IContext,TValue> 
    {
        
    }
    
    public interface IAsyncContextStateStatus  : IAsyncState<IContext,AsyncStatus> 
    {
        
    }
    
    public interface IAsyncContextState  : IAsyncState<IContext> 
    {
        
    }

}