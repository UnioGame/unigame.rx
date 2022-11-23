namespace UniGame.Core.Runtime
{
    using Cysharp.Threading.Tasks;

    public interface ILifeTimeCommand
    {
        UniTask Execute(ILifeTime lifeTime);
    }
}