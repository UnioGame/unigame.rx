using System;
using Cysharp.Threading.Tasks;
using UniGame.Core.Runtime.Extension;
using UniRx;

namespace UniModules.UniGame.Rx.Runtime.Operations
{
    public class BatchPlayerTimingObservable<TSource> : IObservable<TSource>, IObserver<TSource>, IDisposable
    {
        private readonly int _frameCount;
        
        private PlayerLoopTiming _timing = PlayerLoopTiming.Update;
        private bool _isActive = false;
        private bool _isValueChanged = false;
        private TSource _value;
        private Subject<TSource> _observable = new Subject<TSource>();
        private CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public BatchPlayerTimingObservable(IObservable<TSource> source,int frameCount, PlayerLoopTiming timing = PlayerLoopTiming.Update) 
        {
            _frameCount = frameCount;
            _timing = timing;

            source.Subscribe(this)
                .AddTo(_compositeDisposable);
            
            _compositeDisposable.Add(_observable);
        }

        public void OnNext(TSource source)
        {
            
            _isValueChanged = true;
            _value = source;
            
            lock (this)
            {
                if (!_isActive && _observable.HasObservers)
                {
                    _isActive = true;
                    
                    this.AwaitTiming(_timing, _frameCount)
                        .ToObservable()
                        .DoOnCompleted(() =>
                        {
                            _observable.OnNext(_value);
                            _isActive = false;
                        })
                        .Subscribe()
                        .AddTo(_compositeDisposable);
                }
            }
        }
        
        public void OnError(Exception error)
        {
            _observable.OnError(error);
            Dispose();
        }

        public void OnCompleted()
        {
            _observable.OnCompleted();
            Dispose();
        }

        public IDisposable Subscribe(IObserver<TSource> observer)
        {
            var disposable =_compositeDisposable.IsDisposed ?
                Disposable.Empty:
                _observable.Subscribe(observer);
            
            if(_isValueChanged)
                observer.OnNext(_value);
            
            return disposable;
        }

        public void Dispose() => _compositeDisposable.Dispose();

    }

}
