namespace UniGame.Core.Runtime.Rx
{
    using System;
    using UniRx;

    public interface IReactiveValue<TValue> : 
        IReactiveProperty<TValue>,
        IReadonlyReactiveValue<TValue>,
        IValueContainerStatus,
        IDisposable,
        IReadonlyObjectValue
#if UNITY_EDITOR
        ,IObjectValue
#endif
        
    {
        new TValue Value { get; set; }

        void SetValueForce(TValue propertyValue);

        void SetValueSilence(TValue value);

        void RemoveValueSilence();
    }
}
