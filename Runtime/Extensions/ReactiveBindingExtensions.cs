namespace UniGame.Runtime.Rx.Runtime.Extensions
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Cysharp.Threading.Tasks;
    using R3;
    using UniGame.Core.Runtime;
    using ReflectionUtils;
    using Rx;
    using UnityEngine;

    public static class ReactiveBindingExtensions
    {
        #region lifetime context
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Bind<TView,TValue>(this TView view, 
            Observable<TValue> source, 
            IObserver<Unit> observer)
            where TView : ILifeTimeContext
        {
            return view.Bind(source,x => observer.OnNext(Unit.Default));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Bind<TView,TValue>(this TView view, Observable<TValue> source, 
            Observer<TValue> observer)
            where TView : ILifeTimeContext
        {
            if (source == null) return view;
            source.Subscribe(observer).AddTo(view.LifeTime);
            return view;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Bind<TView,TValue>(this TView view, Observable<TValue> source, 
            IObserver<TValue> observer)
            where TView : ILifeTimeContext
        {
            if (source == null) return view;
            source.Subscribe(observer.ToObserver()).AddTo(view.LifeTime);
            return view;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Bind<TView,TValue>(this TView view, IObservable<TValue> source, Observer<TValue> observer)
            where TView : ILifeTimeContext
        {
            if (source == null) return view;
            source.ToObservable()
                .Subscribe(observer)
                .AddTo(view.LifeTime);
            return view;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Bind<TView,TValue>(this TView view, IObservable<TValue> source, IObserver<TValue> observer)
            where TView : ILifeTimeContext
        {
            if (source == null) return view;
            source.Subscribe(observer).AddTo(view.LifeTime);
            return view;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Bind<TView,TValue>(this TView view, 
            Observable<TValue> source,
            ReactiveProperty<TValue> value)
            where TView : ILifeTimeContext
        {
            return view.Bind(source, x => value.Value = x);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Bind<TView,TValue>(this TView view, 
            Observable<TValue> source,
            ISubject<TValue> value)
            where TView : ILifeTimeContext
        {
            return view.Bind(source,value.OnNext);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Bind<TView,TValue>(this TView view, 
            IObservable<TValue> source,
            ReactiveProperty<TValue> value)
            where TView : ILifeTimeContext
        {
            return view.Bind(source.ToObservable(),value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Bind<TView,TValue>(this TView view, 
            Observable<TValue> source,
            ReactiveValue<TValue> value)
            where TView : ILifeTimeContext
        {
            return view.Bind(source, x => value.Value = x);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ILifeTime Bind<TValue>(this ILifeTime source, 
            Observable<TValue> observable,
            ReactiveProperty<TValue> value)
        {
            return source.Bind(observable, x => value.Value = x);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ILifeTime Bind<TValue>(this ILifeTime source, 
            Observable<TValue> observable,
            ReactiveValue<TValue> value)
        {
            return source.Bind(observable, x => value.Value = x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Bind<TView, TValue>(this TView view,
            ReactiveValue<TValue> source,
            MethodInfo value)
            where TView : ILifeTimeContext
        {
            return Bind<TView, TValue>(view,
                source.Where(source, static (x, y) => y.HasValue),
                value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Bind<TView,TValue>(this TView view, 
            Observable<TValue> source,
            MethodInfo value)
            where TView : ILifeTimeContext
        {
            if (value == null || source == null) return view;
            
            return view.Bind(source, x =>
            {
                var parameters = value.GetParametersInfo();
                switch (parameters.Length)
                {
                    case > 1:
                        return;
                    case 0:
                        value.Invoke(view,Array.Empty<object>());
                        return;
                    case 1 when parameters[0].ParameterType == typeof(TValue):
                        var args = ArrayPool<object>.Shared.Rent(1);
                        args[0] = x;
                        value.Invoke(view, args);
                        ArrayPool<object>.Shared.Return(args);
                        return;
                }
            });
        }
        
        public static TView Bind<TView>(this TView view, ReactiveValue<bool> source, GameObject asset)
            where TView : ILifeTimeContext
        {
            return !asset ? view : view
                .Bind(source.Where(source,static (x,y) => y.HasValue), asset.SetActive);
        }
        
        public static TView Bind<TView>(this TView view, ReactiveValue<bool> source, IEnumerable<GameObject> assets)
            where TView : ILifeTimeContext
        {
            return view.Bind(source, x =>
            {
                foreach (var gameObject in assets)
                {
                    if(gameObject==null) continue;
                    gameObject.SetActive(x);
                }
            });
        }
        
        public static TView Bind<TView>(this TView view, Observable<bool> source, GameObject asset)
            where TView : ILifeTimeContext
        {
            return !asset ? view : view.Bind(source, asset.SetActive);
        }

        public static TView BindNot<TView>(this TView view, Observable<bool> source, GameObject asset)
            where TView : ILifeTimeContext
        {
            return !asset ? view : view.Bind(source,x => asset.SetActive(!x));
        }
        
        public static TView BindNot<TView>(this TView view, Observable<bool> source, Action<bool> action)
            where TView : ILifeTimeContext
        {
            return view.Bind(source.Select(x => !x),action);
        }
        
        public static TSource Bind<TSource,T>(this TSource view,
            Observable<T> source, 
            Func<T,UniTask> asyncAction)
            where TSource : ILifeTimeContext
        {
            return view.Bind(source, x => asyncAction(x)
                .AttachExternalCancellation(view.LifeTime.Token)
                .Forget());
        }
        
        public static TSource Bind<TSource,T>(this TSource view,
            ReactiveValue<T> source, 
            Func<T,UniTask> asyncAction)
            where TSource : ILifeTimeContext
        {
            return view.Bind(source.Where(source,static (x,y) => y.HasValue), 
                x => asyncAction(x)
                .AttachExternalCancellation(view.LifeTime.Token)
                .Forget());
        }
        
        public static T Bind<T, TValue>(this T sender, ReactiveValue<TValue> source, Action<TValue> action)
            where T : ILifeTimeContext
        {
            return Bind<T,TValue>(sender, 
                source.Where(source,static (x,y) => y.HasValue), 
                action, sender.LifeTime);
        }
        
        public static T Bind<T, TValue>(this T sender, Observable<TValue> source, Action<TValue> action)
            where T : ILifeTimeContext
        {
            return Bind<T,TValue>(sender, source, action, sender.LifeTime);
        }
        
        public static T Bind<T, TValue, TFunc>(this T sender, ReactiveValue<TValue> source, Func<TFunc> action)
            where T : ILifeTimeContext
        {
            return Bind(sender, 
                source.Where(source,static (x,y) => y.HasValue),
                action);
        }

        public static T Bind<T, TValue, TFunc>(this T sender, Observable<TValue> source, Func<TFunc> action)
            where T : ILifeTimeContext
        {
            return Bind<T,TValue>(sender, source,x => action(), sender.LifeTime);
        }
        
        public static T Bind<T, TValue>(this T sender, ReactiveValue<TValue> source, Func<UniTask> action)
            where T : ILifeTimeContext
        {
            return Bind(sender, 
                source.Where(source,static (x,y) => y.HasValue),
                action);
        }
        
        public static T Bind<T, TValue>(this T sender, Observable<TValue> source, Func<UniTask> action)
            where T : ILifeTimeContext
        {
            if(action == null) return sender;
            return Bind(sender, source,x => action().Forget(), sender.LifeTime);
        }

        public static T Bind<T>(this T sender, IDisposable disposable)
            where T : ILifeTimeContext
        {
            sender.LifeTime.AddDispose(disposable);
            return sender;
        }
        
        public static T Bind<T, TValue>(this T sender, Observable<TValue> source, Action<T,TValue> action)
            where T : ILifeTimeContext
        {
            return Bind<T,TValue>(sender, source, action, sender.LifeTime);
        }
        
        public static T Bind<T, TValue>(this T sender, Observable<TValue> source, Action action)
            where T : ILifeTimeContext
        {
            return action == null ? sender : Bind<T,TValue>(sender, source, x => action(), sender.LifeTime);
        }
        
        public static ILifeTime Bind<TValue>(this ILifeTime sender, 
            Observable<TValue> source, Action action)
        {
            return action == null 
                ? sender 
                : Bind(sender, source, x => action());
        }
        
        public static TResult BindConvert<TResult,T, TValue>(this T sender,Func<T,TResult> converter, Observable<TValue> source, Action action)
            where T : ILifeTimeContext
        {
            TResult result = default;
            if (action != null)
                sender = Bind<T,TValue>(sender, source, x => action(), sender.LifeTime);
            return converter == null ? result : converter(sender);
        }
        
        public static TSource BindWhere<TSource,T>(
            this TSource sender,
            Observable<T> source, 
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
            Observable<T> source, 
            Func<bool> predicate,
            Action<T> target)
            where TSource : ILifeTimeContext
        {
            if (predicate != null && predicate())
                return view.Bind(source, target);
            return view;
        }
        
        
        #endregion

        #region base 
        
        public static TSource Bind<TSource,T,TTaskValue>(
            this TSource view,
            ReactiveValue<T> source, 
            Func<T,UniTask<TTaskValue>> asyncAction)
            where TSource : ILifeTimeContext
        {
            return Bind(view,source.Where(source,static (x,y) => y.HasValue),asyncAction);
        }
        
        public static TSource Bind<TSource,T,TTaskValue>(
            this TSource view,
            Observable<T> source, 
            Func<T,UniTask<TTaskValue>> asyncAction)
            where TSource : ILifeTimeContext
        {
            return view.Bind(source, x => asyncAction(x).AttachExternalCancellation(view.LifeTime.Token).Forget());
        }
        
        public static TSource BindWhere<TSource,T>(
            this TSource sender,
            Observable<T> source, 
            Func<bool> predicate,
            Action<T> target,
            ILifeTime lifeTime)
        {
            if (predicate != null && predicate())
                sender.Bind(source, target,lifeTime);
            return sender;
        }

        public static TSource Bind<TSource,TValue>(this TSource view, 
            IEnumerable<TValue> source,
            Action<TValue> action)
        {
            if (source == null || action == null) return view;

            foreach (var value in source)
                action(value);
            
            return view;
        }
        
        public static T Bind<T, TValue>(this T sender, Observable<TValue> source, 
            Action<TValue> action, ILifeTime lifeTime)
        {
            if (action == null) return sender;
            source.Subscribe(action).AddTo(lifeTime);
            return sender;
        }
        
        public static ILifeTime Bind<TValue>(this ILifeTime lifeTime, 
            ReactiveValue<TValue> source, Action<TValue> action)
        {
            return Bind(lifeTime, 
                source.Where(source,static (x,y) => y.HasValue) , action);
        }
        
        public static ILifeTime Bind<TValue>(this ILifeTime lifeTime, 
            Observable<TValue> source, 
            Action<TValue> action)
        {
            if (action == null) return lifeTime;
            source.Subscribe(action).AddTo(lifeTime);
            return lifeTime;
        }
        
        public static ILifeTime Bind<TValue>(this ILifeTime lifeTime, 
            Observable<TValue> source, 
            Func<UniTask> action)
        {
            if (action == null) return lifeTime;
            source.Subscribe(action,(x,y) => 
                    y().AttachExternalCancellation(lifeTime.Token).Forget())
                .AddTo(lifeTime);
            return lifeTime;
        }

        public static ILifeTime Bind<TValue, TTaskValue>(this ILifeTime lifeTime,
            ReactiveValue<TValue> source,
            Func<UniTask<TTaskValue>> action)
        {
            return Bind(lifeTime, source
                .Where(source,static (x,y)=>y.HasValue), action);
        }

        public static ILifeTime Bind<TValue,TTaskValue>(this ILifeTime lifeTime, 
            Observable<TValue> source, 
            Func<UniTask<TTaskValue>> action)
        {
            if (action == null) return lifeTime;
            
            var token = lifeTime.Token;
            
            source.Subscribe(action,(x,y) 
                    => y().AttachExternalCancellation(token).Forget())
                .AddTo(lifeTime);
            return lifeTime;
        }

        public static ILifeTime Bind<TValue>(this ILifeTime lifeTime,
            ReactiveValue<TValue> source,
            Func<TValue, UniTask> action)
        {
            return Bind(lifeTime, 
                source.Where(source,static (x,y) => y.HasValue),
                action);
        }

        public static ILifeTime Bind<TValue>(this ILifeTime lifeTime, 
            Observable<TValue> source, 
            Func<TValue,UniTask> action)
        {
            if (action == null) return lifeTime;
            
            source.Subscribe(action,(x,y) => y(x)
                    .AttachExternalCancellation(lifeTime.Token)
                    .Forget()).AddTo(lifeTime);
            
            return lifeTime;
        }
        
        public static T Bind<T, TValue>(this T sender, 
            ReactiveValue<TValue> source, 
            Action<T,TValue> action,
            ILifeTime lifeTime)
        {
            if (action == null) return sender;
            
            source.Where(source, static (x,y) => y.HasValue)
                .Subscribe(action,(x,y) => y(sender,x))
                .AddTo(lifeTime);
            
            return sender;
        }
        
        public static T Bind<T, TValue>(this T sender, Observable<TValue> source, 
            Action<T,TValue> action,
            ILifeTime lifeTime)
        {
            if (action == null) return sender;
            source.Subscribe(action,(x,y) => y(sender,x))
                .AddTo(lifeTime);
            return sender;
        }
        
        public static T Bind<T, TValue>(this T sender, 
            Observable<TValue> source, 
            ReactiveCommand<TValue> action,
            ILifeTime lifeTime)
        {
            if (action == null) return sender;
            
            source.Where(action,static (x,y) => y.CanExecute())
                .Subscribe(action,static (x,y) => y.Execute(x))
                .AddTo(lifeTime);
            
            return sender;
        }
        
        public static T Bind<T, TValue>(this T sender,
            Observable<TValue> source, 
            ReactiveCommand<Unit> action,
            ILifeTime lifeTime)
        {
            if (action == null) return sender;
            
            source.Where(action,static (x,y) => y.CanExecute())
                .Subscribe(action,static (x,y) => y.Execute(Unit.Default))
                .AddTo(lifeTime);
            
            return sender;
        }

        #endregion
    }
}