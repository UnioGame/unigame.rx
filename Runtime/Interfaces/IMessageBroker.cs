namespace UniGame.Runtime.Rx
{
    using System;
    using Core.Runtime;

    public interface IMessageBroker : IMessagePublisher, IMessageReceiver, IDisposable
    {
    }
}