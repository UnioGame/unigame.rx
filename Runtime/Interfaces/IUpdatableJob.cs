namespace UniGame.Core.Runtime
{
    using Cysharp.Threading.Tasks;

    public interface IUpdatableJob
    {
        PlayerLoopTiming UpdateType { get; }

        void Update();
    }
}