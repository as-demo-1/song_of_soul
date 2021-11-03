using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SMBEventList<TSMBEvent> : ISMBEventList<TSMBEvent>
    where TSMBEvent : SMBEvent
{
    //[SerializeField]
    private bool m_IgnorePreviousEventsWhenEntered = true;
    [SerializeField]
    private List<TSMBEvent> m_SMBEventList;
    public bool IgnorePreviousEventsWhenEntered => m_IgnorePreviousEventsWhenEntered;
    public int Index { get; private set; }
    public int Count => m_SMBEventList.Count;
    public int InvokeEvents(float previousNormalizedTime, float currentNormalizedTime, int index)
    {
        for (; index < m_SMBEventList.Count; index++)
        {
            if (m_SMBEventList[index].NotEndYet(currentNormalizedTime))
            {
                if (m_SMBEventList[index].IsActive(previousNormalizedTime, currentNormalizedTime))
                    m_SMBEventList[index].EventActive();
            }
            else
                break;
        }
        return index;
    }

    public void SetIndexByTime(float timeNormalized)
    {
        while (Index < m_SMBEventList.Count)
        {
            if (m_SMBEventList[Index].Next(timeNormalized))
                Index++;
            else
                break;
        }
    }

    public void SetIndex(int newIndex) => Index = newIndex;

    public void Sort() => m_SMBEventList.Sort();

    public static implicit operator List<TSMBEvent>(SMBEventList<TSMBEvent> smbEventList)
        => smbEventList.m_SMBEventList;
}

public interface ISMBEventList<out T>
{
    public bool IgnorePreviousEventsWhenEntered { get; }
    public int Index { get; }
    public int Count { get; }
    public int InvokeEvents(float previousNormalizedTime, float currentNormalizedTime, int index);
    public void SetIndexByTime(float timeNormalized);
    public void SetIndex(int countAdd);
    public void Sort();
}
