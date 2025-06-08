namespace UniGame.Core.Runtime
{
    using R3;

    public interface IReactiveStatus
    {
        /// <summary>
        /// is service ready to work
        /// </summary>
        ReadOnlyReactiveProperty<bool> IsReady { get; }
    }
}