using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
public class ClearSignal__ : StateMachineBehaviour
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
}

public class SendMassage : StateMachineBehaviour
{

    public string[] enterEvent;
    public string[] exitEvent;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        foreach(var myEvent in enterEvent) {
            //Debug.Log(myEvent);
            EventCenter<String>.Instance.TiggerEvent(myEvent,null);
        }
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        foreach(var myEvent in exitEvent) {
            EventCenter<String>.Instance.TiggerEvent(myEvent,null);
        }
    }
}
