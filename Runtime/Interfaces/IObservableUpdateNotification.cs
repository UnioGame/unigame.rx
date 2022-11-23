namespace UniGame.Core.Runtime.Rx
{
    using System;
    using UniRx;

    public interface IObservableUpdateNotification
    {

        IObservable<Unit> Update { get; }


    }
}
