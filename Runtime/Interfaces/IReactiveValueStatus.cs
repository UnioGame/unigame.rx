namespace UniGame.Core.Runtime
{
    using R3;

    public interface ReactiveValueStatus
    {
        ReadOnlyReactiveProperty<bool> HasValueSource { get; }

    }
}