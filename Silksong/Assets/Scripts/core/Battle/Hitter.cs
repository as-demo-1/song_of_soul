using System;
using System.Collections;
using System.Collections.Generic;using UnityEditor;
//using UnityEditor.PackageManager.Requests;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(Collider2D))]
public abstract class Hitter : MonoBehaviour
{
    protected int _atk = 0;
    public float _atkDistance = int.MaxValue;
    [HideInInspector]
    public BattleEventType m_eventType;

    public void RefreshAtk(int atk)
    {
        _atk = atk;
    }

    public virtual void TiggerAtkEvent()
    {
        EventCenter<BattleEventType>.Instance.TiggerEvent(m_eventType, this);
    }
    public abstract bool AtkPerTarget(HpDamable target);
    protected abstract bool IsAtkSuccess(HpDamable target);
}
