using System;
using Cysharp.Threading.Tasks;
using UniModules.UniGame.Rx.Runtime.Operations;
using UniRx;

namespace UniModules.UniGame.Rx.Runtime.Extensions
{
    public static class RxObservableExtensions
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

        public static IObservable<T> BatchPlayerTiming<T>(this IObservable<T> source, int frameCount = 1,PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            return new BatchPlayerTimingObservable<T>(source, frameCount, timing);
        }
    }
}
