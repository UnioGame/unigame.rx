namespace UniGame.Runtime.Rx
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using R3;

    [Serializable]
    public class ReactiveCommand : ReactiveCommand<Unit>
    {
        public ReactiveCommand() : base() { }
        
        public ReactiveCommand(Action<Unit> execute) : base(execute) { }

        public ReactiveCommand(Func<Unit, CancellationToken, ValueTask> executeAsync,
            AwaitOperation awaitOperation = AwaitOperation.Sequential,
            bool configureAwait = true,
            bool cancelOnCompleted = false,
            int maxSequential = -1) 
            : base(executeAsync,awaitOperation,configureAwait,cancelOnCompleted,maxSequential) { }
        
        public ReactiveCommand(Observable<bool> canExecuteSource, bool initialCanExecute) 
            : base(canExecuteSource,initialCanExecute) { }
    }
}