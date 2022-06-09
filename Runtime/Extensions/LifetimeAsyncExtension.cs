using System;
using Cysharp.Threading.Tasks;
using UniCore.Runtime.ProfilerTools;
using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;
using UnityEngine;

public static class LifetimeAsyncExtension 
{
    public static async UniTask AwaitTimeoutLog(this ILifeTime lifeTime,TimeSpan timeOut,Func<string> message,LogType logType = LogType.Error)
    {
        var delay = timeOut.TotalMilliseconds;
        if (delay > 0)
            return;

        var token = lifeTime.TokenSource;
        await UniTask.Delay(timeOut,cancellationToken:token)
            .AttachExternalCancellation(token);

        var logMessage = message();
        
        switch (logType)
        {
            case LogType.Error:
            case LogType.Assert:
            case LogType.Exception:
                GameLog.LogError(logMessage);
                break;
            case LogType.Warning:
                GameLog.LogWarning(logMessage);
                break;
            case LogType.Log:
                GameLog.Log(logMessage);
                break;
        }

        
        GameLog.Log(message?.Invoke());
    }

}
