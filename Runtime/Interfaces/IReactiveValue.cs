namespace UniGame.Core.Runtime.Rx
{
    using System;
    using UniRx;

    public interface IReactiveValue<TValue> : 
        IReactiveProperty<TValue>,
        IReadonlyReactiveValue<TValue>,
        IValueContainerStatus,
        IDisposable
#if UNITY_EDITOR
        ,IReadonlyObjectValue
        ,IObjectValue
#endif
        
    {
        new TValue Value { get; set; }

        void SetValueForce(TValue propertyValue);

        void SetValueSilence(TValue value);

        void RemoveValueSilence();
    }
}
