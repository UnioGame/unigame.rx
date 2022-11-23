namespace UniGame.Core.Runtime
{
    using System;
    using UniRx;

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

    public interface IDisposableContext : 
        IContext, 
        IDisposable
    {
        
    }
}
