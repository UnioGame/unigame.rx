namespace UniGame.Core.Runtime.Rx
{
    using UniRx;

    public interface IReadonlyReactiveValue<TValue> : 
        IReadOnlyReactiveProperty<TValue>,
        IRecycleObservable<TValue>, 
        IContainerValueStatus
    {
    }
}