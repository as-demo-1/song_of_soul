using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerSMBEvents : PlayerSMB
{

    [SerializeField]
    private SMBEventList<SMBStateData> m_SMBStateData;

    [SerializeField]
    private SMBEventList<SMBStatusData> m_SMBStatusData;


    [SerializeField]
    private SMBEventList<SMBWwiseData> m_SMBWwiseData;


    private List<ISMBEventList> m_SMBEventLists = new List<ISMBEventList>();
    private SMBStateInfo m_SMBStateInfo = new SMBStateInfo();

    public override void OnStart(Animator animator)//将在mono.start时调用
    {
        m_SMBStateData.reInvokeWhenLoop = false;
        m_SMBStatusData.reInvokeWhenLoop = false;
       // m_SMBWwiseData.reInvokeWhenLoop =true;   //set in slEnter 

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
        foreach (var SMBEventList in m_SMBEventLists)
        {
            ResetEventList(m_SMBStateInfo, SMBEventList);
        }
    }

    protected override void OnSLStateActive(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
    {

        base.OnSLStateActive(animator, stateInfo, layerIndex, controller);

        UpdateSMBStateInfoNormalizedTime(stateInfo, m_SMBStateInfo);
        foreach (var SMBEventList in m_SMBEventLists)
        {
            UpdateEvents(m_SMBStateInfo, SMBEventList);
        }
        UpdateSMBStateInfoLoopCount(m_SMBStateInfo);
    }

  

    private void UpdateSMBStateInfoNormalizedTime(AnimatorStateInfo stateInfo, SMBStateInfo smbStateInfo) => smbStateInfo.UpdateNormalizedTimeInfo(stateInfo);

    private void UpdateSMBStateInfoLoopCount(SMBStateInfo smbStateInfo) => smbStateInfo.LoopCount = smbStateInfo.NormalizedTimeLoopCount;

    private void UpdateEvents(SMBStateInfo smbStateInfo, ISMBEventList smbEvents)
    {
        if (smbEvents.Count == 0)
            return;

        if (smbStateInfo.NormalizedTimeLoopCount > smbStateInfo.LoopCount)//动画循环一遍
        {
           int newI= smbEvents.InvokeEvents(1.0f, smbEvents.Index);
           smbEvents.SetIndex(newI);

           if(smbEvents.reInvokeWhenLoop)
           smbEvents.SetIndex(0);

        }

        int newIndex = smbEvents.InvokeEvents(smbStateInfo.CurrentNormalizedTime, smbEvents.Index);
        smbEvents.SetIndex(newIndex);
        
    }

    private void ResetEventList(SMBStateInfo smbStateInfo, ISMBEventList smbEvents)
    {
        if (smbEvents.Count == 0)
            return;
        smbEvents.SetIndex(0);
    }
}

public class SMBStateInfo
{
    //public float firstFrameTime => Time.time - stateInfo.normalizedTime * stateInfo.length;
    //public float previousFrameTime => Time.time - PreviousNormalizedTime * stateInfo.length;
    public AnimatorStateInfo stateInfo;

   // public bool isLoop => stateInfo.;

    public float StateLength => stateInfo.length;
   // public float StateNormalizedTime => stateInfo.normalizedTime;
   // public float PreviousNormalizedTime { get; set; }
    public float CurrentNormalizedTime { get; set; }
    public int LoopCount { get; set; }
    public int NormalizedTimeLoopCount { get; set; }

    public void UpdateNormalizedTimeInfo(AnimatorStateInfo stateInfo)
    {
        this.stateInfo = stateInfo;
        this.CurrentNormalizedTime = stateInfo.normalizedTime % 1.0f;
        this.NormalizedTimeLoopCount = Mathf.FloorToInt(stateInfo.normalizedTime);
    }
}
