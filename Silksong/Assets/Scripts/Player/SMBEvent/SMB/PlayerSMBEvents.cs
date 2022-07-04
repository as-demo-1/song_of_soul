using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerSMBEvents : EventSMB
{

    [SerializeField]
    private SMBEventList<SMBStateData> m_SMBStateData;

    [SerializeField]
    private SMBEventList<SMBStatusData> m_SMBStatusData;


    [SerializeField]
    private SMBEventList<SMBWwiseData> m_SMBWwiseData;



    public override void OnStart(Animator animator)//将在mono.start时调用
    {
        m_SMBStateData.reInvokeWhenLoop = false;
        m_SMBStatusData.reInvokeWhenLoop = false;

        m_SMBEventLists.Add(m_SMBStateData);
        m_SMBEventLists.Add(m_SMBStatusData);
        m_SMBEventLists.Add(m_SMBWwiseData);

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


