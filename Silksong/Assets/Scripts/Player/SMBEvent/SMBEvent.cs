using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SMBEvent : IComparable<SMBEvent>, ISMBEventBehaviour, ISMBEventActive
{
    public abstract void EventActive();
    public abstract bool IsActive(float previousTimeNormalized, float currentTimeNormalized);
    public abstract bool Next(float previousTimeNormalized);
    public abstract bool NotEndYet(float currentTimeNormalized);
    public virtual int CompareTo(SMBEvent other)
    {
        return this.FieldToCompare - other.FieldToCompare >= 0 ? 1 : -1;
    }
    public abstract float FieldToCompare { get; } 
}

public interface ISMBEventActive : ISMBEventBehaviour
{
    public bool Next(float previousTimeNormalized);
    public bool IsActive(float previousTimeNormalized, float currentTimeNormalized);
    public bool NotEndYet(float currentTimeNormalized);
}

public interface ISMBEventBehaviour
{
    public void EventActive();
}

public abstract class SMBEventTimeStamp : SMBEvent
{
    [Range(0, 1)]
    public float timeNormalized;
    public override bool Next(float previousTimeNormalized)
    {
        return timeNormalized <= previousTimeNormalized;
    }
    public override bool IsActive(float previousTimeNormalized, float currentTimeNormalized)
    {
        return timeNormalized > previousTimeNormalized && timeNormalized <= currentTimeNormalized;
    }
    public override bool NotEndYet(float currentTimeNormalized)
    {
        return this.timeNormalized <= currentTimeNormalized;
    }
    public override float FieldToCompare => timeNormalized;
}

//public abstract class SMBEventTimeRange : SMBEvent
//{
//    [Range(0, 1)]
//    public float startTimeNormalized;
//    [Range(0, 1)]
//    public float endTimeNormalized;

//    public override bool Next(float previousTimeNormalized)
//    {
//        return this.endTimeNormalized <= previousTimeNormalized;
//    }

//    public override bool IsActive(float previousTimeNormalized, float currentTimeNormalized)
//    {
//        return !(this.startTimeNormalized > currentTimeNormalized || this.endTimeNormalized <= previousTimeNormalized);
//    }

//    public override bool NotEndYet(float currentTimeNormalized)
//    {
//        return this.startTimeNormalized <= currentTimeNormalized;
//    }

//    public override int CompareTo(SMBEvent other)
//    {
//        if (this.FieldToCompare != other.FieldToCompare)
//            return this.FieldToCompare - other.FieldToCompare >= 0 ? 1 : -1;
//        else
//        {
//            if (other is SMBEventTimeRange)
//            {
//                return this.endTimeNormalized - (other as SMBEventTimeRange).endTimeNormalized >= 0 ? 1 : -1;
//            }
//            else
//            {
//                return this.endTimeNormalized - other.FieldToCompare >= 0 ? 1 : -1;
//            }
//        }
//    }
//    public override float FieldToCompare => startTimeNormalized;
//}