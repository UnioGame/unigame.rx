namespace UniGame.Rx.Runtime.Extensions
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using UniGame.Core.Runtime;
    using UniRx;
    using UnityEngine;

    public static class RxBindExtensions
    {
        #region lifetime context
        
        public static TView Bind<TView,TValue>(this TView view, IObservable<TValue> source, IObserver<Unit> observer)
            where TView : ILifeTimeContext
        {
            return view.Bind(source,x => observer.OnNext(Unit.Default));
        }
        
        public static TView Bind<TView,TValue>(this TView view, IObservable<TValue> source, IObserver<TValue> observer)
            where TView : ILifeTimeContext
        {
            return view.Bind(source, observer.OnNext);
        }
        
        public static TView Bind<TView,TValue>(this TView view, 
            IObservable<TValue> source,
            IReactiveProperty<TValue> value)
            where TView : ILifeTimeContext
        {
            return view.Bind(source, x => value.Value = x);
        }

                
        public static TView Bind<TView>(this TView view, IObservable<bool> source, GameObject asset)
            where TView : ILifeTimeContext
        {
            return !asset ? view : view.Bind(source, asset.SetActive);
        }

        public static TView BindNot<TView>(this TView view, IObservable<bool> source, GameObject asset)
            where TView : ILifeTimeContext
        {
            return !asset ? view : view.Bind(source,x => asset.SetActive(!x));
        }
        
        public static T Bind<T, TValue>(this T sender, IObservable<TValue> source, Action<TValue> action)
            where T : ILifeTimeContext
        {
            return Bind<T,TValue>(sender, source, action, sender.LifeTime);
        }
        
        public static T Bind<T, TValue,TResult>(this T sender, IObservable<TValue> source, Func<TValue,TResult> action)
            where T : ILifeTimeContext
        {
            return Bind(sender, source,x => action(x), sender.LifeTime);
        }
        
        public static T Bind<T, TValue, TFunc>(this T sender, IObservable<TValue> source, Func<TFunc> action)
            where T : ILifeTimeContext
        {
            return Bind<T,TValue>(sender, source,x => action(), sender.LifeTime);
        }

        public static T Bind<T>(this T sender, IDisposable disposable)
            where T : ILifeTimeContext
        {
            sender.LifeTime.AddDispose(disposable);
            return sender;
        }
        
        public static T Bind<T, TValue>(this T sender, IObservable<TValue> source, Action<T,TValue> action)
            where T : ILifeTimeContext
        {
            return Bind<T,TValue>(sender, source, action, sender.LifeTime);
        }
        
        public static T Bind<T, TValue>(this T sender, IObservable<TValue> source, Action action)
            where T : ILifeTimeContext
        {
            return action == null ? sender : Bind<T,TValue>(sender, source, x => action(), sender.LifeTime);
        }
        
        public static TResult BindConvert<TResult,T, TValue>(this T sender,Func<T,TResult> converter, IObservable<TValue> source, Action action)
            where T : ILifeTimeContext
        {
            TResult result = default;
            if (action != null)
                sender = Bind<T,TValue>(sender, source, x => action(), sender.LifeTime);
            return converter == null ? result : converter(sender);
        }
        
        public static TSource BindWhere<TSource,T>(
            this TSource sender,
            IObservable<T> source, 
            Func<bool> predicate,
            Action<T> target)
            where TSource : ILifeTimeContext
        {
            if (predicate != null && predicate())
                sender.Bind(source, target);
            return sender;
        }
        

        
        public static TSource BindCleanUp<TSource>(
            this TSource view, 
            Action target)
            where TSource : ILifeTimeContext
        {
            view.AddCleanUpAction(target);
            return view;
        }
        
        public static TSource BindDispose<TSource>(
            this TSource view,
            IDisposable target)
            where TSource : ILifeTimeContext
        {
            view.AddDisposable(target);
            return view;
        }
        
                
        public static TSource BindLateUpdate<TSource>(
            this TSource view,
            Func<bool> predicate, 
            Action target)
            where TSource : ILifeTimeContext
        {
            var observable = Observable.EveryLateUpdate()
                .Where(x => predicate());
                
            return view.Bind(observable,target);
        }
        
        public static TSource BindIntervalUpdate<TSource>(
            this TSource view,
            TimeSpan interval,
            Action target)
            where TSource : ILifeTimeContext
        {
            var observable = Observable.Interval(interval);
            return view.Bind(observable,target);
        }

        public static TSource BindIntervalUpdate<TSource>(
            this TSource view,
            TimeSpan interval,
            Action target,
            Func<bool> predicate)
            where TSource : ILifeTimeContext
        {
            var observable = Observable.Interval(interval)
                .Where(x => view != null && view.LifeTime.IsTerminated == false && predicate());
            
            return view.Bind(observable,target);
        }
        
        public static TSource BindIntervalUpdate<TSource,TValue>(
            this TSource view,
            TimeSpan interval,
            Func<TValue> source,
            Action<TValue> target,
            Func<bool> predicate = null)
            where TSource : ILifeTimeContext
        {
            var observable = Observable.Interval(interval)
                .Where(x => view.LifeTime.IsTerminated == false && (predicate == null || predicate()))
                .Select(x => source());

            return view.Bind(observable, target);
        }
                
        public static TSource BindIf<TSource,T>(
            this TSource view,
            IObservable<T> source, 
            Func<bool> predicate,
            Action<T> target)
            where TSource : ILifeTimeContext
        {
            if (predicate != null && predicate())
                return view.Bind(source, target);
            return view;
        }
        
        
        #endregion

        #region async
        
        public static TSource Bind<TSource,T,TTaskValue>(
            this TSource view,
            IObservable<T> source, 
            Func<T,UniTask<TTaskValue>> asyncAction)
            where TSource : ILifeTimeContext
        {
            return view.Bind(source, x => asyncAction(x).AttachExternalCancellation(view.LifeTime.CancellationToken).Forget());
        }
        
        public static TSource Bind<TSource,T>(this TSource view,
            IObservable<T> source, 
            Func<T,UniTask> asyncAction)
            where TSource : ILifeTimeContext
        {
            return view.Bind(source, x => asyncAction(x)
                .AttachExternalCancellation(view.LifeTime.CancellationToken)
                .Forget());
        }

        #endregion
        
        
        #region base 
        
        public static TSource BindWhere<TSource,T>(
            this TSource sender,
            IObservable<T> source, 
            Func<bool> predicate,
            Action<T> target,
            ILifeTime lifeTime)
        {
            if (predicate != null && predicate())
                sender.Bind(source, target,lifeTime);
            return sender;
        }

        public static TSource Bind<TSource,TValue>(this TSource view, IEnumerable<TValue> source, Action<TValue> action)
        {
            if (source == null || action == null) return view;

            foreach (var value in source)
                action(value);
            
            return view;
        }
        
        public static T Bind<T, TValue>(this T sender, IObservable<TValue> source, Action<TValue> action, ILifeTime lifeTime)
        {
            if (action == null) return sender;
            source.Subscribe(action)
                .AddTo(lifeTime);
            return sender;
        }
        
        public static T Bind<T, TValue>(this T sender, IObservable<TValue> source, 
            Action<T,TValue> action,
            ILifeTime lifeTime)
        {
            if (action == null) return sender;
            source.Subscribe(x => action(sender,x)).AddTo(lifeTime);
            return sender;
        }
        
        public static T Bind<T, TValue>(this T sender, 
            IObservable<TValue> source, 
            IReactiveCommand<TValue> action,
            ILifeTime lifeTime)
        {
            if (action == null) return sender;
            source.Where(x => action.CanExecute.Value)
                .Subscribe(x => action.Execute(x))
                .AddTo(lifeTime);
            
            return sender;
        }
        
        public static T Bind<T, TValue>(this T sender, IObservable<TValue> source, 
            IReactiveCommand<Unit> action,
            ILifeTime lifeTime)
        {
            if (action == null) return sender;
            
            source.Where(x => action.CanExecute.Value)
                .Subscribe(x => action.Execute(Unit.Default))
                .AddTo(lifeTime);
            
            return sender;
        }

        #endregion
        
    }
}