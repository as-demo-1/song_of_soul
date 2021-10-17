using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SMBEvent : IComparable<SMBEvent>
{
    public abstract void Behaviour();
    public abstract bool IsActive(float previousTimeNormalized, float currentTimeNormalized);
    public abstract bool Next(float previousTimeNormalized);
    public virtual int CompareTo(SMBEvent other)
    {
        return this.FieldToCompare - other.FieldToCompare >= 0 ? 1 : -1;
    }
    public abstract float FieldToCompare { get; } 
}
