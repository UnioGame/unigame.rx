using UniModules.UniGame.Core.Runtime.Interfaces;

namespace UniGame.GameFlow.Runtime.Interfaces
{
    using System;

    public interface IGameService : 
        IDisposable, 
        ILifeTimeContext
    {
    }
}
