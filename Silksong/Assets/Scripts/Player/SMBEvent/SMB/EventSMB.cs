using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public abstract class EventSMB : SceneLinkedSMB<MonoBehaviour>
{

    protected List<ISMBEventList> m_SMBEventLists = new List<ISMBEventList>();
    protected SMBStateInfo m_SMBStateInfo = new SMBStateInfo();

    public override void OnStart(Animator animator)//将在mono.start时调用
    {

    }

    public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
    {

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
            int newI = smbEvents.InvokeEvents(1.0f, smbEvents.Index,m_MonoBehaviour);
            smbEvents.SetIndex(newI);

            if (smbEvents.reInvokeWhenLoop)
                smbEvents.SetIndex(0);

        }

        int newIndex = smbEvents.InvokeEvents(smbStateInfo.CurrentNormalizedTime, smbEvents.Index,m_MonoBehaviour);
        smbEvents.SetIndex(newIndex);

    }

    private void ResetEventList(SMBStateInfo smbStateInfo, ISMBEventList smbEvents)
    {
        if (smbEvents.Count == 0)
            return;
        smbEvents.SetIndex(0);
    }

    //子类将不再调用以下sealed方法
    public sealed override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

    public sealed override void OnSLTransitionToStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

    public sealed override void OnSLStatePostEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

    public sealed override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

    public sealed override void OnSLStatePreExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

    public sealed override void OnSLTransitionFromStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

    public sealed override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
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
