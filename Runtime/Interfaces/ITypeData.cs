namespace UniGame.Core.Runtime
{
    using UniGame.Runtime.Rx;

    public interface ITypeData :
        IMessageBroker,
        IReadonlyTypeData
    {
        bool Remove<TData>();
    }

    public interface IReadonlyTypeData :
        IMessageReceiver,
        IReadOnlyData,
        IValueContainerStatus
    {
    }
}