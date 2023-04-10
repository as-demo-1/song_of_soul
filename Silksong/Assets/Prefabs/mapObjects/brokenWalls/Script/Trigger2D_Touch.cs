using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Trigger2D_Touch : Trigger2DBase
{
    public UnityEvent WhenTriggerEnter;
    public GameObject t;
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
         base.OnTriggerEnter2D(collision);
        if (t!=null&& collision.gameObject == t)
            WhenTriggerEnter?.Invoke();
        else if (t == null)
        {
            WhenTriggerEnter?.Invoke();
        }
    }
     
}
