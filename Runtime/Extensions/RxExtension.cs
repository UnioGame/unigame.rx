namespace UniGame.Runtime.Rx.Extensions
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core.Runtime;
    using Cysharp.Threading.Tasks;
    using global::UniGame.Runtime.ObjectPool;
    using Rx;
    using global::UniGame.Runtime.Rx;
    using R3;


    public static class RxExtension
    {
        
        public static void Execute(this ReactiveCommand command)
        {
            command.Execute(Unit.Default);
        }
        
        public static void Execute(this ReactiveCommand<Unit> command)
        {
            command.Execute(Unit.Default);
        }
        
        public static IDisposable Subscribe(this Observable<Unit> observable,Action action)
        {
            return observable.Subscribe(action,(x,y) => y());
        }
        
        public static IRecycleObserver<T> CreateRecycleObserver<T>(this object _, 
            Action<T> onNext, 
            Action onComplete = null,
            Action<Exception> onError = null)
        {
            
            var observer = ClassPool.Spawn<RecycleActionObserver<T>>();
            
            observer.Initialize(onNext,onComplete,onError);

            return observer;

        }

        public static Observable<T> When<T>(this Observable<T> source, Predicate<T> predicate, Action<T> action)
        {
            return source
                .Where(predicate,(x,y) => y(x))
                .Do(x => action?.Invoke(x));
        }

        public static Observable<T> When<T>(this Observable<T> source, 
            Predicate<T> predicate, 
            Action<T> actionIfTrue, 
            Action<T> actionIfFalse)
        {
            return source
                .Where(x => predicate(x))
                .Do(x =>
                {
                    if (predicate(x))
                    {
                        actionIfTrue(x);
                    }
                    else
                    {
                        actionIfFalse(x);
                    }
                });
        }

        public static Observable<bool> WhenTrue(this Observable<bool> source, Action<bool> action)
        {
            return source.When(x => x, action);
        }

        public static Observable<bool> WhenFalse(this Observable<bool> source, Action<bool> action)
        {
            return source.When(x => !x, action);
        }
    }
}
