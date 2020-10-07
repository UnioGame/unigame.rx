using System;
using UniModules.UniGame.Rx.Runtime.Operations;
using UniRx;
using UnityEngine.UI;

namespace UniModules.UniGame.Rx.Runtime.Extensions
{
    public static class ExObservableExtensions
    {
    
        public static IObservable<T> ToChain<T>(this IObservable<T> source, IObservable<Unit> skipSource = null)
        {
            return new ChainObservable<T>(source, skipSource);
        }

        public static IObservable<T> AddToChain<T>(this IObservable<T> source, IObservable<T> other)
        {
            if (source is ChainObservable<T> chainObservable) {
                return chainObservable.Add(other);
            }

            return ToChain(source).AddToChain(other);
        }
    }
}
