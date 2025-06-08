namespace UniGame.Runtime.Rx
{
    public interface IMessagePublisher
    {
        void Publish<T>(T message);
    }
}