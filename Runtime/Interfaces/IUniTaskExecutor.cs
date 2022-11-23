namespace UniGame.Core.Runtime
{
    using Cysharp.Threading.Tasks;

    public interface IUniTaskExecutor
    {

        UniTask Execute(UniTask actionTask);

        void Stop();

    }
}
