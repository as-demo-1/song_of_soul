using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DamagerComponent : ContactComponentBase
{
    public event Action<ContactObject, ContactObject, float> OnDamageOther;
    public float sendDamageNum;
    public override void Initialize(ContactObject contactObject)
    {
        base.Initialize(contactObject);
        m_ContactObject.OnContactTargetsFactionObject += DamagerContactTargetFactionCallbackFunc;
    }

    void DamagerContactTargetFactionCallbackFunc(ContactObject sender, ContactObject target, ContactObject.EContactFaction targerFaction)
    {
        HpComponent targetHp;
        if (!target.TryGetComponent<HpComponent>(out targetHp))
            return;
        targetHp?.ChangeCurrentHp(sendDamageNum);
        OnDamageOther?.Invoke(sender, target, sendDamageNum);
    }
}