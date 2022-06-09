namespace UniModules.UniGame.Core.Runtime.Interfaces
{
    using Cysharp.Threading.Tasks;

    public interface IUpdatableJob
    {
        PlayerLoopTiming UpdateType { get; }

        void Update();
    }
}