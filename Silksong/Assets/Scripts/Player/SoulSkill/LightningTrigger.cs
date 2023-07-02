using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CircleCollider2D))]
public class LightningTrigger : MonoBehaviour
{
    [HideInInspector]
    public UnityEvent<HpDamable> makeLightningEvent;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.GetComponent<HpDamable>())
        {
            makeLightningEvent.Invoke(col.GetComponent<HpDamable>());
        }
    }
}
