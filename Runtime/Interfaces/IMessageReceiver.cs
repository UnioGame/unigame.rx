namespace UniGame.Core.Runtime
{
    using R3;

    public interface IMessageReceiver
    {
        Observable<T> Receive<T>();
    }
}