namespace UniGame.Core.Runtime
{
    using UniRx;

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