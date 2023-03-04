using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveDamager : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        animator.transform.parent.transform.Find("Damager_Vomit").gameObject.SetActive(true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        animator.transform.parent.transform.Find("Damager_Vomit").gameObject.SetActive(false);
    }
}
