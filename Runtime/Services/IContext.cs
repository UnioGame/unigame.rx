namespace UniGame.Core.Runtime
{
    using UniGame.Runtime.Rx;


    public interface IReadOnlyContext :
        ILifeTimeContext,
        IReadonlyTypeData
    {
        
    }
    
    public interface IMessageContext : 
        IReadOnlyContext,
        IMessageBroker
    { }
    
    public interface IContext :
        IMessageContext,
        IManagedBroadcaster<IMessagePublisher>,
        ITypeData
    {
        
    }
}
