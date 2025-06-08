using System;

namespace UniGame.Runtime.Rx.Runtime.Operations
{
    using R3;

    public abstract class OperatorObserverBase<TSource, TResult> : IDisposable, IObserver<TSource>
    {
        protected internal volatile Observer<TResult> observer;
        IDisposable cancel;

        public OperatorObserverBase(Observer<TResult> observer, IDisposable cancel)
        {
            this.observer = observer;
            this.cancel = cancel;
        }

        public abstract void OnNext(TSource value);

        public abstract void OnError(Exception error);

        public abstract void OnCompleted();

        public void Dispose()
        {
            observer = null;
            var target = System.Threading.Interlocked.Exchange(ref cancel, null);
            target?.Dispose();
        }
    }
}