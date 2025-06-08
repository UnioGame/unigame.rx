namespace UniGame.Core.Runtime
{
    using UniGame.Runtime.Rx;


    public interface IWritableValue
    {
        void CopyTo(IMessagePublisher target);
        
    }
}
