using System;
using System.Collections;
using System.Collections.Generic;using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
abstract public class Hitter : MonoBehaviour
{
    protected int _atk = 0;
    public String m_actionName;
    
    public void RefreshAtk(int atk)
    {
        _atk = atk;
    }
    
    public void AtkTick(List<Hittable> targets)
    {
        EventCenter.Instance.TiggerEvent(m_actionName, this);
    }

    public virtual void AtkPerTarget(Hittable target)
    {
        
    }
}
