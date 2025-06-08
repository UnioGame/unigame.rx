namespace UniGame.Utils.Runtime
{
    using System;
    using R3;
    using UnityEngine;
    using UnityEngine.Serialization;

    [Serializable]
    public class AnimatorStateTrigger : StateMachineBehaviour
    {
        [FormerlySerializedAs("_stateName")]
        [SerializeField] 
        public string StateName;

        /// <summary>
        /// Parameter - animatorStateInfo.fullPathHash
        /// </summary>
        public Observable<string> ObserveStateEnter => _observeStateEnter;

        /// <summary>
        /// Parameter - animatorStateInfo.fullPathHash
        /// </summary>
        public Observable<string> ObserveStateExit => _observeStateExit;

        private Subject<string> _observeStateEnter = new();
        private Subject<string> _observeStateExit  = new();

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            _observeStateEnter.OnNext(StateName);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
            _observeStateExit.OnNext(StateName);
        }
    }
}