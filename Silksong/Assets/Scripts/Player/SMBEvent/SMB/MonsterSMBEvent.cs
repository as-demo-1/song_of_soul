using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
public class MonsterSMBEvent : EventSMB
{
    [SerializeField]
    private SMBEventList<SMBWwiseData> m_SMBWwiseData;
    public override void OnStart(Animator animator)//将在mono.start时调用
    {
        m_SMBEventLists.Add(m_SMBWwiseData);

        foreach (var SMBEventList in m_SMBEventLists)
        {
            SMBEventList.Sort();
        }
    }

    public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
    {

        AnimatorClipInfo cilpInfo = animator.GetCurrentAnimatorClipInfo(0)[0];
        m_SMBWwiseData.reInvokeWhenLoop = cilpInfo.clip.isLooping;
        base.OnSLStateEnter(animator, stateInfo, layerIndex, controller);
    }
}
