namespace UniGame.Core.Runtime
{
    using UniRx;

    public interface IWritableValue
    {
        void CopyTo(IMessagePublisher target);
        
    }
}
