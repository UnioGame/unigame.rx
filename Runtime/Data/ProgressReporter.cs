namespace UniGame.Core.Runtime.DataStructure
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using ObjectPool;
    using R3;
    using UniGame.Runtime.ObjectPool;
    using UniGame.Runtime.DataFlow;
     
    using UnityEngine;

    public class ProgressReporter : IDisposable
    {
        public float minProgress = 0;
        public float maxProgress = 100;
        public float progress = 0;
        public bool сomplete = false;
        
        private List<ProgressNode> _progress = new();
        private LifeTime _lifeTime = new();
        private List<IProgress<float>> _reporters = new();
        
        public UniTask WaitForComplete()
        {
            return UniTask.WaitWhile(() => this.сomplete != true,
                PlayerLoopTiming.PostLateUpdate,_lifeTime.Token);
        }

        public void AddReporter(IProgress<float> reporter)
        {
            _reporters.Add(reporter);
            reporter.Report(progress);
        }
        
        public IProgress<float> NewProgress()
        {
            var progressNode = ClassPool.Spawn<ProgressNode>();
            progressNode.minProgress = minProgress;
            progressNode.maxProgress = maxProgress;
            progressNode.currentProgress = 0;
            
            _progress.Add(progressNode);
            
            progressNode.Connect(x => UpdateProgress());

            return progressNode;
        }
        
        public void UpdateProgress()
        {
            progress = 0;
            
            foreach (var progressNode in _progress)
                progress += progressNode.currentProgress;

            progress /= _progress.Count;
            сomplete = progress >= maxProgress;

            Report();
        }

        public void Report()
        {
            foreach (var reporter in _reporters)
                reporter.Report(progress);
        }

        public void Dispose()
        {
            _reporters.Clear();
            
            foreach (var progressNode in _progress)
            {
                progressNode.Release();
            }
            
            _progress.Clear();
            _lifeTime.Release();
        }
    }

    public class ProgressNode : IProgress<float>,IDisposable
    {
        public float minProgress = 0;
        public float maxProgress = 100;
        public float currentProgress = 0;
        
        private LifeTime _lifeTime = new ();
        private Subject<float> _progress = new();

        public void Connect(Action<float> progressAction)
        {
            _progress
                .Subscribe(progressAction)
                .AddTo(_lifeTime);
        }
        
        public void Report(float value)
        {
            currentProgress = Mathf.Clamp(value, minProgress, maxProgress);
            _progress.OnNext(value);
        }

        public void Release()
        {
            _lifeTime.Release();
        }

        public void Dispose() => Release();
    }
}