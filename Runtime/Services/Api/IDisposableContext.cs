namespace UniGame.Core.Runtime
{
    using System;

    public interface IDisposableContext : 
        IContext, 
        IDisposable
    {
        
    }
}