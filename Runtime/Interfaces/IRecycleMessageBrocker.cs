using UniGame.Core.Runtime.ObjectPool;

namespace UniGame.Core.Runtime.Rx
{
    using UniRx;

    public interface IRecycleMessageBrocker : IMessageBroker, IPoolable
    {
    }
}