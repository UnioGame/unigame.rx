using UniGame.Core.Runtime;

namespace UniGame.Runtime.Rx
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using R3;

    [Serializable]
    public class ReactiveValue<T> : ReadOnlyReactiveProperty<T>,
        IValueContainerStatus,
        IReadonlyObjectValue,
        IObservable<T>
    {
        private const byte NotCompleted = 0;
        private const byte CompletedSuccess = 1;
        private const byte CompletedFailure = 2;
        private const byte Disposed = 3;
        private byte completeState;
        private Exception? error;
        private T currentValue;
        private IEqualityComparer<T>? equalityComparer;
        private bool hasValue;
        
        private ObserverNode
#nullable enable
            ? root;


        public bool HasValue => hasValue;

        public Type Type => typeof(T);
        public object ObjectValue => currentValue;
        
        public IEqualityComparer<T>? EqualityComparer => equalityComparer;

        public override T CurrentValue => currentValue;

        public bool HasObservers => root != null;

        public bool IsCompleted => completeState == 1 || completeState == 2;

        public bool IsDisposed => completeState == 3;

        public bool IsCompletedOrDisposed => IsCompleted || IsDisposed;

        public virtual T Value
        {
            get => currentValue;
            set
            {
                OnValueChanging(ref value);
                if (EqualityComparer != null && EqualityComparer.Equals(currentValue, value))
                    return;

                hasValue = true;
                currentValue = value;
                
                OnValueChanged(value);
                OnNextCore(value);
            }
        }

        public ReactiveValue()
        {
            equalityComparer = EqualityComparer<T>.Default;
            hasValue = false;
            currentValue = default;
        }

        public ReactiveValue(T value)
            : this(value, EqualityComparer<T>.Default)
        {
        }

        public ReactiveValue(T value, IEqualityComparer<T>? equalityComparer)
        {
            this.hasValue = true;
            this.equalityComparer = equalityComparer;
            OnValueChanging(ref value);
            currentValue = value;
            OnValueChanged(value);
        }

        protected ReactiveValue(
            T value,
            IEqualityComparer<T>? equalityComparer,
            bool callOnValueChangeInBaseConstructor)
        {
            this.equalityComparer = equalityComparer;
            if (callOnValueChangeInBaseConstructor)
                OnValueChanging(ref value);
            currentValue = value;
            if (!callOnValueChangeInBaseConstructor)
                return;
            OnValueChanged(value);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return Subscribe(observer.ToObserver());
        }

        public void SetValueForce(T value) => OnNext(value);

        protected virtual void OnValueChanging(ref T value)
        {
        }

        protected ref T GetValueRef() => ref currentValue;

        public virtual void ForceNotify() => OnNext(Value);

        public virtual void OnNext(T value)
        {
            OnValueChanging(ref value);
            currentValue = value;
            hasValue = true;
            OnValueChanged(value);
            OnNextCore(value);
        }

        protected virtual void OnNextCore(T value)
        {
            ThrowIfDisposed();
            if (!hasValue) return;
            if (IsCompleted) return;
            
            ObserverNode observerNode = root;
            ObserverNode previous = observerNode?.Previous;
            for (; observerNode != null; observerNode = observerNode.Next)
            {
                observerNode.Observer.OnNext(value);
                if (observerNode == previous)
                    break;
            }
        }

        public void OnErrorResume(Exception error)
        {
            ThrowIfDisposed();
            if (IsCompleted)
                return;
            OnReceiveError(error);
            ObserverNode observerNode =
                Volatile.Read<ObserverNode>(ref root);
            ObserverNode previous = observerNode?.Previous;
            for (; observerNode != null; observerNode = observerNode.Next)
            {
                observerNode.Observer.OnErrorResume(error);
                if (observerNode == previous)
                    break;
            }
        }

        public void OnCompleted(Result result)
        {
            ThrowIfDisposed();
            if (IsCompleted)
                return;
            ObserverNode observerNode = null;
            lock (this)
            {
                if (completeState == 0)
                {
                    completeState = result.IsSuccess ? (byte)1 : (byte)2;
                    error = result.Exception;
                    observerNode = Volatile.Read<ObserverNode>(ref root);
                    Volatile.Write<ObserverNode>(ref root,
                        null);
                }
                else
                {
                    ThrowIfDisposed();
                    return;
                }
            }

            if (result.IsFailure)
                OnReceiveError(result.Exception);
            ObserverNode previous = observerNode?.Previous;
            for (; observerNode != null; observerNode = observerNode.Next)
            {
                observerNode.Observer.OnCompleted(result);
                if (observerNode == previous)
                    break;
            }
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            Result? nullable;
            lock (this)
            {
                ThrowIfDisposed();
                nullable = !IsCompleted
                    ? new Result?()
                    : new Result?(error == null ? Result.Success : Result.Failure(error));
            }

            if (!nullable.HasValue)
            {
                if(hasValue)
                    observer.OnNext(currentValue);
                lock (this)
                {
                    ThrowIfDisposed();
                    if (!IsCompleted)
                        return new ObserverNode(this, observer);
                    nullable = new Result?(error == null ? Result.Success : Result.Failure(error));
                }
            }
            else if (nullable.HasValue)
            {
                if (nullable.Value.IsSuccess)
                    observer.OnNext(currentValue);
                observer.OnCompleted(nullable.Value);
                return Disposable.Empty;
            }

            if (nullable.HasValue)
                observer.OnCompleted(nullable.Value);
            return Disposable.Empty;
        }

        private void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("");
        }

        public override void Dispose() => Dispose(true);

        public void Dispose(bool callOnCompleted)
        {
            ObserverNode observerNode = null;
            lock (this)
            {
                if (completeState == 3)
                    return;
                if (callOnCompleted && !IsCompleted)
                    observerNode = Volatile.Read<ObserverNode>(ref root);
                Volatile.Write<ObserverNode>(ref root, null);
                completeState = 3;
            }

            for (; observerNode != null; observerNode = observerNode.Next)
                observerNode.Observer.OnCompleted<T>();
            DisposeCore();
        }

        protected virtual void DisposeCore()
        {
        }

        public override string? ToString()
        {
            return currentValue != null ? currentValue.ToString() : "(null)";
        }

        private sealed class ObserverNode : IDisposable
        {
            public readonly Observer<T> Observer;
            private ReactiveValue<T>? parent;

            public ObserverNode
#nullable enable
                ? Previous { get; set; }

            public ObserverNode
#nullable enable
                ? Next { get; set; }

            public ObserverNode(ReactiveValue<T> parent, Observer<T> observer)
            {
                this.parent = parent;
                Observer = observer;
                if (parent.root == null)
                {
                    Volatile.Write<ObserverNode>(ref parent.root, this);
                }
                else
                {
                    ObserverNode observerNode = parent.root.Previous ?? parent.root;
                    observerNode.Next = this;
                    Previous = observerNode;
                    parent.root.Previous = this;
                }
            }

            public void Dispose()
            {
                ReactiveValue<T> ReactiveValue =
                    Interlocked.Exchange<ReactiveValue<T>>(ref parent, null);
                if (ReactiveValue == null)
                    return;
                lock (ReactiveValue)
                {
                    if (ReactiveValue.IsCompletedOrDisposed)
                        return;
                    if (this == ReactiveValue.root)
                    {
                        if (Previous == null || Next == null)
                        {
                            ReactiveValue.root = null;
                        }
                        else
                        {
                            ObserverNode next = Next;
                            next.Previous = next.Next != null ? Previous : null;
                            ReactiveValue.root = next;
                        }
                    }
                    else
                    {
                        Previous.Next = Next;
                        if (Next != null)
                            Next.Previous = Previous;
                        else
                            ReactiveValue.root.Previous = Previous;
                    }
                }
            }
        }

    }
}