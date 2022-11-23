namespace UniGame.Core.Runtime
{
    using System;
    using Cysharp.Threading.Tasks;
    using UniRx;

    public interface IAsyncController : IDisposable
    {
        
        IReadOnlyReactiveProperty<bool> IsInitialized { get; }

        ILifeTime LifeTime { get; }

        UniTask Initialize();
        
    }
}