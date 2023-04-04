using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class MonsterSMBEvents : EventSMB
{

    [SerializeField]
    private SMBEventList<SMBWwiseData> m_SMBWwiseData;
    [SerializeField]
    private SMBEventList<SMBEffectData> m_SMBEffectData;


    public override void OnStart(Animator animator)//将在mono.start时调用
    {
        m_SMBEffectData.reInvokeWhenLoop = false;

        m_SMBEventLists.Add(m_SMBWwiseData);
        m_SMBEventLists.Add(m_SMBEffectData);

        foreach (var SMBEventList in m_SMBEventLists)
        {
            SMBEventList.Sort();
        }
    }

    public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
    {

        AnimatorClipInfo cilpInfo = animator.GetCurrentAnimatorClipInfo(0)[0];
        /*if (stateInfo.IsName("JumpUp")) 
        Debug.Log(cilpInfo.clip.isLooping);*/
        // Debug.Log(cilpInfo.clip.name);
        m_SMBWwiseData.reInvokeWhenLoop = cilpInfo.clip.isLooping;
        base.OnSLStateEnter(animator, stateInfo, layerIndex, controller);
    }
}
