using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ContactComponentBase : MonoBehaviour
{
    protected ContactObject m_ContactObject;

    public virtual void Initialize(ContactObject contactObject)
    {
        this.m_ContactObject = contactObject;
    }
   
}
