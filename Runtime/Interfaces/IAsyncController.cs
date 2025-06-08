namespace UniGame.Core.Runtime
{
    using System;
    using Cysharp.Threading.Tasks;
    using R3;

    public interface IAsyncController : IDisposable
    {
        
        ReadOnlyReactiveProperty<bool> IsInitialized { get; }

        ILifeTime LifeTime { get; }

        UniTask Initialize();
        
    }
}