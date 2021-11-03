using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HpComponent : ContactComponentBase
{
    public float maxHp;
    public float m_CurrentHp;
    private bool m_IsDead;

    public event Action<ContactObject> OnResurrection;
    public event Action<ContactObject, float> OnIncreaseHp;
    public event Action<ContactObject, float> OnDecreaseHp;
    public event Action<ContactObject> OnDied;
    public event Action<ContactObject> OnAddToFullHp;
    public event Action<ContactObject, float> OnIncreaseMaxHp;
    public event Action<ContactObject, float> OnDecreaseMaxHp;

    //public List<InvulnerableComponent> InvulnerableComponents;

    public override void Initialize(ContactObject contactObject)
    {
        base.Initialize(contactObject);
        Resurrection();
    }

    public void Resurrection()
    {
        m_CurrentHp = maxHp;
        m_IsDead = false;
        OnResurrection?.Invoke(m_ContactObject);
    }

    public void ChangeCurrentHp(float hp)
    {
        if (hp == 0)
            return;
        if (hp > 0)
            OnIncreaseHp?.Invoke(m_ContactObject, hp);        
        m_CurrentHp += hp;
        if (m_CurrentHp >= maxHp)
        {
            m_CurrentHp = maxHp;
            OnAddToFullHp?.Invoke(m_ContactObject);
        }
        //if (!CheckInVulnerable())
        //    return;
        if (hp < 0)
        {
            OnDecreaseHp?.Invoke(m_ContactObject, hp);
            //foreach (var item in InvulnerableComponents)
            //{
            //    item.enableInvulnerability();
            //}
        }
        if (m_CurrentHp <= 0)
        {
            m_CurrentHp = 0;
            m_IsDead = true;
            OnDied?.Invoke(m_ContactObject);
        }
    }

    //bool CheckInVulnerable()
    //{
    //    if (InvulnerableComponents is null)
    //        return true;
    //    foreach (var item in InvulnerableComponents)
    //    {
    //        if (item.Invulnerable)
    //            return false;
    //    }
    //    return true;
    //}

    public void ChangeMaxHp(int hp, bool MeanWhileSetCurrentHpToMax)
    {
        if (hp == 0)
            return;
        maxHp += hp;
        if (hp > 0)
            OnIncreaseMaxHp?.Invoke(m_ContactObject, maxHp);
        else if(hp < 0)
            OnDecreaseMaxHp?.Invoke(m_ContactObject, maxHp);
        if (MeanWhileSetCurrentHpToMax)
            ChangeCurrentHp(hp);
    }
}
