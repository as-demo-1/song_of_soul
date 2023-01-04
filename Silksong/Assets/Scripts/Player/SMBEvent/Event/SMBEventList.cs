using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SMBEventList<TSMBEvent> : ISMBEventList//<TSMBEvent>
    where TSMBEvent : SMBEvent
{
    [SerializeField]
    public List<TSMBEvent> m_SMBEventList;
    public int Index { get; private set; }
    public int Count => m_SMBEventList.Count;

    public bool reInvokeWhenLoop { get; set; }
    public int InvokeEvents(float currentNormalizedTime, int index,MonoBehaviour mono)//index:指向下一个未执行的事件
    {
        for (; index < m_SMBEventList.Count; index++)
        {
            //if (m_SMBEventList[index].NotEndYet(currentNormalizedTime))
            if (m_SMBEventList[index].IsActive(currentNormalizedTime))
            {
                //if (m_SMBEventList[index].IsActive(previousNormalizedTime, currentNormalizedTime))
                    m_SMBEventList[index].EventActive(mono);
            }
            else
                break;
        }
        return index;
    }

   /* public void SetIndexByTime(float timeNormalized)
    {
        while (Index < m_SMBEventList.Count)
        {
            if (m_SMBEventList[Index].Next(timeNormalized))
                Index++;
            else
                break;
        }
    }*/

    public void SetIndex(int newIndex) => Index = newIndex;

    public void Sort() => m_SMBEventList.Sort();

}

public interface ISMBEventList//<out T>
{
  //  public bool IgnorePreviousEventsWhenEntered { get; }
    public int Index { get; }
    public int Count { get; }

    public bool reInvokeWhenLoop { get; set; }
    //public int InvokeEvents(float previousNormalizedTime, float currentNormalizedTime, int index);
    public int InvokeEvents(float currentNormalizedTime, int index,MonoBehaviour mono);
   // public void SetIndexByTime(float timeNormalized);
    public void SetIndex(int countAdd);
    public void Sort();
}
