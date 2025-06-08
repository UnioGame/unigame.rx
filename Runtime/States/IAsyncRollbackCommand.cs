namespace UniGame.Core.Runtime
{
    public interface IAsyncRollbackCommand : IAsyncCommand, IAsyncRollback
    {
       
    }
    
    public interface IAsyncRollbackCommand<T> : IAsyncCommand<T>,IAsyncRollback<T>
    {
        
    }
    
    public interface IAsyncRollbackCommand<TCommand,TRollback> : IAsyncCommand<TCommand>,IAsyncRollback<TRollback>
    {
        
    }
}