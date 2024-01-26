namespace UniModules.UniCore.Runtime.Common
{
    using System;
    using global::UniGame.Runtime.ObjectPool.Extensions;
    using global::UniGame.Core.Runtime.ObjectPool;
    using global::UniGame.Core.Runtime;
    using UniGame.Core.Runtime.Rx;

    [Serializable]
    public class ReactiveValue<TData> : IDataValue<TData> , IPoolable
    {
        private bool hasValue = false;
        private bool isValueType = typeof(TData).IsValueType;
        private UniGame.Core.Runtime.Rx.ReactiveValue<TData> _reactiveValue = new UniGame.Core.Runtime.Rx.ReactiveValue<TData>();

        public TData Value
        {
            get => _reactiveValue.Value;
            private set => _reactiveValue.SetValue(value);
        }

        public bool HasValue => hasValue;

        public bool IsValueType => isValueType;
        
        public void SetValue(TData value)
        {
            hasValue = true;
            Value = value;
        }

        public void Dispose() => this.DespawnWithRelease();

        public IDisposable Subscribe(IObserver<TData> action)
        {
            return _reactiveValue.Subscribe(action);
        }

        public void Release()
        {
            hasValue = false;
            _reactiveValue.Release();
        }
    }
}