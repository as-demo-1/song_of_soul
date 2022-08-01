using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SMBEvent : IComparable<SMBEvent>, ISMBEventBehaviour, ISMBEventActive //实现IComparable接口，重写CompareTo方法
{
    public abstract void EventActive(MonoBehaviour mono);
    //public abstract bool IsActive(float previousTimeNormalized, float currentTimeNormalized);
    public abstract bool IsActive(float currentTimeNormalized);
  //  public abstract bool Next(float previousTimeNormalized);//是否是下一个event
  //  public abstract bool NotEndYet(float currentTimeNormalized);
    public virtual int CompareTo(SMBEvent other)
    {
        return this.FieldToCompare - other.FieldToCompare >= 0 ? 1 : -1;
    }
    public abstract float FieldToCompare { get; } 
}

public interface ISMBEventActive : ISMBEventBehaviour
{
   // public bool Next(float previousTimeNormalized);
    //public bool IsActive(float previousTimeNormalized, float currentTimeNormalized);
    public bool IsActive(float  currentTimeNormalized);
   // public bool NotEndYet(float currentTimeNormalized);
}

public interface ISMBEventBehaviour
{
    public void EventActive(MonoBehaviour mono);
}

public abstract class SMBEventTimeStamp : SMBEvent
{
    [Range(0, 1)]
    public float timeNormalized;

    public override bool IsActive(float currentTimeNormalized)
    {
        //return timeNormalized > previousTimeNormalized && timeNormalized <= currentTimeNormalized;
        return this.timeNormalized <= currentTimeNormalized;
    }

    public override float FieldToCompare => timeNormalized;
}
