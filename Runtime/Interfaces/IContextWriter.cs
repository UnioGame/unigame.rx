namespace UniGame.Core.Runtime
{
    using UniRx;

    public interface IContextWriter : IMessagePublisher
    {
        
        bool Remove<TData>();

        void CleanUp();

    }
}