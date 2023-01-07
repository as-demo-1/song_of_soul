using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearSignal : StateMachineBehaviour
{
    public string[] clearAtEnter;
    public string[] clearAtExit;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
        foreach(var signal in clearAtEnter){
            animator.ResetTrigger(signal);
        }
    }
    
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        foreach(var signal in clearAtExit) {
            animator.ResetTrigger(signal);
        }
    }
}
