using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowAnimEvent : MonoBehaviour
{
    //public Collider2D attackCollider2D;
    //public ParticleSystem iceEffect;
    public GameObject atkEffect;

    private void OnAtkStart()
    {
        atkEffect.SetActive(true);
    }

    private void OnAtkEnd()
    {
        atkEffect.SetActive(false);
    }
}
