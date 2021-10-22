using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SMBEvent : IComparable<SMBEvent>, ISMBEventBehaviour, ISMBEventActive
{
    public abstract void Behaviour();
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
    public void Behaviour();
}