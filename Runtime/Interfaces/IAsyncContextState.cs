namespace UniModules.UniGame.Context.SerializableContext.Runtime.Abstract
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