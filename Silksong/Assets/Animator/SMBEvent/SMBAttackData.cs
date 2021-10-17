using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SMBAttackData : SMBTimeRange
{
    public AttackData attackDamege;
    public override void Behaviour()
    {
        throw new System.NotImplementedException();
    }

}

public abstract class SMBTimeRange : SMBEvent
{
    [Range(0, 1)]
    public float startTimeNormalized;
    [Range(0, 1)]
    public float endTimeNormalized;

    public override bool IsActive(float previousTimeNormalized, float currentTimeNormalized)
    {
        return this.startTimeNormalized <= currentTimeNormalized && this.endTimeNormalized > previousTimeNormalized;
    }

    public override bool Next(float previousTimeNormalized)
    {
        return this.endTimeNormalized <= previousTimeNormalized;
    }
    public override int CompareTo(SMBEvent other)
    {
        if (this.FieldToCompare != other.FieldToCompare)
            return this.FieldToCompare - other.FieldToCompare >= 0 ? 1 : -1;
        else
        {
            if (other is SMBTimeRange)
            {
                return this.endTimeNormalized - (other as SMBTimeRange).endTimeNormalized >= 0 ? 1 : -1;
            }
            else
            {
                return this.endTimeNormalized - other.FieldToCompare >= 0 ? 1 : -1;
            }
        }
    }
    public override float FieldToCompare => startTimeNormalized;
}

public class AttackData { }
