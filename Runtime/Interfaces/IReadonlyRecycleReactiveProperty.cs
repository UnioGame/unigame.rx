namespace UniGame.Core.Runtime.Rx
{
    using UniRx;

    public interface IReadonlyRecycleReactiveProperty<TValue> : 
        IReadOnlyReactiveProperty<TValue>,
        IRecycleObservable<TValue>, 
        IContainerValueStatus
    {
    }
}