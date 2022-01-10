using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerSMBEvents : PlayerSMB<MonoBehaviour>
{
    [SerializeField]
    [Range(0f, 1f)]
    private float m_Threshold = 0.1f;
    [SerializeField]
    private SMBEventList<SMBStateData> m_SMBStateData;
    [SerializeField]
    private SMBEventList<SMBStatusData> m_SMBStatusData;    
    [SerializeField]
    private SMBEventList<SMBAttackData> m_SMBAttackData;

    private List<ISMBEventList<SMBEvent>> m_SMBEventLists = new List<ISMBEventList<SMBEvent>>();
    private SMBStateInfo m_SMBStateInfo = new SMBStateInfo();
    protected override void InternalInitialise(Animator animator, MonoBehaviour monoBehaviour)
    {
        base.InternalInitialise(animator, monoBehaviour);
        //Initialise();
    }

    public static void Initialise(Animator animator)
    {
        PlayerSMBEvents[] PlayerSMBEvents = animator.GetBehaviours<PlayerSMBEvents>();

        for (int i = 0; i < PlayerSMBEvents.Length; i++)
        {
            PlayerSMBEvents[i].Initialise();
        }
    }

    public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
    {
        base.OnSLStateEnter(animator, stateInfo, layerIndex, controller);
        UpdateSMBStateInfoNormalizedTime(stateInfo, m_SMBStateInfo);
        foreach (var SMBEventList in m_SMBEventLists)
        {
            CheckTransition(m_SMBStateInfo, SMBEventList);
        }
        UpdateSMBStateInfoLoopCount(m_SMBStateInfo);
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

    private void Initialise()
    {
        m_SMBEventLists.Add(m_SMBStateData);
        m_SMBEventLists.Add(m_SMBStatusData);
        m_SMBEventLists.Add(m_SMBAttackData);
        foreach (var SMBEventList in m_SMBEventLists)
        {
            SMBEventList.Sort();
        }
    }

    private void UpdateSMBStateInfoNormalizedTime(AnimatorStateInfo stateInfo, SMBStateInfo smbStateInfo) 
        => smbStateInfo.UpdateNormalizedTimeInfo(stateInfo);

    private void UpdateSMBStateInfoLoopCount(SMBStateInfo smbStateInfo) 
        => smbStateInfo.LoopCount = smbStateInfo.NormalizedTimeLoopCount;

    private void UpdateEvents(SMBStateInfo smbStateInfo, ISMBEventList<SMBEvent> smbEvents)
    {
        if (smbEvents.Count == 0)
            return;
        if (smbStateInfo.NormalizedTimeLoopCount > smbStateInfo.LoopCount)
        {
            smbEvents.InvokeEvents(smbStateInfo.PreviousNormalizedTime, 1.0f, smbEvents.Index);
            for (int i = 0; i < smbStateInfo.NormalizedTimeLoopCount - smbStateInfo.LoopCount - 1; i++)
            {
                smbEvents.InvokeEvents(Mathf.NegativeInfinity, 1.0f, 0);
            }
            smbEvents.SetIndex(0);
            int newIndex = smbEvents.InvokeEvents(Mathf.NegativeInfinity, smbStateInfo.CurrentNormalizedTime, smbEvents.Index);
            smbEvents.SetIndex(newIndex);
        }
        else
        {
            int newIndex = smbEvents.InvokeEvents(smbStateInfo.PreviousNormalizedTime, smbStateInfo.CurrentNormalizedTime, smbEvents.Index);
            smbEvents.SetIndex(newIndex);
        }
    }

    private void CheckTransition(SMBStateInfo smbStateInfo, ISMBEventList<SMBEvent> smbEvents)
    {
        if (smbEvents.Count == 0)
            return;
        smbEvents.SetIndex(0);
        if (!smbEvents.IgnorePreviousEventsWhenEntered)
        {
            for (int i = 0; i < smbStateInfo.NormalizedTimeLoopCount - smbStateInfo.LoopCount; i++)
            {
                smbEvents.InvokeEvents(Mathf.NegativeInfinity, 1.0f, 0);
            }
        }
        if (smbStateInfo.CurrentNormalizedTime <= m_Threshold)
        {
            smbStateInfo.CurrentNormalizedTime = Mathf.NegativeInfinity;
        }
        else
        {
            smbStateInfo.CurrentNormalizedTime = smbStateInfo.CurrentNormalizedTime;
        }
    }
}

public class SMBStateInfo
{
    //public float firstFrameTime => Time.time - stateInfo.normalizedTime * stateInfo.length;
    //public float previousFrameTime => Time.time - PreviousNormalizedTime * stateInfo.length;
    public AnimatorStateInfo stateInfo;

    public float StateLength => stateInfo.length;
    public float StateNormalizedTime => stateInfo.normalizedTime;
    public float PreviousNormalizedTime { get; set; }
    public float CurrentNormalizedTime { get; set; }
    public int LoopCount { get; set; }
    public int NormalizedTimeLoopCount { get; set; }

    public void UpdateNormalizedTimeInfo(AnimatorStateInfo stateInfo)
    {
        this.stateInfo = stateInfo;
        this.PreviousNormalizedTime = this.CurrentNormalizedTime;
        this.CurrentNormalizedTime = stateInfo.normalizedTime % 1.0f;
        this.NormalizedTimeLoopCount = Mathf.FloorToInt(stateInfo.normalizedTime / 1f);
    }
}
