using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerSMB: SceneLinkedSMB<PlayerController>
{
  /*  public AnimatorStateInfo PreviousStateInfo { get; private set; }
    public AnimatorStateInfo CurrentStateInfo { get; private set; }
    public AnimatorStateInfo NextStateInfo { get; private set; }*/

    //static protected PlayerAnimatorStatesManager m_PlayerAnimatorStatesManager;

   /* protected override void InternalInitialise(Animator animator, PlayerController monoBehaviour)
    {
        base.InternalInitialise(animator, monoBehaviour);
        //if (m_PlayerAnimatorStatesManager == null)
        //    m_PlayerAnimatorStatesManager = FindObjectOfType<PlayerAnimatorStatesManager>();
    }*/

   /* protected override void OnSLStateActive(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
    {
       // UpdatePlayerStatesInfo(animator, stateInfo, layerIndex, controller);
    }*/

  /*  void UpdatePlayerStatesInfo(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
    {
        AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(layerIndex);
        AnimatorStateInfo next = animator.GetNextAnimatorStateInfo(layerIndex);
        //Debug.Log(current.shortNameHash);
        if(next.shortNameHash!=0)
        Debug.Log(next.shortNameHash);

        if (stateInfo.fullPathHash == current.fullPathHash)
        {
            if (CurrentStateInfo.fullPathHash != current.fullPathHash)
            {
                PreviousStateInfo = CurrentStateInfo;
            }
            CurrentStateInfo = stateInfo;
            NextStateInfo = next;
        }
        else if (stateInfo.fullPathHash == next.fullPathHash)
        {
            PreviousStateInfo = current;
            CurrentStateInfo = stateInfo;
        }
    }*/

        //子类将不再调用一下sealed方法
    public sealed override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

    public sealed override void OnSLTransitionToStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

    public sealed override void OnSLStatePostEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

    public sealed override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

    public sealed override void OnSLStatePreExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

    public sealed override void OnSLTransitionFromStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

    public sealed override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

}
