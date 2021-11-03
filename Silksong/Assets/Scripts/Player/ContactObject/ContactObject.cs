using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ContactObject : MonoBehaviour
{
    public EContactFaction faction = EContactFaction.None;
    public List<EContactFaction> targetFactions;
    public event Action<ContactObject, ContactObject, EContactFaction> OnContactTargetsFactionObject;
    public event Action<ContactObject, ContactObject> OnContactAnyOtherFactionsObject;
    public event Action<ContactObject, ContactObject> OnContactAnyObject;

    public List<ContactComponentBase> contactComponentBases = new List<ContactComponentBase>();

    private void Awake()
    {
        InitializeContactComponents();
    }

    public void InitializeContactComponents()
    {
        foreach (var item in contactComponentBases)
        {
            item.Initialize(this);
        }
    }

    public void AddContactComponent(ContactComponentBase contactComponent)
    {
        contactComponentBases.Add(contactComponent);
        contactComponent.Initialize(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ContactObject otherContactObject;
        if (!other.TryGetComponent<ContactObject>(out otherContactObject))
            return;
        OnContactAnyObject?.Invoke(this, otherContactObject);
        if (otherContactObject.faction != faction)
            OnContactAnyOtherFactionsObject?.Invoke(this, otherContactObject);
        if (targetFactions.Contains(otherContactObject.faction))
            OnContactTargetsFactionObject?.Invoke(this, otherContactObject, otherContactObject.faction);
    }

    public bool TryGetContactComponent<T>(out T contactComponent)
        where T : ContactComponentBase
    {
        contactComponent = null;
        for (int i = 0; i < contactComponentBases.Count; i++)
        {
            if (contactComponentBases[i] is T)
            {
                contactComponent = contactComponentBases[i] as T;
                return true;
            }
        }
        return false;
    }

    public bool TryGetContactComponents<T>(out List<T> contactComponents)
        where T : ContactComponentBase
    {
        contactComponents = new List<T>();
        for (int i = 0; i < contactComponentBases.Count; i++)
        {
            if (contactComponentBases[i] is T)
            {
                contactComponents.Add(contactComponentBases[i] as T);
            }
        }
        return contactComponents.Count > 0;
    }

    public enum EContactFaction
    {
        None = 0,
        Player = 1,
        Enemy = 2,
    }
}
