namespace UniGame.Core.Runtime
{
    using UniRx;

    public interface IReactiveValueStatus
    {
        IReadOnlyReactiveProperty<bool> HasValueSource { get; }

    }
}