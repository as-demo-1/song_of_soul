using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    public class InstantiateObject : StateMachineBehaviour
    {

        public GameObject target;
        [Range(0, 1)]
        public float spawnAt = 0;
        public bool isChildTransform = true;
        public Vector3 offset;
        public bool destroyOnExit = true;
        GameObject go;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (spawnAt == 0)
                Spawn(animator);
        }

        private void Spawn(Animator animator)
        {
            go = Instantiate(target) as GameObject;
            go.transform.position = animator.transform.position + offset;
            go.transform.rotation = animator.transform.rotation;
            if (isChildTransform) go.transform.parent = animator.transform;
            go.SetActive(true);
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (spawnAt > 0 && stateInfo.normalizedTime >= spawnAt && go == null)
                Spawn(animator);

        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (destroyOnExit && go != null)
                Destroy(go);
            go = null;
        }

        // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}
    }
}