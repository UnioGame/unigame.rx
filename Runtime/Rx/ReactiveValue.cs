using UniGame.Core.Runtime;

namespace UniGame.Runtime.Rx
{
    using System;
    using R3;

    [Serializable]
    public class ReactiveValue<T> : ReactiveProperty<T>,
        IValueContainerStatus,
        IReadonlyObjectValue,
        IObservable<T>
    {
        public ReactiveValue() : base() { }
        
        public ReactiveValue(T value) : base(value) { }
        
        public bool HasValue => CurrentValue != null;
        
        public Type Type => typeof(T);

        public object ObjectValue => CurrentValue;

        public void SetValueForce(T value) => OnNext(value);
        
        public IDisposable Subscribe(IObserver<T> observer)
        {
            return Subscribe(observer.ToObserver());
        }
    }
}