using System;

// implements note : all field must be readonly.
namespace UniGame.Runtime.Rx.Runtime.Operations
{
    using R3;

    public abstract class OperatorObservableBase<T> : IObservable<T>
    {
        readonly bool isRequiredSubscribeOnCurrentThread;

        public OperatorObservableBase(bool isRequiredSubscribeOnCurrentThread)
        {
            this.isRequiredSubscribeOnCurrentThread = isRequiredSubscribeOnCurrentThread;
        }

        public bool IsRequiredSubscribeOnCurrentThread()
        {
            return isRequiredSubscribeOnCurrentThread;
        }
        
        public IDisposable Subscribe(IObserver<T> observer)
        {
            return Subscribe(observer.ToObserver());
        }
        
        public IDisposable Subscribe(Observer<T> observer)
        {
            var subscription = new SingleAssignmentDisposable();

            // note:
            // does not make the safe observer, it breaks exception durability.
            // var safeObserver = Observer.CreateAutoDetachObserver<T>(observer, subscription);

            subscription.Disposable = SubscribeCore(observer, subscription);

            return subscription;
        }

        protected abstract IDisposable SubscribeCore(Observer<T> observer, IDisposable cancel);

    }
}
