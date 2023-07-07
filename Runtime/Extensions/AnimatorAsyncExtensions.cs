namespace Game.Modules.UnioModules.UniGame.CoreModules.UniGame.Rx.Runtime.Extensions
{
    using System;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    public static class AnimatorAsyncExtensions
    {
        public static async UniTask WaitStateEndAsync(this Animator animator, int stateHash, int layer = 0)
        {
            if (animator == null || !animator.HasState(layer, stateHash)) {
                var nextState = animator.GetNextAnimatorStateInfo(layer);
                return;
            }
            
            animator.SetTrigger(stateHash);

            await animator.WaitForEndAsync(stateHash, layer);
        }

        public static async UniTask WaitForEndAsync(this Animator animator, int stateHash, int layer = 0)
        {
            while (animator == null || animator.GetCurrentAnimatorStateInfo(layer).shortNameHash != stateHash)
                await UniTask.Yield();

            await UniTask.Delay(TimeSpan.FromSeconds(animator.GetCurrentAnimatorStateInfo(layer).length));
        }
    }
}