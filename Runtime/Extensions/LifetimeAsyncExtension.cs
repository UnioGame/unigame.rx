using System;
using Cysharp.Threading.Tasks;
using R3;
using UniGame.Core.Runtime;
using UnityEngine;

public static class LifetimeExtension 
{
    
    public static ILifeTime BindEvent(this ILifeTimeContext lifeTimeContext,Action action,Action cancellationAction)
    {
        return lifeTimeContext.LifeTime.BindEvent(action,cancellationAction);
    }
    
    public static ILifeTime BindEvent<TArg>(this ILifeTimeContext lifeTimeContext,Action<TArg> action,Action<TArg> cancellationAction)
    {
        return lifeTimeContext.LifeTime.BindEvent(action,cancellationAction);
    }
    
    public static ILifeTime BindEvent<TArg,TArg2>(this ILifeTimeContext lifeTimeContext,Action<TArg,TArg2> action,Action<TArg,TArg2> cancellationAction)
    {
        return lifeTimeContext.LifeTime.BindEvent(action,cancellationAction);
    }
    
    public static ILifeTime BindEvent<TArg,TArg2>(this ILifeTime lifeTime,Action<TArg,TArg2> source,Action<TArg,TArg2> listener)
    {
        if (source == null || listener == null || lifeTime.IsTerminated) return lifeTime;
        
        Observable.FromEvent(x => source+=listener,
                x => source-=listener)
            .Subscribe()
            .AddTo(lifeTime);
        
        return lifeTime;
    }
    
    public static ILifeTime BindEvent<TArg>(this ILifeTime lifeTime,Action<TArg> source,Action<TArg> listener)
    {
        if (source == null || listener == null || lifeTime.IsTerminated) return lifeTime;
        
        Observable.FromEvent(x => source+=listener,
                x => source-=listener)
            .Subscribe()
            .AddTo(lifeTime);
        
        return lifeTime;
    }
    
    public static ILifeTime BindEvent(this ILifeTime lifeTime,Action source,Action listener)
    {
        if (source == null || listener == null || lifeTime.IsTerminated) return lifeTime;
        
        Observable.FromEvent(x => source+=listener,
                x => source-=listener)
            .Subscribe()
            .AddTo(lifeTime);
        
        return lifeTime;
    }


}
