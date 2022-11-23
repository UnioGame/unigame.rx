namespace UniGame.Core.Runtime
{
    using UniRx;

    public interface IReactiveStatus
    {
        /// <summary>
        /// is service ready to work
        /// </summary>
        IReadOnlyReactiveProperty<bool> IsReady { get; }
    }
}